using UnityEngine;

public class TrackHolder : MonoBehaviour
{
    public string midiPath;
    public AudioClip[] guitarRiffClip;
    public AudioSource guitarRiff;

    private NoteManager noteManager;
    private GameObject noteManagerGameObject;

    public void Start()
    {
        noteManagerGameObject = GameObject.Find("NoteManager");
        noteManager = noteManagerGameObject.GetComponent<NoteManager>();
    }

    void Update()
    {
        if (noteManager.levelsBeaten == guitarRiffClip.Length)
        {
            noteManager.levelsBeaten = 0;
        }

        switch (guitarRiff.clip.name)
        {
            case "guitar riff":
                midiPath = "test1.mid";
                guitarRiff.clip = guitarRiffClip[0];
                Debug.Log("Successful switch case");
                break;
            case "battle song finished":
                midiPath = "test.mid";
                guitarRiff.clip = guitarRiffClip[1];
                Debug.Log("Successful switch case");
                break;
            case "hit and run":
                midiPath = "test.mid";
                guitarRiff.clip = guitarRiffClip[2];
                Debug.Log("Successful switch case");
                break;
        }

        guitarRiff.clip = guitarRiffClip[noteManager.levelsBeaten];

        

    }
}
