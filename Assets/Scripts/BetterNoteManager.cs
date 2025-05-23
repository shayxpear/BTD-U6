using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class BetterNoteManager : MonoBehaviour
{
    [Header("Hit Tolerance")]
    [SerializeField] private float hitTolerance; //Based off of seconds
    [SerializeField] private float hitDistance; //Based off of the extra width of the mainCircle

    [Header("BPM")]
    [SerializeField] private float bpm; // Set this in the inspector or calculate from MIDI

    [Header("Canvas Elements")]
    [SerializeField] private RectTransform notebar; // Note bar rectangle will spawn left circles at left edge and right circles at right edge
    [SerializeField] private Image mainCircle; // Main circle for the player to hit the notes with

    [Header("Audio")]
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource miss;
    [SerializeField] private AudioSource cooldown;

    [Header("Crosshair")]
    [SerializeField] private SpriteRenderer crosshairRenderer;

    [Header("Crosshair Sprite Sheets")]
    [SerializeField] private Sprite[] redCrosshairSprites;  // For left notes
    [SerializeField] private Sprite[] blueCrosshairSprites; // For right notes
    [SerializeField] private Sprite[] whiteCrosshairSprites; // For when the song is finished

    [Header("Prefabs")]
    [SerializeField] private GameObject leftNotePrefab;
    [SerializeField] private GameObject rightNotePrefab;
    [SerializeField] private GuitarController guitarController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private PlayerCooldown playerCooldown;
    [SerializeField] private TrackHolder trackHolder;

    [Header("Debug")]
    [SerializeField] private int attempts;
    [SerializeField] public int noteCombo;
    [SerializeField] private int sprite;
    public bool ended;
    public bool startedRiff;
    public bool started = false;
    public int levelsBeaten;
    public bool successfulHit;


    //Temp Vars
    private int tempAttempts; //prevents hardcoding reset for attempts

    //Note Variables
    private readonly List<double> leftNoteTimes = new();
    private readonly List<double> rightNoteTimes = new();
    private readonly List<GameObject> leftNotes = new();
    private readonly List<GameObject> rightNotes = new();
    private readonly Queue<RectTransform> activeLeftNotes = new();
    private readonly Queue<RectTransform> activeRightNotes = new();
    private int leftNoteIndex = 0;
    private int rightNoteIndex = 0;

    //Sides
    private enum SIDE {LEFT_SIDE = 0, RIGHT_SIDE = 1, BOTH_SIDES = 2};
    private SIDE side;

    private bool leftSideCollided;
    private bool rightSideCollided;
    private bool bothSideCollided;

    private double AudioSourceTime { get { return (double)trackHolder.guitarRiff.timeSamples / trackHolder.guitarRiff.clip.frequency; } }

    void Start()
    {
        LoadMidiFile(); //Loads the MIDI File, will need to add an if statement to make sure there is a track available
        tempAttempts = attempts; //should always be the number set in engine
    }

    void Update()
    {
        NoteChecker();
        CollisionCheck();

        

    }
    private void LoadMidiFile()
    {
        leftNoteTimes.Clear();
        rightNoteTimes.Clear();
        MidiFile midiFile = MidiFile.Read($"{Application.streamingAssetsPath}/{trackHolder.midiPath}");
        TempoMap tempoMap = midiFile.GetTempoMap();

        // Get the first tempo event (assuming constant tempo for simplicity)
        var tempoChanges = tempoMap.GetTempoChanges();
        foreach (var tempo in tempoChanges)
        {
            double microsecondsPerQuarterNote = tempo.Value.MicrosecondsPerQuarterNote;
            bpm = 60000000f / (float)microsecondsPerQuarterNote;
            Debug.Log($"Detected BPM: {bpm}");
            break; // Assuming only the first tempo for simplicity
        }

        foreach (Note note in midiFile.GetNotes())
        {
            var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap);
            double noteTime = metricTimeSpan.Minutes * 60 + metricTimeSpan.Seconds + metricTimeSpan.Milliseconds / 1000f;

            if (note.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.F)
            {
                leftNoteTimes.Add(noteTime);
                GameObject g = Instantiate(leftNotePrefab, notebar);
                g.SetActive(false);
                leftNotes.Add(g);
            }
            else if (note.NoteName == Melanchall.DryWetMidi.MusicTheory.NoteName.FSharp)
            {
                rightNoteTimes.Add(noteTime);
                GameObject g = Instantiate(rightNotePrefab, notebar);
                g.SetActive(false);
                rightNotes.Add(g);
            }
        }
    }

    private void NoteChecker()
    {
        if (trackHolder.guitarRiff.isPlaying && !ended)
        {
            // Left Note Spawn Check
            if (leftNoteIndex < leftNoteTimes.Count && AudioSourceTime >= leftNoteTimes[leftNoteIndex])
            {
                SpawnNote(leftNoteIndex++, SIDE.LEFT_SIDE); // Spawn left side note

            }

            // Right Note Spawn Check
            if (rightNoteIndex < rightNoteTimes.Count && AudioSourceTime >= rightNoteTimes[rightNoteIndex])
            {
                SpawnNote(rightNoteIndex++, SIDE.RIGHT_SIDE); // Spawn right side note
            }

            // Move Left Notes
            foreach (RectTransform activeNote in activeLeftNotes)
            {
                activeNote.anchoredPosition += new Vector2(notebar.rect.width / 2 * Time.deltaTime, 0);

            }

            // Move Right Notes
            foreach (RectTransform activeNote in activeRightNotes)
            {
                activeNote.anchoredPosition -= new Vector2(notebar.rect.width / 2 * Time.deltaTime, 0);
            }

            // Left Note Despawn Check
            if (activeLeftNotes.Count > 0 && activeLeftNotes.Peek().anchoredPosition.x > notebar.rect.width / 2)
            {
                activeLeftNotes.Dequeue().gameObject.SetActive(false); // Despawn note
                Miss();
            }

            // Right Note Despawn Check
            if (activeRightNotes.Count > 0 && -1 * activeRightNotes.Peek().anchoredPosition.x > notebar.rect.width / 2)
            {
                activeRightNotes.Dequeue().gameObject.SetActive(false); // Despawn note
                Miss();
            }

            // Player Left Click Check
            if (Input.GetMouseButtonDown(0) && playerController.CanDash && playerCooldown.GetCooldown() == false)
            {

                if (leftSideCollided)
                {
                    activeLeftNotes.Dequeue().gameObject.SetActive(false);
                    Hit();
                }
                else
                {
                    Miss();
                }
            }

            // Player Right Click Check
            if (Input.GetMouseButtonDown(1) && playerController.CanDash && playerCooldown.GetCooldown() == false)
            {
                if (rightSideCollided)
                {
                    activeRightNotes.Dequeue().gameObject.SetActive(false);
                    Hit();
                }
                else
                {
                    Miss();
                }
            }

            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && playerController.CanDash)
            {
                /**
                bool leftHit = CollisionCheck(true);
                bool rightHit = CollisionCheck(false);
                if (leftHit)
                {
                    activeLeftNotes.Dequeue().gameObject.SetActive(false);
                    if (leftHit) { HitDash(); }
                    else { Miss(); }
                }
                if (rightHit)
                {
                    activeRightNotes.Dequeue().gameObject.SetActive(false);
                    if (rightHit) { HitDash(); }
                    else { Miss(); }
                }
                **/
            }
        }
        else if (ended)
        {
            music.volume = 1;
            if (activeLeftNotes.Count > 0)
                activeLeftNotes.Dequeue().gameObject.SetActive(false);
            if (activeRightNotes.Count > 0)
                activeRightNotes.Dequeue().gameObject.SetActive(false);
            trackHolder.guitarRiff.Stop();
        }

        //Checks if the MIDI is done
        if ((leftNoteIndex == leftNotes.Count && rightNoteIndex == rightNotes.Count) && activeLeftNotes.Count == 0 && activeRightNotes.Count == 0)
        {
            started = false;
        }
    }

    private void CollisionCheck()
    {

        foreach (RectTransform note in activeLeftNotes)
        {
            if (Mathf.Abs(note.anchoredPosition.x - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance)
            {
                leftSideCollided = true;
            }
        }

        foreach (RectTransform note in activeRightNotes)
        {
            if (Mathf.Abs((note.anchoredPosition.x * -1) - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance)
            {
                rightSideCollided = true;
            }
        }
        
    }

    private void SpawnNote(int index, SIDE spawnSide)
    {
        RectTransform note;

        switch(spawnSide)
        {
            case SIDE.LEFT_SIDE:
                {
                    note = leftNotes[index].GetComponent<RectTransform>();
                    note.anchoredPosition = new Vector2(0, 0); // Move it to left (anchored to left of parent)
                    activeLeftNotes.Enqueue(note); // Track note on active note queue
                    note.gameObject.SetActive(true); // Make the note visible
                    break;
                }

            case SIDE.RIGHT_SIDE:
                {
                    note = rightNotes[index].GetComponent<RectTransform>();
                    note.anchoredPosition = new Vector2(0, 0); // Move it to right (anchored to right of parent)
                    activeRightNotes.Enqueue(note); // Track note on active note queue
                    note.gameObject.SetActive(true); // Make the note visible
                    break;
                }
        }
    }

    public void StartSong()
    {
        if (started || trackHolder.guitarRiff.isPlaying) return; // Prevent user from starting song if already running

        trackHolder.guitarRiff.Play();
        started = true;
        leftNoteIndex = 0; // Reset left and right node indexes for new run
        rightNoteIndex = 0;
        attempts = tempAttempts;
    }

    /**
    public IEnumerator LeftNoteHitTolerance()
    {
        yield return new WaitForSeconds(hitTolerance);

        if(!successfulHit)
        {
            leftSideCollided = false;
            activeLeftNotes.Dequeue().gameObject.SetActive(false);
        }
        
    }

    public IEnumerator RightNoteHitTolerance()
    {
        yield return new WaitForSeconds(hitTolerance);

        if(!successfulHit)
        {
            rightSideCollided = false;
            activeRightNotes.Dequeue().gameObject.SetActive(false);
        }
        successfulHit = false;
       
    }

    **/
    public void Hit()
    {
        //successfulHit = true;
        guitarController.Shoot();
    }

    public void HitDash()
    {

    }

    public void Miss()
    {
        if (startedRiff)
        {
            attempts--;
            miss.Play();
            noteCombo = 0;
            //successfulHit = false;
        }

        if (attempts <= 0)
        {
            cooldown.Play();
            playerCooldown.StartCooldown();
            while (activeLeftNotes.Count > 0)
            {
                activeLeftNotes.Dequeue().gameObject.SetActive(false);
            }

            while (activeRightNotes.Count > 0)
            {
                activeRightNotes.Dequeue().gameObject.SetActive(false);
            }
            attempts = tempAttempts;
            startedRiff = false;
        }
    }
}
