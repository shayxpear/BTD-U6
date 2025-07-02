using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class BeatManager : MonoBehaviour
{
    //Write a note state function so it is easier to tell what state the notes are in
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

    //Note Information
    /**
    public readonly List<Note> leftNotes = new();
    public readonly List<Note> rightNotes = new();
    public readonly Queue<RectTransform> activeLeftNotes = new();
    public readonly Queue<RectTransform> activeRightNotes = new();
    **/

    public List<Note> notes = new();
    public readonly Queue<Note> activeNotes = new();

    public List<Note> freestyleNotes = new();

    private AudioSource audioSource;

    [Header("Tracks")]
    [SerializeField] public TrackManager[] tracks;

    [Header("DEBUGGING - [C] Clear stage [R] Reset stage [M] Metronome")]
    [SerializeField] private bool turnOnDebugTools;
    private AudioSource metronome;

    public bool clearedStage;
    private bool turnOnMetronome;

    public int currentBPM;

    public int currentTrackIndex;
    public int currentNoteIndex;

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
        switch(songStage)
        {
            case SongStage.INTRO:
                IntroNotes();
                break;
            case SongStage.INTRO_TRANSITION:
                break;
            case SongStage.RIFF:
                NoteChecker();
                InputChecker();
                break;
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
                freestyleNote.gameObject.SetActive(true);
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
                freestyleNote.gameObject.SetActive(true);
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

    public void LoadAllNotesFromTrack()
    {
        foreach(Note note in freestyleNotes)
        {
            notes.Add(note);
        }
    }

    public void NoteSpawner()
    {
        if (turnOnMetronome)
            metronome.Play();

        if (songStage == SongStage.RIFF)
        {
            if (notes[currentNoteIndex].side == Note.SIDE.LEFT_SIDE)
            {
                activeNotes.Enqueue(Instantiate(leftNotePrefab, RhythmModuleTransform));
            }
            else if (notes[currentNoteIndex].side == Note.SIDE.RIGHT_SIDE)
            {
                activeNotes.Enqueue(Instantiate(rightNotePrefab, RhythmModuleTransform));
            }
            currentNoteIndex++;
        }

        if (currentNoteIndex == freestyleNotes.Count) { currentNoteIndex = 0; }
    }

    public void NoteChecker()
    {
        foreach (Note note in activeNotes)
        {
            //Move Active Notes
            if(note.noteState != Note.NoteState.Despawning)
            {
                if (note.noteTransform.anchoredPosition.x < notebar.rect.width / 2 && note.side == Note.SIDE.LEFT_SIDE) { note.noteTransform.anchoredPosition += new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0); }
                if (-note.noteTransform.anchoredPosition.x < notebar.rect.width / 2 && note.side == Note.SIDE.RIGHT_SIDE) { note.noteTransform.anchoredPosition -= new Vector2(notebar.rect.width / 2 * Time.deltaTime / noteTravelTimeSeconds, 0); }
            }

            switch (note.noteState)
            {
                case Note.NoteState.Active:
                    //Check for collision
                    if (note.side == Note.SIDE.LEFT_SIDE)
                    {
                        float distance = Mathf.Abs(note.noteTransform.anchoredPosition.x - (notebar.rect.width / 2));
                        if (distance < mainCircle.rectTransform.rect.width + hitDistance && note.noteState == Note.NoteState.Active)
                        {
                            note.noteState = Note.NoteState.Hittable;
                        }
                    }

                    else
                    {
                        float distance = Mathf.Abs((-note.noteTransform.anchoredPosition.x) - (notebar.rect.width / 2));

                        if (distance < mainCircle.rectTransform.rect.width + hitDistance && note.noteState == Note.NoteState.Active)
                        {
                            note.noteState = Note.NoteState.Hittable;
                        }
                    }
                    break;

                case Note.NoteState.Hittable:
                    // Left Note Despawn Check
                    if (activeNotes.Count > 0)
                    {
                        var posX = note.GetComponent<RectTransform>().anchoredPosition.x;

                        if (posX >= notebar.rect.width / 2 && note.noteState == Note.NoteState.Hittable && note.side == Note.SIDE.LEFT_SIDE)
                        {
                            StartCoroutine(HitTolerance(note));
                        }

                        if (-posX >= notebar.rect.width / 2 && note.noteState == Note.NoteState.Hittable && note.side == Note.SIDE.RIGHT_SIDE)
                        {
                            StartCoroutine(HitTolerance(note));
                        }
                    }
                    break;
            }
            
        }

        
    }

    public IEnumerator HitTolerance(Note note)
    {
        yield return new WaitForSeconds(hitTolerance);

        if (note == null || note.gameObject == null)
            yield break;

        if (note.noteState == Note.NoteState.Despawning || note.noteState == Note.NoteState.Dodge || note.noteState == Note.NoteState.Hit || note.noteState == Note.NoteState.Miss)
            yield break; // Already handled

        if (activeNotes.Count > 0 && activeNotes.Peek() == note)
        {
            activeNotes.Dequeue();
        }

        Destroy(note.gameObject);
    }

    public void InputChecker()
    {
        // Guard clauses: skip input if player is in cooldown or dodging
        if (spriteHandler.guitar == PlayerSpriteHandler.Guitar.COOLDOWN ||
            spriteHandler.playerBody == PlayerSpriteHandler.PlayerBody.DODGING_F ||
            spriteHandler.playerBody == PlayerSpriteHandler.PlayerBody.DODGING_B)
            return;

        if (activeNotes.Count == 0)
            return;

        Note note = activeNotes.Peek();

        // Left Shoot
        if (inputHandler.GetLeftShootDown())
        {
            ProcessNoteInput(note, Note.SIDE.LEFT_SIDE);
        }

        // Right Shoot
        if (inputHandler.GetRightShootDown())
        {
            ProcessNoteInput(note, Note.SIDE.RIGHT_SIDE);
        }

        // Dodge
        if (inputHandler.GetDodgeDown() &&
           (inputHandler.getPlayerMovement.x != 0 || inputHandler.getPlayerMovement.y != 0))
        {
            ProcessDodgeInput(note, Note.SIDE.LEFT_SIDE);
        }
    }

    private void ProcessNoteInput(Note note, Note.SIDE inputSide)
    {
        Animator animator = note.GetComponent<Animator>();

        if (note.side != inputSide)
        {
            animator.SetTrigger("Miss");
            note.noteState = Note.NoteState.Miss; // Mark state so coroutine skips
            StartCoroutine(NoteAnimation(animator, note));
            return;
        }

        animator.SetTrigger(note.noteState == Note.NoteState.Hittable ? "Hit" : "Miss");
        note.noteState = note.noteState == Note.NoteState.Hittable ? Note.NoteState.Hit : Note.NoteState.Miss;
        StartCoroutine(NoteAnimation(animator, note));

        activeNotes.Dequeue();
    }

    private void ProcessDodgeInput(Note note, Note.SIDE inputSide)
    {
        Animator animator = note.GetComponent<Animator>();

        if (note.side != inputSide)
        {
            animator.SetTrigger("Miss");
            note.noteState = Note.NoteState.Miss; // Mark state so coroutine skips
            StartCoroutine(NoteAnimation(animator, note));
            return;
        }

        animator.SetTrigger(note.noteState == Note.NoteState.Hittable ? "Dodge" : "Miss");
        note.noteState = note.noteState == Note.NoteState.Hittable ? Note.NoteState.Hit : Note.NoteState.Miss;
        StartCoroutine(NoteAnimation(animator, note));

        activeNotes.Dequeue();
    }


    //---Make this section all inside of the Note script---
    public IEnumerator NoteAnimation(Animator animator, Note activeNote)
    {
        while (true)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string stateName = stateInfo.IsName("Miss") ? "Miss"
                               : stateInfo.IsName("Hit") ? "Hit"
                               : stateInfo.IsName("Dodge") ? "Dodge"
                               : null;

            if (stateName != null)
            {
                // Apply state
                switch (stateName)
                {
                    case "Miss":
                        Miss(activeNote);
                        break;
                    case "Hit":
                        Hit(activeNote);
                        break;
                    case "Dodge":
                        Dodge(activeNote);
                        break;
                }

                if (activeNotes.Count > 0 && activeNotes.Peek() == activeNote)
                {
                    activeNotes.Dequeue();
                }

                yield return new WaitForSeconds(stateInfo.length);
                Destroy(activeNote.gameObject);

                break;
            }

            yield return null;
        }
    }

    private void Hit(Note note)
    {
        note.noteState = Note.NoteState.Hit;
        if (note.side == Note.SIDE.LEFT_SIDE)
            inputHandler.successfulLeftShoot = true;
        else
            inputHandler.successfulRightShoot = true;
    }

    private void Miss(Note note)
    {
        note.noteState = Note.NoteState.Miss;

        if (note.side == Note.SIDE.LEFT_SIDE)
        {
            inputHandler.successfulLeftShoot = false;
        }
        else
        {
            inputHandler.successfulRightShoot = false;
        }
    }

    private void Dodge(Note note)
    {
        note.noteState = Note.NoteState.Dodge;
        inputHandler.successfulDodge = true;
    }

    //------------------------------------------------------
    private void ClearNotes()
    {
        foreach (Note note in notes) { Destroy(note.gameObject); }
        foreach (Note note in freestyleNotes) { Destroy(note.gameObject); }

        notes.Clear();

        activeNotes.Clear();
        freestyleNotes.Clear();

        currentNoteIndex = 0;

        introIndex = 0;
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
        ClearNotes();
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
