using UnityEngine;

public class TrackHolder : MonoBehaviour
{
    public string midiPath;
    public AudioClip[] backgroundSongClip;
    public AudioClip[] guitarRiffClip;

    public AudioSource backgroundSong;
    public AudioSource guitarRiff;

    private BetterNoteManager noteManager;
    private GameObject noteManagerGameObject;

    public void Start()
    {
        noteManagerGameObject = GameObject.Find("NoteManager");
        noteManager = noteManagerGameObject.GetComponent<BetterNoteManager>();
    }

    void Update()
    {
        if (noteManager.levelsBeaten == guitarRiffClip.Length)
        {
            noteManager.levelsBeaten = 0;
        }

        guitarRiff.clip = guitarRiffClip[noteManager.levelsBeaten];
        backgroundSong.clip = backgroundSongClip[noteManager.levelsBeaten];



    }
}
