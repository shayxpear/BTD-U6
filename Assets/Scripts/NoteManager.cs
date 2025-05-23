using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class NoteManager : MonoBehaviour
{
    [Header("Control Panel")]
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
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource missSFX;
    [SerializeField] private AudioSource reloadRefresh;

    [SerializeField] private float fadeDuration = 0.5f; // Duration for fade in/out
    private Coroutine fadeCoroutine;
    private double AudioSourceTime
    {
        get
        {
            return (double)trackHolder.guitarRiff.timeSamples / trackHolder.guitarRiff.clip.frequency;
        }
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject leftNotePrefab;
    [SerializeField] private GameObject rightNotePrefab;
    [SerializeField] private GuitarController guitarController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private PlayerCooldown playerCooldown;
    [SerializeField] private TrackHolder trackHolder;

    private readonly List<double> leftNoteTimes = new();
    private readonly List<double> rightNoteTimes = new();
    private readonly List<GameObject> leftNotes = new();
    private readonly List<GameObject> rightNotes = new();
    private readonly Queue<RectTransform> activeLeftNotes = new();
    private readonly Queue<RectTransform> activeRightNotes = new();
    private int leftNoteIndex = 0;
    private int rightNoteIndex = 0;

    [HideInInspector] public float pulsePhase;

    [Header("BPM")]
    [SerializeField] private float bpm; // Set this in the inspector or calculate from MIDI
    private Coroutine bpmPulseCoroutine;
    private Vector3 originalScale;
    public float pulseAmount = 0.1f;
    public float smoothTime = 0.1f;

    [Header("Debug")]
    [SerializeField] public int noteCombo;
    public bool ended;
    private int sprite;
    //private bool canStartSong;
    public bool startedRiff;
    public bool started = false;
    public int levelsBeaten;

    [Header("Crosshair")]
    [SerializeField] private SpriteRenderer crosshairRenderer;
    
    [Header("Crosshair Sprite Sheets")]
    [SerializeField] private Sprite[] redCrosshairSprites;  // For left notes
    [SerializeField] private Sprite[] blueCrosshairSprites; // For right notes
    [SerializeField] private Sprite[] whiteCrosshairSprites; // For when the song is finished

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

        //BUG: Not suppose to turn the entire ui transparent just the discs and tnd the rhythm bar
        if (playerCooldown.cooldown || !startedRiff)
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

        if (trackHolder.guitarRiff.isPlaying && !ended)
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
        else if(ended)
        {
            music.volume = 1;
            if(activeLeftNotes.Count > 0)
                activeLeftNotes.Dequeue().gameObject.SetActive(false);
            if(activeRightNotes.Count > 0)
                activeRightNotes.Dequeue().gameObject.SetActive(false);
            trackHolder.guitarRiff.Stop();
        }

        if ((leftNoteIndex == leftNotes.Count && rightNoteIndex == rightNotes.Count) && activeLeftNotes.Count == 0 && activeRightNotes.Count == 0)
        {
            started = false;
        }
    }

    private IEnumerator FadeAudio(bool fadeIn)
    {
        float startVolume = fadeIn ? 0f : trackHolder.guitarRiff.volume;
        float targetVolume = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        if (fadeIn && !trackHolder.guitarRiff.isPlaying)
        {
            trackHolder.guitarRiff.volume = 0f;
            trackHolder.guitarRiff.Play();
        }

        while (elapsedTime < fadeDuration)
        {
            trackHolder.guitarRiff.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            music.volume = Mathf.Lerp(1f, 0.5f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        trackHolder.guitarRiff.volume = targetVolume;
        music.volume = 0f;

        if (!fadeIn)
        {
            music.volume = 1f;
            trackHolder.guitarRiff.Stop();
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

    // Call this to start the song and the node spawning after the delay
    public void StartSong()
    {
        if (started || trackHolder.guitarRiff.isPlaying) return; // Prevent user from starting song if already running

        trackHolder.guitarRiff.Play();
        started = true;
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
        // Merge & sort note times
        var sortedNoteTimes = new List<double>(leftNoteTimes);
        sortedNoteTimes.AddRange(rightNoteTimes);
        sortedNoteTimes.Sort();

        sortedNoteTimes.Insert(0,0.0);

        Sprite[] currentSpriteSheet = null;
        int currentSprite = 0;

        // Loop through all note segments
        for (int i = 0; i < sortedNoteTimes.Count - 1; i++)
        {
            double currentNoteTime = sortedNoteTimes[i];
            double nextNoteTime = sortedNoteTimes[i + 1];

            // Wait until the audio reaches the current note time
            yield return new WaitUntil(() => AudioSourceTime >= currentNoteTime || !trackHolder.guitarRiff.isPlaying);

            // If the song ends, reset to white
            if (!trackHolder.guitarRiff.isPlaying)
            {
                ResetToWhiteCrosshair();
                yield break;
            }

            // Use the note time of the next note to decide the color
            if (leftNoteTimes.Contains(nextNoteTime))
            {
                currentSpriteSheet = redCrosshairSprites;
            }
            else if (rightNoteTimes.Contains(nextNoteTime))
            {
                currentSpriteSheet = blueCrosshairSprites;
            }
            else
            {
                currentSpriteSheet = playerUI.crosshairSprites;
            }

            int totalSprites = currentSpriteSheet.Length + 1;
            currentSprite = 0;

            // Update sprite during the segment
            while (AudioSourceTime < nextNoteTime && trackHolder.guitarRiff.isPlaying)
            {
                // Calculate progress and update sprite
                float progress = (float)((AudioSourceTime - currentNoteTime) / (nextNoteTime - currentNoteTime));
                int newSprite = Mathf.Clamp(Mathf.FloorToInt(progress * (totalSprites - 1)), 0, totalSprites - 1);

                if (newSprite != currentSprite)
                {
                    currentSprite = newSprite;
                    crosshairRenderer.sprite = currentSpriteSheet[currentSprite];
                }
                yield return null;
            }

            // If the song ends during the segment, reset to white
            if (!trackHolder.guitarRiff.isPlaying)
            {
                ResetToWhiteCrosshair();
                yield break;
            }
        }

        // Wait until the audio completes playing before resetting to white
        yield return new WaitUntil(() => !trackHolder.guitarRiff.isPlaying);

        ResetToWhiteCrosshair();
    }

    private void ResetToWhiteCrosshair()
    {
        if (whiteCrosshairSprites != null && whiteCrosshairSprites.Length > 0)
        {
            crosshairRenderer.sprite = whiteCrosshairSprites[0];
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