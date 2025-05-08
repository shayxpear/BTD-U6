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
    [SerializeField] private int attempts;

    private int tempAttempts; //prevents hardcoding reset for attempts

    [Header("Pulse Objects")]
    [SerializeField] private RectTransform leftPulseObject;
    [SerializeField] private RectTransform rightPulseObject;
    [SerializeField] private float pulseDistance = 100f; // Distance the objects move outwards
    [SerializeField] private float pulseDuration = 0.2f;

    private Vector2 leftOriginalPos;
    private Vector2 rightOriginalPos;


    [Header("Canvas Elements")]
    [SerializeField] private RectTransform notebar; // Note bar rectangle will spawn left circles at left edge and right circles at right edge
    [SerializeField] private Image mainCircle; // Main circle for the player to hit the notes with

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource missSFX;
    [SerializeField] private AudioSource reloadRefresh;

    [SerializeField] private float fadeDuration = 0.5f; // Duration for fade in/out
    private Coroutine fadeCoroutine;
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
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private PlayerCooldown playerCooldown;

    private readonly List<double> leftNoteTimes = new();
    private readonly List<double> rightNoteTimes = new();
    private readonly List<GameObject> leftNotes = new();
    private readonly List<GameObject> rightNotes = new();
    private readonly Queue<RectTransform> activeLeftNotes = new();
    private readonly Queue<RectTransform> activeRightNotes = new();
    private int leftNoteIndex = 0;
    private int rightNoteIndex = 0;
    
    bool cooldown = false;
    bool ended;
    [HideInInspector] public float pulsePhase;

    [Header("BPM")]
    [SerializeField] private float bpm; // Set this in the inspector or calculate from MIDI
    private Coroutine bpmPulseCoroutine;
    private Vector3 originalScale;
    public float pulseAmount = 0.1f;
    public float smoothTime = 0.1f;

    [Header("Debug")]
    [SerializeField] public int noteCombo;
    private int sprite;
    private bool canStartSong;
    public bool startedRiff;
    public bool started = false;

    [Header("Crosshair")]
    [SerializeField] private SpriteRenderer crosshairRenderer;

    private void Start()
    {
        // Load Midi File on start
        LoadMidiFile();
        originalScale = mainCircle.rectTransform.localScale;

        if (bpmPulseCoroutine != null) StopCoroutine(bpmPulseCoroutine);
        bpmPulseCoroutine = StartCoroutine(BpmPulse());

        leftOriginalPos = leftPulseObject.anchoredPosition;
        rightOriginalPos = rightPulseObject.anchoredPosition;

        tempAttempts = attempts; //should always be the number set in engine
        

    }

    private void Update()
    {
        if(playerCooldown.cooldown || !startedRiff)
        {
            leftNotePrefab.GetComponent<Image>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            rightNotePrefab.GetComponent<Image>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        }
        else
        {
            leftNotePrefab.GetComponent<Image>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            rightNotePrefab.GetComponent<Image>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        


        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            ShootPulse(true);
            ShootPulse(false);// Left click
        }

        if (audioSource.isPlaying && !ended)
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
            if (Input.GetMouseButtonDown(0) && playerController.CanDash && playerCooldown.GetCooldown() == false)
            {
                bool hit = CollisionCheck(true);
                activeLeftNotes.Dequeue().gameObject.SetActive(false);
                //Debug.Log($"Hit = {hit}"); // Output hit true or false on left note

                if(hit) { Hit(); }
                else { Miss(); }
            }

            // Player Right Click Check
            if (Input.GetMouseButtonDown(1) && playerController.CanDash && playerCooldown.GetCooldown() == false)
            {
                bool hit = CollisionCheck(false);
                activeRightNotes.Dequeue().gameObject.SetActive(false);
                //Debug.Log($"Hit = {hit}"); // Output hit true or false on right note

                if (hit) { Hit(); }
                else { Miss(); }
            }

            if((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && playerController.CanDash)
            {
                bool leftHit = CollisionCheck(true);
                bool rightHit = CollisionCheck(false);
                if (leftHit)
                {
                    activeLeftNotes.Dequeue().gameObject.SetActive(false);
                    if (leftHit) { HitDash(); }
                    else { Miss(); }
                }
                if(rightHit)
                {
                    activeRightNotes.Dequeue().gameObject.SetActive(false);
                    if (rightHit) { HitDash(); }
                    else { Miss(); }
                }
            }
        }
        else
        {
            music.volume = 1f;
        }
        Debug.Log(leftNoteIndex);
        if ((leftNoteIndex == leftNotes.Count && rightNoteIndex == rightNotes.Count) && activeLeftNotes.Count == 0 && activeRightNotes.Count == 0)
        {
            started = false;
        }
    }

    private IEnumerator FadeAudio(bool fadeIn)
    {
        float startVolume = fadeIn ? 0f : audioSource.volume;
        float targetVolume = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        if (fadeIn && !audioSource.isPlaying)
        {
            audioSource.volume = 0f;
            audioSource.Play();
        }

        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            music.volume = Mathf.Lerp(1f, 0.5f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;
        music.volume = 0.5f;

        if (!fadeIn)
        {
            music.volume = 1f;
            audioSource.Stop();
        }
    }

    public void Hit()
    {
        guitarController.Shoot();
    }

    public void HitDash()
    {

    }

    public void Miss()
    {
        if(startedRiff)
        {
            attempts--;
            missSFX.Play();
            noteCombo = 0;
            
        }
        if (attempts <= 0)
        {
            reloadRefresh.Play();
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

    // Creates time lists for right notes and left notes from Midi file
    private void LoadMidiFile()
    {
        MidiFile midiFile = MidiFile.Read($"{Application.streamingAssetsPath}/{midiFilePath}");
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

    // Call this to start the song and the node spawning after the delay
    public void StartSong()
    {
        if (started || audioSource.isPlaying) return; // Prevent user from starting song if already running


        started = true;
        ended = false;
        leftNoteIndex = 0; // Reset left and right node indexes for new run
        rightNoteIndex = 0;
        attempts = tempAttempts;
        Invoke(nameof(StartSongHelper), startSongDelaySeconds);
    }

    // Helper function to call after delay
    private void StartSongHelper()
    {
        fadeCoroutine = StartCoroutine(FadeAudio(true));
        StartCoroutine(CrosshairSprite());
        //started = false;
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

    private IEnumerator CrosshairSprite()
    {
        // Sort the note times
        var sortedNoteTimes = new List<double>(leftNoteTimes);
        sortedNoteTimes.AddRange(rightNoteTimes);
        sortedNoteTimes.Sort();



        int spriteIndex = 0;
        int totalSprites = playerUI.crosshairSprites.Length;

        // Loop through the sorted note times
        for (int i = 0; i < sortedNoteTimes.Count - 1; i++)
        {
            double currentNoteTime = sortedNoteTimes[i];
            double nextNoteTime = sortedNoteTimes[i + 1];

            // Wait until current note time arrives
            while (AudioSourceTime < currentNoteTime)
            {
                yield return null;
            }

            // Calculate time between these two notes
            float interval = (float)(nextNoteTime - currentNoteTime);
            float timePerSprite = interval / totalSprites;

            // Cycle through all sprites during this interval
            for (int j = 0; j < totalSprites; j++)
            {
                crosshairRenderer.sprite = playerUI.crosshairSprites[spriteIndex];
                spriteIndex = (spriteIndex + 1) % totalSprites; // Loop back to the first sprite
                yield return new WaitForSeconds(timePerSprite);
            }
        }

        // After all notes, reset to first sprite
        if (sortedNoteTimes.Count > 0)
        {
            // Wait until final note time passes
            double finalNoteTime = sortedNoteTimes[^1]; //Retrieve last note time
            while (AudioSourceTime < finalNoteTime)
            {
                yield return null;
            }

            crosshairRenderer.sprite = playerUI.crosshairSprites[0];
        }
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

    private IEnumerator BpmPulse()
    {
        float pulseInterval = 60f / bpm;

        while (true)
        {
            // Instantly enlarge
            mainCircle.rectTransform.localScale = originalScale * (1f + pulseAmount);
            playerUI.healthBar.transform.localScale = originalScale * (1f + pulseAmount);
            float elapsedTime = 0f;

            // Smooth shrink phase
            while (elapsedTime < pulseInterval)
            {
                
                float t = elapsedTime / pulseInterval;
                startSongDelaySeconds = t;
                float scale = 1f + pulseAmount * (1f - Mathf.SmoothStep(0f, 1f, t));
                mainCircle.rectTransform.localScale = originalScale * scale;
                playerUI.healthBar.transform.localScale = originalScale * scale;
                elapsedTime += Time.deltaTime;
                yield return null;

            }

            // Ensure it's at the original size after shrinking
            mainCircle.rectTransform.localScale = originalScale;
            playerUI.healthBar.transform.localScale = originalScale;
            yield return null;
        }
    }

    public void ShootPulse(bool isLeft)
    {
        StartCoroutine(SmoothPulse(isLeft));
    }

    private IEnumerator SmoothPulse(bool isLeft)
    {
        RectTransform pulseObject = isLeft ? leftPulseObject : rightPulseObject;
        Vector2 originalPos = isLeft ? leftOriginalPos : rightOriginalPos;
        Vector2 targetPos = originalPos + (isLeft ? Vector2.left : Vector2.right) * pulseDistance;

        float elapsedTime = 0f;

        // Smooth pulse out and back
        while (elapsedTime < pulseDuration)
        {
            float t = elapsedTime / pulseDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t <= 0.5f ? t * 2f : (1f - t) * 2f); // Out and back
            pulseObject.anchoredPosition = Vector2.Lerp(originalPos, targetPos, smoothT);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it snaps back
        pulseObject.anchoredPosition = originalPos;
    }
}