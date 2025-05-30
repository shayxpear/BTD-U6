using UnityEngine;

public class TrackHolder : MonoBehaviour
{
    public string midiPath;
    public AudioClip[] backgroundSongClip;
    public AudioClip[] guitarRiffClip;
    public AudioClip[] introClip;

    public AudioSource backgroundSong;
    public AudioSource guitarRiff;
    public AudioSource introRiff;

    private BetterNoteManager noteManager;

    public void Start()
    {
        noteManager = GameObject.Find("NoteManager").GetComponent<BetterNoteManager>();
    }

    void Update()
    {
        if (noteManager.levelsBeaten == guitarRiffClip.Length)
        {
            noteManager.levelsBeaten = 0;
        }

        introRiff.clip = introClip[noteManager.levelsBeaten];
        guitarRiff.clip = guitarRiffClip[noteManager.levelsBeaten];
        backgroundSong.clip = backgroundSongClip[noteManager.levelsBeaten];



    }
}


