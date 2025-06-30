using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class BeatManager : MonoBehaviour
{
    [Header("Hit Tolerance")]
    [SerializeField] public float hitTolerance; //Based off of seconds
    [SerializeField] public float hitDistance; //Based off of the extra width of the mainCircle
    [SerializeField] public float noteTravelTimeSeconds;

    [Header("Stats")]
    [SerializeField] public int attempts;
    [SerializeField] public int noteCombo;

    [Header("Note Prefabs")]
    [SerializeField] private Note leftNotePrefab;
    [SerializeField] private Note rightNotePrefab;

    [Header("Handlers")]
    [SerializeField] private PlayerSpriteHandler spriteHandler;
    [SerializeField] private InputHandler inputHandler;

    [Header("RhythmUI")]
    [SerializeField] private RectTransform RhythmModuleTransform;
    [SerializeField] private RectTransform notebar;
    [SerializeField] private Image mainCircle;

    [Header("FreestyleUI")]
    [SerializeField] private RectTransform FreestyleModuleTransform;

    [Header("AudioClips")]
    [SerializeField] private AudioClip miss;

    //Note Information
    /**
    public readonly List<Note> leftNotes = new();
    public readonly List<Note> rightNotes = new();
    public readonly Queue<RectTransform> activeLeftNotes = new();
    public readonly Queue<RectTransform> activeRightNotes = new();
    **/

    public List<Note> notes = new();

    public readonly Queue<RectTransform> activeLeftNotes = new();
    public readonly Queue<RectTransform> activeRightNotes = new();

    public List<Note> freestyleNotes = new();

    private AudioSource audioSource;

    [Header("Tracks")]
    [SerializeField] public TrackManager[] tracks;

    [Header("DEBUGGING - [C] Clear stage [R] Reset stage [M] Metronome")]
    [SerializeField] private bool turnOnDebugTools;
    private AudioSource metronome;

    public bool clearedStage;
    private bool turnOnMetronome;

    public bool leftSideHittable;
    public bool rightSideHittable;

    public int currentBPM;

    public int currentTrackIndex;
    public int currentNoteIndex;

    public int leftNoteIndex;
    public int rightNoteIndex;

    private int introIndex;

    public SongStage songStage;


    public enum SongStage { INTRO, INTRO_TRANSITION, RIFF, OUTRO, BACKGROUND}

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (tracks == null)
        {
            //grab a default track if no tracks are in the list
        }
        else
        {
            PlayIntro();
            currentBPM = tracks[currentTrackIndex].GetBPM();
        }

        metronome = GameObject.Find("Metronome").GetComponent<AudioSource>();
    }

    public void Update()
    {
        NoteChecker();
        //CollisionCheck();
        //InputChecker();

        if(songStage == SongStage.INTRO)
        {
            IntroNotes();
        }

        if(turnOnDebugTools)
        {
            if(Input.GetKeyDown(KeyCode.C))
            {
                clearedStage = true;
            }

            if (Input.GetKeyDown(KeyCode.R) && songStage == SongStage.BACKGROUND)
            {
                PlayIntro();
            }

            if(Input.GetKeyDown(KeyCode.M))
            {
                turnOnMetronome = !turnOnMetronome;
            }
        }
    }

    public void IntroNotes()
    {

        if (freestyleNotes.Count < tracks[currentTrackIndex].introNoteClips.Length)
        {
            if (inputHandler.GetLeftShootDown())
            {
                Note freestyleNote = Instantiate(leftNotePrefab, FreestyleModuleTransform);
                freestyleNote.noteState = Note.NoteState.Active;
                freestyleNotes.Add(freestyleNote);

                if (introIndex < tracks[currentTrackIndex].introNoteClips.Length)
                {
                    audioSource.clip = tracks[currentTrackIndex].introNoteClips[introIndex];
                    audioSource.Play();
                    introIndex++;
                }
            }

            if (inputHandler.GetRightShootDown())
            {
                Note freestyleNote = Instantiate(rightNotePrefab, FreestyleModuleTransform);
                freestyleNote.noteState = Note.NoteState.Active;
                freestyleNotes.Add(freestyleNote);

                if (introIndex < tracks[currentTrackIndex].introNoteClips.Length)
                {
                    audioSource.clip = tracks[currentTrackIndex].introNoteClips[introIndex];
                    audioSource.Play();
                    introIndex++;
                }
            }

            
            
        }
        else
        {
            StartCoroutine(PlayDelayedRiff());
        }
        
    }

    IEnumerator BPMUpdate()
    {                                                                                                                               
        while (tracks[currentTrackIndex].trackSource.clip == tracks[currentTrackIndex].GetGuitarRiff())
        {
            NoteSpawner();
            yield return new WaitForSeconds((60f - noteTravelTimeSeconds) / currentBPM); //noteTravelTime spawns notes early so it matches BPM
        }
    }

    //Load from trackmanager's note list
    public void LoadAllNotesFromTrack()
    {
        foreach(Note note in freestyleNotes)
        {
            if(note.side == Note.SIDE.LEFT_SIDE)
            {
                Note leftNote = Instantiate(leftNotePrefab, RhythmModuleTransform);
                notes.Add(leftNote);
                leftNote.noteState = Note.NoteState.NotActive;
            }

            if (note.side == Note.SIDE.RIGHT_SIDE)
            {
                Note rightNote = Instantiate(rightNotePrefab, RhythmModuleTransform);
                notes.Add(rightNote);
                rightNote.noteState = Note.NoteState.NotActive;
            }
        }
    }
    public void NoteSpawner()
    {
        if (turnOnMetronome)
            metronome.Play();

        if (currentNoteIndex >= freestyleNotes.Count) //prevent leftover notes
            return;

        if (songStage == SongStage.RIFF)
        {
            if (notes[currentNoteIndex].side == Note.SIDE.LEFT_SIDE)
            {
                SpawnNote(notes[currentNoteIndex]);
            }
            else if (notes[currentNoteIndex].side == Note.SIDE.RIGHT_SIDE)
            {
                SpawnNote(notes[currentNoteIndex]);
            }
            currentNoteIndex++;
        }

        if (currentNoteIndex == freestyleNotes.Count) { currentNoteIndex = 0; }
    }
    private void SpawnNote(Note note)
    {
        RectTransform noteTransform;
        switch (note.side)
        {
            case Note.SIDE.LEFT_SIDE:
                {
                    noteTransform = note.GetComponent<RectTransform>();
                    noteTransform.anchoredPosition = new Vector2(0, 0); // Move it to left (anchored to left of parent)
                    activeLeftNotes.Enqueue(noteTransform);
                    note.noteState = Note.NoteState.Active;
                    break;
                }

            case Note.SIDE.RIGHT_SIDE:
                {
                    noteTransform = note.GetComponent<RectTransform>();
                    noteTransform.anchoredPosition = new Vector2(0, 0); // Move it to right (anchored to right of parent)
                    activeRightNotes.Enqueue(noteTransform);
                    note.noteState = Note.NoteState.Active;

                    break;
                }
        }
    }

    public void NoteChecker()
    {
        if (songStage == SongStage.RIFF)
        {

            //Move Left Notes
            foreach (RectTransform activeNote in activeLeftNotes)
            {
                if (activeNote.anchoredPosition.x < notebar.rect.width / 2)
                    activeNote.anchoredPosition += new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0);
            }
            foreach (RectTransform activeNote in activeRightNotes)
            {
                if (-activeNote.anchoredPosition.x < notebar.rect.width / 2)
                    activeNote.anchoredPosition -= new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0);
            }

            // Left Note Despawn Check
            if (activeLeftNotes.Count > 0)
            {
                var front = activeLeftNotes.Peek();
                if (front.anchoredPosition.x >= notebar.rect.width / 2)
                {
                    
                    StartCoroutine(LeftNoteHitTolerance(front));
                }
            }
            // Right Note Despawn Check
            if (activeRightNotes.Count > 0)
            {
                var front = activeRightNotes.Peek();
                if (-front.anchoredPosition.x >= notebar.rect.width / 2)
                {
                    StartCoroutine(RightNoteHitTolerance(front));
                }
            }

        }
    }
    /**
    public void InputChecker()
    {
        //Checks if riff started, and the guitar is not in cooldown
        if (songStage == SongStage.RIFF && spriteHandler.guitar != PlayerSpriteHandler.Guitar.COOLDOWN && 
            (spriteHandler.playerBody != PlayerSpriteHandler.PlayerBody.DODGING_F || spriteHandler.playerBody != PlayerSpriteHandler.PlayerBody.DODGING_B))
        {
            if (inputHandler.GetLeftShootDown())
            {
                if (leftSideHittable && activeLeftNotes.Count > 0)
                {
                    var note = activeLeftNotes.Dequeue();
                    Animator animator = note.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Hit");
                        StartCoroutine(NoteAnimation(note, animator));
                    }
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
                            StartCoroutine(NoteAnimation(note, animator));
                        }
                    }
                }
            }

            if (inputHandler.GetRightShootDown())
            {
                if (rightSideHittable && activeRightNotes.Count > 0) Hit(SIDE.RIGHT_SIDE); else Miss();
            }

            if (inputHandler.GetDodgeDown() && (inputHandler.getPlayerMovement.x != 0 || inputHandler.getPlayerMovement.y != 0))
            {
                if ((leftSideHittable && activeLeftNotes.Count > 0)) { Dodge(); } else { Miss(); }
            }
        }
    }
    private void Hit()
    {
        if (hitSide == SIDE.LEFT_SIDE)
        {
            inputHandler.successfulLeftShoot = true;
            activeLeftNotes.Dequeue().gameObject.SetActive(false);
        }

        if (hitSide == SIDE.RIGHT_SIDE)
        {
            inputHandler.successfulRightShoot = true;
            activeRightNotes.Dequeue().gameObject.SetActive(false);
        }
    }
    public void Dodge()
    {
        inputHandler.successfulDodge = true;
        activeLeftNotes.Dequeue().gameObject.SetActive(false);
    }

    private void Miss()
    {
        audioSource.PlayOneShot(miss);

        bool hasLeft = activeLeftNotes.Count > 0;
        bool hasRight = activeRightNotes.Count > 0;

        if (!hasLeft && !hasRight) return;

        if (hasLeft && !hasRight)
        {
            inputHandler.successfulLeftShoot = false;
            activeLeftNotes.Dequeue().gameObject.SetActive(false);
            return;
        }

        if (hasRight && !hasLeft)
        {
            inputHandler.successfulRightShoot = false;
            activeRightNotes.Dequeue().gameObject.SetActive(false);
            return;
        }

        // Both are active — pick the closer one to center
        RectTransform left = activeLeftNotes.Peek();
        RectTransform right = activeRightNotes.Peek();

        float centerX = notebar.rect.width / 2f;
        float leftDist = Mathf.Abs(left.anchoredPosition.x - centerX);
        float rightDist = Mathf.Abs(-right.anchoredPosition.x - centerX);

        if (leftDist < rightDist)
        {
            inputHandler.successfulLeftShoot = false;
            activeLeftNotes.Dequeue().gameObject.SetActive(false);
        }
        else
        {
            inputHandler.successfulRightShoot = false;
            activeRightNotes.Dequeue().gameObject.SetActive(false);
        }
    }


    private void CollisionCheck()
    {
        leftSideHittable = false;
        rightSideHittable = false;
        foreach (RectTransform note in activeLeftNotes)
        {
            if ((Mathf.Abs(note.anchoredPosition.x - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance))
            {
                leftSideHittable = true;
                break;
            }
        }

        foreach (RectTransform note in activeRightNotes)
        {
            if ((Mathf.Abs((note.anchoredPosition.x * -1) - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance))
            {
                rightSideHittable = true;
                break;
            }
        }
    }

    
    **/
    /**
    private void ClearNotes()
    {
        foreach (Note note in leftNotes) { note.noteState = Note.NoteState.NotActive; Destroy(note.gameObject); }
        foreach (Note note in rightNotes) { note.noteState = Note.NoteState.NotActive; Destroy(note.gameObject); }
        foreach (Note note in freestyleNotes) { note.noteState = Note.NoteState.NotActive; Destroy(note.gameObject); }

        tracks[currentTrackIndex].GetTrackNotes().Clear();

        leftNotes.Clear();
        rightNotes.Clear();

        activeRightNotes.Clear();
        activeLeftNotes.Clear();
        freestyleNotes.Clear();

        leftSideHittable = false;
        rightSideHittable = false;

        currentNoteIndex = 0;
        leftNoteIndex = 0;
        rightNoteIndex = 0;

        introIndex = 0;
    }
        **/

    public IEnumerator LeftNoteHitTolerance(RectTransform activeNote)
    {
        yield return new WaitForSeconds(hitTolerance);
        if (activeLeftNotes.Count > 0 && activeLeftNotes.Peek() == activeNote)
        {
            leftSideHittable = false;
            activeLeftNotes.Dequeue();
        }
       
    }

    public IEnumerator RightNoteHitTolerance(RectTransform note)
    {
        yield return new WaitForSeconds(hitTolerance);
        if (activeRightNotes.Count > 0 && activeRightNotes.Peek() == note)
        {
            rightSideHittable = false;
            activeRightNotes.Dequeue();
        }

    }


    public void PlayIntro()
    {
        songStage = SongStage.INTRO;
        clearedStage = false;
        tracks[currentTrackIndex].trackSource.clip = tracks[currentTrackIndex].GetIntroRiff();
        tracks[currentTrackIndex].trackSource.Play();
        tracks[currentTrackIndex].trackSource.loop = true;
    }

    public IEnumerator PlayDelayedRiff()
    {
        songStage = SongStage.INTRO_TRANSITION;
        LoadAllNotesFromTrack();
        yield return new WaitForSeconds(60f/currentBPM);
        StartCoroutine(PlayRiff());
    }

    public IEnumerator PlayRiff()
    {
        songStage = SongStage.RIFF;
        tracks[currentTrackIndex].trackSource.clip = tracks[currentTrackIndex].GetGuitarRiff();
        tracks[currentTrackIndex].trackSource.Play();
        tracks[currentTrackIndex].trackSource.loop = true;
        StartCoroutine(BPMUpdate());
        yield return new WaitUntil(() => clearedStage);
        StartCoroutine(PlayOutro());
    }

    public IEnumerator PlayOutro()
    {
        songStage = SongStage.OUTRO;
        //ClearNotes();
        tracks[currentTrackIndex].trackSource.clip = tracks[currentTrackIndex].GetOutroRiff();
        tracks[currentTrackIndex].trackSource.Play();
        tracks[currentTrackIndex].trackSource.loop = false;
        yield return new WaitForSeconds(tracks[currentTrackIndex].GetOutroRiffTime());
        PlayBackgroundSong();
    }

    public void PlayBackgroundSong() 
    {
        songStage = SongStage.BACKGROUND;
        tracks[currentTrackIndex].trackSource.clip = tracks[currentTrackIndex].GetBackgroundSong();
        tracks[currentTrackIndex].trackSource.Play();
        tracks[currentTrackIndex].trackSource.loop = true;
    }
}
