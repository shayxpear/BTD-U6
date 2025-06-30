using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class BetterNoteManager : MonoBehaviour
{

    [Header("Hit Tolerance")]
    [SerializeField] public float hitTolerance; //Based off of seconds
    [SerializeField] private float hitDistance; //Based off of the extra width of the mainCircle

    [Header("BPM")]
    [SerializeField] public float bpm; // Set this in the inspector or calculate from MIDI

    [Header("Canvas Elements")]
    [SerializeField] private RectTransform notebar; // Note bar rectangle will spawn left circles at left edge and right circles at right edge
    [SerializeField] private Image mainCircle; // Main circle for the player to hit the notes with

    [Header("Audio")]
    [SerializeField] private AudioSource miss;
    [SerializeField] private AudioSource cooldown;

    [Header("Note Prefabs")]
    [SerializeField] private GameObject leftNotePrefab;
    [SerializeField] private GameObject rightNotePrefab;

    [Header("Player Prefabs")]
    [SerializeField] private GuitarController guitarController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private CrosshairSpriteController crosshairSpriteController;
    [SerializeField] private PlayerCooldown playerCooldown;

    [Header("Game Prefabs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] public TrackHolder trackHolder;

    [Header("Debug")]
    [SerializeField] private int attempts;
    [SerializeField] public int noteCombo;
    [SerializeField] private float noteTravelTimeSeconds;
    public int levelsBeaten;
    public bool successfulHit;
    [SerializeField] public bool playedIntro; // called in GuitarController
    public bool ended;
    public bool startedRiff; 
    public bool started = false; // called in GuitarController
    [SerializeField] private bool leftSideCollided;
    [SerializeField] private bool rightSideCollided;
    private bool bothSideCollided;
    [SerializeField] private bool isLeftToleranceActive = false;
    [SerializeField] private bool isRightToleranceActive = false;

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
        /**
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
        **/
    }

    private void NoteChecker()
    {
        if (trackHolder.guitarRiff.isPlaying && !ended)
        {
            // Left Note Spawn Check
            if (leftNoteIndex < leftNoteTimes.Count && AudioSourceTime >= leftNoteTimes[leftNoteIndex] - noteTravelTimeSeconds)
            {
                SpawnNote(leftNoteIndex++, SIDE.LEFT_SIDE); // Spawn left side note

            }

            // Right Note Spawn Check
            if (rightNoteIndex < rightNoteTimes.Count && AudioSourceTime >= rightNoteTimes[rightNoteIndex] - noteTravelTimeSeconds)
            {
                SpawnNote(rightNoteIndex++, SIDE.RIGHT_SIDE); // Spawn right side note
            }

            // Move Left Notes
            foreach (RectTransform activeNote in activeLeftNotes)
            {
                if (activeNote.anchoredPosition.x < notebar.rect.width / 2)
                    activeNote.anchoredPosition += new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0);
            }

            // Move Right Notes
            foreach (RectTransform activeNote in activeRightNotes)
            {
                if (-activeNote.anchoredPosition.x < notebar.rect.width / 2)
                    activeNote.anchoredPosition -= new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0);
            }

            // Left Note Despawn Check
            if (activeLeftNotes.Count > 0)
            {
                var front = activeLeftNotes.Peek();
                if (!isLeftToleranceActive && front.anchoredPosition.x >= notebar.rect.width / 2)
                {
                    isLeftToleranceActive = true;
                    StartCoroutine(LeftNoteHitTolerance());
                }
            }
            // Right Note Despawn Check
            if(activeRightNotes.Count > 0)
            {
                var front = activeRightNotes.Peek();
                if (!isRightToleranceActive && -front.anchoredPosition.x >= notebar.rect.width / 2)
                {
                    isRightToleranceActive = true;
                    StartCoroutine(RightNoteHitTolerance());
                }
            }

            // Player Left Click Check
            if (Input.GetMouseButtonDown(0) && playerCooldown.GetCooldown() == false)
            {

                if (leftSideCollided)
                {
                    if (activeLeftNotes.Count > 0)
                    {
                        var note = activeLeftNotes.Dequeue();
                        Animator animator = note.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetTrigger("Hit");
                            StartCoroutine(NoteAnimation(note, animator));
                        }
                    }
                    successfulHit = true;
                }
                else
                {
                    if (activeLeftNotes.Count > 0)
                    {
                        var note = activeLeftNotes.Dequeue();
                        Animator animator = note.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetTrigger("Miss");
                            Debug.Log("Left Early Miss");
                            StartCoroutine(NoteAnimation(note, animator));
                        }
                    }
                }
            }

            // Player Right Click Check
            if (Input.GetMouseButtonDown(1) && playerCooldown.GetCooldown() == false)
            {
                if (rightSideCollided)
                {
                    if (activeRightNotes.Count > 0)
                    {
                        var note = activeRightNotes.Dequeue();
                        Animator animator = note.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetTrigger("Hit");
                            StartCoroutine(NoteAnimation(note, animator));
                        }
                    }
                    successfulHit = true;
                }
                else
                {
                    if (activeRightNotes.Count > 0)
                    {
                        var note = activeRightNotes.Dequeue();
                        Animator animator = note.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetTrigger("Miss");
                            Debug.Log("Right Early Miss");
                            StartCoroutine(NoteAnimation(note, animator));
                        }
                    }
                }
            }

            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)))
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
            if (activeLeftNotes.Count > 0)
                activeLeftNotes.Dequeue().gameObject.SetActive(false);
            if (activeRightNotes.Count > 0)
                activeRightNotes.Dequeue().gameObject.SetActive(false);
            trackHolder.guitarRiff.Stop();

            playedIntro = false;
        }

        //Checks if the MIDI is done
        if ((leftNoteIndex == leftNotes.Count && rightNoteIndex == rightNotes.Count) && activeLeftNotes.Count == 0 && activeRightNotes.Count == 0)
        {
            started = false;
            //successfulHit = false;
        }
    }

    private void CollisionCheck()
    {

        foreach (RectTransform note in activeLeftNotes)
        {
            if ((Mathf.Abs(note.anchoredPosition.x - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance) || isLeftToleranceActive)
            {
                leftSideCollided = true;
            }
            else
            {
                leftSideCollided = false;
            }
        }
        

        foreach (RectTransform note in activeRightNotes)
        {
            if ((Mathf.Abs((note.anchoredPosition.x * -1) - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance) || isRightToleranceActive)
            {
                rightSideCollided = true;
            }
            else
            {
                rightSideCollided = false;
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

        if(!playedIntro)
        {
            trackHolder.introRiff.Play();
            playedIntro = true;
        }

        if(!trackHolder.introRiff.isPlaying)
        {
            trackHolder.guitarRiff.Play();
            started = true;
            leftNoteIndex = 0; // Reset left and right node indexes for new run
            rightNoteIndex = 0;
            attempts = tempAttempts;
            crosshairSpriteController.StartCrosshairCoroutine(
             leftNoteTimes, rightNoteTimes, () => AudioSourceTime
         );
        }
        

    }

    public IEnumerator LeftNoteHitTolerance()
    {
        yield return new WaitForSeconds(hitTolerance);
        if(startedRiff)
        {
            if (activeLeftNotes.Count > 0)
            {
                if (!successfulHit)
                {
                    var note = activeLeftNotes.Dequeue();
                    Animator animator = note.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Miss");
                        StartCoroutine(NoteAnimation(note, animator));
                        Debug.Log("Left Late Miss");
                    }
                }
                
            }
        }
        else
        {
            activeLeftNotes.Dequeue().gameObject.SetActive(false);
        }



        successfulHit = false;
        leftSideCollided = false;
        isLeftToleranceActive = false;
    }

    public IEnumerator RightNoteHitTolerance()
    {
        yield return new WaitForSeconds(hitTolerance);
        if (startedRiff)
        {
            if (activeRightNotes.Count > 0)
            {
                if (!successfulHit)
                {
                    var note = activeRightNotes.Dequeue();
                    Animator animator = note.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Miss");
                        StartCoroutine(NoteAnimation(note, animator));
                        Debug.Log("Left Late Miss");
                    }
                }
            }
        }
        else
        {
            activeRightNotes.Dequeue().gameObject.SetActive(false);
        }
        successfulHit = false;
        rightSideCollided = false;
        isRightToleranceActive = false;
    }

    public IEnumerator NoteAnimation(RectTransform note, Animator animator) //Plays animation before dequeuing note
    {
        while (true)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Miss") || stateInfo.IsName("Hit"))
            {
                if (stateInfo.IsName("Miss"))
                    Miss();
                else if (stateInfo.IsName("Hit"))
                    Hit();

                yield return new WaitForSeconds(stateInfo.length);
                break;
            }
            yield return null;
        }

        note.gameObject.SetActive(false);
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
        if (startedRiff)
        {
            attempts--;
            miss.Play();
            noteCombo = 0;
            successfulHit = false;
        }

        if (attempts <= 0)
        {
            cooldown.Play();
            playerCooldown.StartCooldown();
            attempts = tempAttempts;
            startedRiff = false;
        }
    }
}
