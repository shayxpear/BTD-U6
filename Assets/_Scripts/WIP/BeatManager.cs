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
    [SerializeField] private GameObject leftNotePrefab;
    [SerializeField] private GameObject rightNotePrefab;

    [Header("Extra Components")]
    [SerializeField] private PlayerSpriteHandler spriteHandler;
    [SerializeField] private GuitarController guitarController;

    [Header("RhythmUI")]
    [SerializeField] private RectTransform RhythmModuleTransform;
    [SerializeField] private RectTransform notebar;
    [SerializeField] private Image mainCircle;
    //Note Information
    public readonly List<GameObject> leftNotes = new();
    public readonly List<GameObject> rightNotes = new();
    public readonly Queue<RectTransform> activeLeftNotes = new();
    public readonly Queue<RectTransform> activeRightNotes = new();

    [Header("Tracks")]
    [SerializeField] public TrackManager[] tracks;

    [Header("Metronome (DEBUGGING)")]
    [SerializeField] private bool turnOnMetronome;
    private AudioSource metronome;

    [Header("Debug Tools")]
    public bool clearedStage;

    public bool leftSideHittable;
    public bool rightSideHittable;

    public int currentBPM;

    public int currentTrackIndex;
    public int currentNoteIndex;

    public int leftNoteIndex;
    public int rightNoteIndex;

    public SongStage songStage;

    public enum SongStage { INTRO, RIFF, OUTRO, BACKGROUND}
    private enum SIDE { LEFT_SIDE = 0, RIGHT_SIDE = 1, BOTH_SIDES = 2 };

    private double AudioSourceTime { get { return (double)tracks[currentTrackIndex].GetTrackSource().timeSamples / tracks[currentTrackIndex].GetTrackSource().clip.frequency; } }

    public void Start()
    {
        if (tracks == null)
        {
            //grab a default track if no tracks are in the list
        }
        else
        {
            LoadAllNotesFromtrack();
            currentBPM = tracks[currentTrackIndex].GetBPM();
            StartCoroutine(PlayIntro());

        }

        metronome = GameObject.Find("Metronome").GetComponent<AudioSource>();
    }

    public void Update()
    {
        NoteChecker();
        CollisionCheck();
        InputChecker();
    }

    IEnumerator BPMUpdate()
    {                                                                                                                               
        while (tracks[currentTrackIndex].GetTrackSource().clip == tracks[currentTrackIndex].GetGuitarRiff())
        {
            NoteSpawner();
            yield return new WaitForSeconds((60f - noteTravelTimeSeconds) / currentBPM); //noteTravelTime spawns notes early so it matches BPM
        }
    }

    //Load from trackmanager's note list
    public void LoadAllNotesFromtrack()
    {
        foreach(string note in tracks[currentTrackIndex].GetTrackNotes())
        {
            if(note == "L")
            {
                GameObject leftNote = Instantiate(leftNotePrefab, RhythmModuleTransform);
                leftNote.SetActive(false);
                leftNotes.Add(leftNote);
            }

            if (note == "R")
            {
                GameObject rightNote = Instantiate(rightNotePrefab, RhythmModuleTransform);
                rightNote.SetActive(false);
                rightNotes.Add(rightNote);
            }
        }
    }
    public void NoteSpawner() // checks every BPM
    {
        if (turnOnMetronome) { metronome.Play(); }

        if (songStage == SongStage.RIFF)
        {
            if (tracks[currentTrackIndex].GetTrackNotes()[currentNoteIndex] == "L" && leftNoteIndex < leftNotes.Count)
            {
                SpawnNote(leftNoteIndex++, SIDE.LEFT_SIDE);
            }


            if (tracks[currentTrackIndex].GetTrackNotes()[currentNoteIndex] == "R" && rightNoteIndex < rightNotes.Count)
            {
                SpawnNote(rightNoteIndex++, SIDE.RIGHT_SIDE);
            }
            currentNoteIndex++;
        }

        if(currentNoteIndex == tracks[currentTrackIndex].GetTrackLength())
        {
            currentNoteIndex = 0;
            leftNoteIndex = 0;
            rightNoteIndex = 0;
        }
    }

    public void NoteChecker()
    {
        if (tracks[currentTrackIndex].GetTrackSource().clip == tracks[currentTrackIndex].GetGuitarRiff()) //easier way to call when the riff plays and be able to call the variable in other scripts.
        {

            //Move Left Notes
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

    public void InputChecker()
    {
        //Checks if riff started, and the guitar is not in cooldown
        if (songStage == SongStage.RIFF && spriteHandler.guitar != PlayerSpriteHandler.Guitar.COOLDOWN)
        {
            if(spriteHandler.playerBody != PlayerSpriteHandler.PlayerBody.DODGING_F || spriteHandler.playerBody != PlayerSpriteHandler.PlayerBody.DODGING_B)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (leftSideHittable)
                    {
                        LeftHit();
                    }
                    else
                    {
                        LeftMiss();
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (rightSideHittable)
                    {
                        RightHit();
                    }
                    else
                    {
                        RightMiss();
                    }
                }
            }
        }
    }

    public void LeftHit()
    {
        guitarController.Shoot();
        activeLeftNotes.Dequeue().gameObject.SetActive(false);
    }

    public void LeftMiss()
    {
        activeLeftNotes.Dequeue().gameObject.SetActive(false);
    }

    public void RightHit()
    {
        guitarController.Shoot();
        activeRightNotes.Dequeue().gameObject.SetActive(false);
    }

    public void RightMiss()
    {
        activeRightNotes.Dequeue().gameObject.SetActive(false);
    }

    private void CollisionCheck()
    {

        foreach (RectTransform note in activeLeftNotes)
        {
            if ((Mathf.Abs(note.anchoredPosition.x - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance))
            {
                leftSideHittable = true;
            }
            else
            {
                leftSideHittable = false;
            }

        }


        foreach (RectTransform note in activeRightNotes)
        {
            if ((Mathf.Abs((note.anchoredPosition.x * -1) - (notebar.rect.width / 2)) < mainCircle.rectTransform.rect.width + hitDistance))
            {
                rightSideHittable = true;
            }
            else
            {
                rightSideHittable = false;
            }

        }


    }

    private void SpawnNote(int index, SIDE spawnSide)
    {
        RectTransform note;

        switch (spawnSide)
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

    private void ClearNotes()
    {
        foreach (RectTransform note in activeLeftNotes) { note.gameObject.SetActive(false); }
        foreach (RectTransform note in activeRightNotes) { note.gameObject.SetActive(false); }

        activeRightNotes.Clear();
        activeLeftNotes.Clear();
    }

    public IEnumerator LeftNoteHitTolerance(RectTransform note)
    {
        yield return new WaitForSeconds(hitTolerance);
        if (activeLeftNotes.Count > 0 && activeLeftNotes.Peek() == note)
        {
            leftSideHittable = false;
            activeLeftNotes.Dequeue();
            note.gameObject.SetActive(false);
        }
       
    }

    public IEnumerator RightNoteHitTolerance(RectTransform note)
    {
        yield return new WaitForSeconds(hitTolerance);
        if (activeRightNotes.Count > 0 && activeRightNotes.Peek() == note)
        {
            rightSideHittable = false;
            activeRightNotes.Dequeue();
            note.gameObject.SetActive(false);
        }

    }

    public IEnumerator PlayIntro()
    {
        songStage = SongStage.INTRO;
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetIntroRiff();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = false;
        yield return new WaitForSeconds(tracks[currentTrackIndex].GetIntroRiffTime());
        StartCoroutine(PlayRiff());
    }

    public IEnumerator PlayRiff()
    {
        songStage = SongStage.RIFF;
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetGuitarRiff();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = true;
        StartCoroutine(BPMUpdate());
        yield return new WaitUntil(() => clearedStage);
        StartCoroutine(PlayOutro());
    }

    public IEnumerator PlayOutro()
    {
        songStage = SongStage.OUTRO;
        ClearNotes();
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetOutroRiff();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = false;
        yield return new WaitForSeconds(tracks[currentTrackIndex].GetOutroRiffTime());
        PlayBackgroundSong();
    }

    public void PlayBackgroundSong() 
    {
        songStage = SongStage.BACKGROUND;
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetBackgroundSong();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = true;
    }
}
