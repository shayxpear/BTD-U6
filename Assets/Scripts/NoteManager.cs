using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class NoteManager : MonoBehaviour
{
    [Header("Control Panel")]
    [SerializeField] private string midiFilePath; // Path to midi file inside streaming asset path
    [SerializeField] private float hitTolerance; // Tolerance for collision of hitting note [0 = exact match only, 0.1 = 90% overlap, 0.9 = 10% overlap]
    [SerializeField] private float noteDespawnPastDistance; // Distance for a note to go past hitting the center before despawning
    [SerializeField] private float startSongDelaySeconds; // Delay before starting song after calling StartSong
    [SerializeField] private float noteTravelTimeSeconds; // Seconds the note will travel for

    [Header("Canvas Elements")]
    [SerializeField] private RectTransform notebar; // Note bar rectangle will spawn left circles at left edge and right circles at right edge
    [SerializeField] private Image mainCircle; // Main circle for the player to hit the notes with

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource missSFX;
    private double AudioSourceTime
    {
        get
        {
            return (double)audioSource.timeSamples / audioSource.clip.frequency;
        }
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject leftNotePrefab;
    [SerializeField] private GameObject rightNotePrefab;
    [SerializeField] private GuitarController guitarController;
    [SerializeField] private PlayerController playerController;

    private readonly List<double> leftNoteTimes = new();
    private readonly List<double> rightNoteTimes = new();
    private readonly List<GameObject> leftNotes = new();
    private readonly List<GameObject> rightNotes = new();
    private readonly Queue<RectTransform> activeLeftNotes = new();
    private readonly Queue<RectTransform> activeRightNotes = new();
    private int leftNoteIndex = 0;
    private int rightNoteIndex = 0;
    private bool started = false;
    bool cooldown = false;

    private void Start()
    {
        // Load Midi File on start
        LoadMidiFile();
    }

    private void Update()
    {

        if (audioSource.isPlaying)
        {
            // Left Note Spawn Check
            if (leftNoteIndex < leftNoteTimes.Count && AudioSourceTime >= leftNoteTimes[leftNoteIndex] - noteTravelTimeSeconds)
            {
                SpawnNote(leftNoteIndex++, true); // Spawn left side note
            }

            // Right Note Spawn Check
            if (rightNoteIndex < rightNoteTimes.Count && AudioSourceTime >= rightNoteTimes[rightNoteIndex] - noteTravelTimeSeconds)
            {
                SpawnNote(rightNoteIndex++, false); // Spawn right side note
            }

            // Move Left Notes
            foreach (RectTransform activeNote in activeLeftNotes)
            {
                activeNote.anchoredPosition += new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0);
                
            }

            // Move Right Notes
            foreach (RectTransform activeNote in activeRightNotes)
            {
                activeNote.anchoredPosition -= new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0);
            }

            // Left Note Despawn Check
            if (activeLeftNotes.Count > 0 && activeLeftNotes.Peek().anchoredPosition.x > notebar.rect.width / 2 + noteDespawnPastDistance)
            {
                activeLeftNotes.Dequeue().gameObject.SetActive(false); // Despawn note
                Miss();
            }

            // Right Note Despawn Check
            if (activeRightNotes.Count > 0 && -1 * activeRightNotes.Peek().anchoredPosition.x > notebar.rect.width / 2 + noteDespawnPastDistance)
            {
                activeRightNotes.Dequeue().gameObject.SetActive(false); // Despawn note
                Miss();
            }

            // Player Left Click Check
            if (Input.GetMouseButtonDown(0) && playerController.canDash && guitarController.GetCooldown() == false)
            {
                bool hit = CollisionCheck(true);
                activeLeftNotes.Dequeue().gameObject.SetActive(false);
                Debug.Log($"Hit = {hit}"); // Output hit true or false on left note

                if(hit) { Hit(); }
                else { Miss(); }
            }

            // Player Right Click Check
            if (Input.GetMouseButtonDown(1) && playerController.canDash && guitarController.GetCooldown() == false)
            {
                bool hit = CollisionCheck(false);
                activeRightNotes.Dequeue().gameObject.SetActive(false);
                Debug.Log($"Hit = {hit}"); // Output hit true or false on right note

                if (hit) { Hit(); }
                else { Miss(); }
            }
        }
    }

    public void Hit()
    {
        guitarController.Shoot();
    }

    public void Miss()
    {
        audioSource.Stop();
        missSFX.Play();
        StartCoroutine(guitarController.MissCooldown());
        while (activeLeftNotes.Count > 0)
        {
            activeLeftNotes.Dequeue().gameObject.SetActive(false);
        }

        while (activeRightNotes.Count > 0)
        {
            activeRightNotes.Dequeue().gameObject.SetActive(false);
        }
    }

    
    // Creates time lists for right notes and left notes from Midi file
    private void LoadMidiFile()
    {
        MidiFile midiFile = MidiFile.Read($"{Application.streamingAssetsPath}/{midiFilePath}");
        foreach (Note note in midiFile.GetNotes())
        {
            var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, midiFile.GetTempoMap());
            if (note.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.F) // F note for left
            {
                // Add time to left list
                leftNoteTimes.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);

                // Create gameobject for this note ahead of time
                GameObject g = Instantiate(leftNotePrefab, notebar);
                g.SetActive(false);
                leftNotes.Add(g);
            }
            else if (note.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.FSharp) // FSharp note for right
            {
                // Add time to right list
                rightNoteTimes.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);

                // Create gameobject for this note ahead of time
                GameObject g = Instantiate(rightNotePrefab, notebar);
                g.SetActive(false);
                rightNotes.Add(g);
            }
        }

    }

    // Call this to start the song and the node spawning after the delay
    public void StartSong()
    {
        if (started || audioSource.isPlaying) return; // Prevent user from starting song if already running
        started = true;
        leftNoteIndex = 0; // Reset left and right node indexes for new run
        rightNoteIndex = 0;
        Invoke(nameof(StartSongHelper), startSongDelaySeconds);
    }

    // Helper function to call after delay
    private void StartSongHelper()
    {
        audioSource.Play();
        started = false;
    }
           
    // Spawns a note in
    private void SpawnNote(int index, bool isLeftSide)
    {
        RectTransform note;
        Image noteImage;
        Color noteOpacity;
        if (isLeftSide) // Left side code
        {
            note = leftNotes[index].GetComponent<RectTransform>();
            noteImage = leftNotes[index].GetComponent<Image>();
            noteOpacity = new Color(1,1,1,0);
            note.anchoredPosition = new Vector2(0, 0); // Move it to left (anchored to left of parent)
            activeLeftNotes.Enqueue(note); // Track note on active note queue
            note.gameObject.SetActive(true); // Make the note visible

            StartCoroutine(FadeInNote(noteImage, noteOpacity));

        }
        else // Right side code
        {
            note = rightNotes[index].GetComponent<RectTransform>();
            noteImage = rightNotes[index].GetComponent<Image>();
            noteOpacity = new Color(1, 1, 1, 0);
            note = rightNotes[index].GetComponent<RectTransform>();
            note.anchoredPosition = new Vector2(0, 0); // Move it to right (anchored to right of parent)
            activeRightNotes.Enqueue(note); // Track note on active note queue
            note.gameObject.SetActive(true); // Make the note visible
            StartCoroutine(FadeInNote(noteImage, noteOpacity));
        }
    }
    private IEnumerator FadeInNote(Image noteImage, Color noteOpacity)
    {
        float fadeDuration = noteTravelTimeSeconds; // Duration of the fade-in effect
        float fadeSpeed = 1.0f / fadeDuration; // Speed of fading (from 0 to 1)

        // Gradually increase the alpha from 0 to 1
        while (noteOpacity.a < 1f)
        {
            noteOpacity.a += Time.deltaTime * fadeSpeed;
            noteImage.color = noteOpacity;
            yield return null; // Wait for the next frame
        }

        // Ensure the opacity is exactly 1 when done
        noteOpacity.a = 1f;
        noteImage.color = noteOpacity;
    }

    private bool CollisionCheck(bool isLeftSide)
    {
        if (isLeftSide) // Left side code
        {
            foreach (RectTransform note in activeLeftNotes)
            {
                if (Mathf.Abs(note.anchoredPosition.x - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitTolerance)
                {
                    return true;
                }
            }
        }
        else // Right side code
        {
            foreach (RectTransform note in activeRightNotes)
            {
                if (Mathf.Abs((note.anchoredPosition.x * -1) - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitTolerance)
                {
                    return true;
                }
            }
        }
        return false;
    }
}