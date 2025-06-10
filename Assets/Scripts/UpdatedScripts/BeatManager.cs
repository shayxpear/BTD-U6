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
    [SerializeField] public bool clearedLevel;
    [SerializeField] public bool playingSong; //playing guitar riff
    [SerializeField] public int currentBPM;
    [SerializeField] public int currentTrackIndex;
    [SerializeField] public int currentNoteIndex;
    [SerializeField] public int leftNoteIndex;
    [SerializeField] public int rightNoteIndex;



    private enum SIDE { LEFT_SIDE = 0, RIGHT_SIDE = 1, BOTH_SIDES = 2 };
    private SIDE side;

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
        if(tracks[currentTrackIndex].GetTrackSource().clip == tracks[currentTrackIndex].GetGuitarRiff()) //easier way to call when the riff plays and be able to call the variable in other scripts.
        {
            playingSong = true;

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
        }

        else
        {
            playingSong = false;
        }
    }

    IEnumerator BPMUpdate()
    {
        //Updates every X BPM - problem occurs note travels at the BPM and ends in the maincircle x seconds                                                                                                                                        
        while (tracks[currentTrackIndex].GetTrackSource().clip == tracks[currentTrackIndex].GetGuitarRiff())
        {
            NoteChecker();
            yield return new WaitForSeconds(60f / currentBPM);
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
    public void NoteChecker() // checks every BPM
    {
        if (turnOnMetronome) { metronome.Play(); }

        if (playingSong)
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

    public IEnumerator PlayIntro()
    {
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetIntroRiff();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = false;
        yield return new WaitForSeconds(tracks[currentTrackIndex].GetIntroRiffTime());
        StartCoroutine(PlayRiff());
    }

    public IEnumerator PlayRiff()
    {
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetGuitarRiff();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = true;
        StartCoroutine(BPMUpdate());
        yield return new WaitUntil(() => clearedLevel == true);
        StartCoroutine(PlayOutro());
    }

    public IEnumerator PlayOutro()
    {
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetOutroRiff();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = false;
        yield return new WaitForSeconds(tracks[currentTrackIndex].GetOutroRiffTime());
        PlayBackgroundSong();
    }

    public void PlayBackgroundSong() 
    {
        tracks[currentTrackIndex].GetTrackSource().clip = tracks[currentTrackIndex].GetBackgroundSong();
        tracks[currentTrackIndex].GetTrackSource().Play();
        tracks[currentTrackIndex].GetTrackSource().loop = true;
    }
}
