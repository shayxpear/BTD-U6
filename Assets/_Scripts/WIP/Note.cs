using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class Note : MonoBehaviour
{
    public SIDE side;
    public NoteState noteState;
    public enum SIDE { LEFT_SIDE, RIGHT_SIDE };
    public enum NoteState { 
        Active, Hittable, 
        Hit, Miss, Dodge, 
        Despawning}

    public AudioClip miss;

    private AudioSource audioSource;

    public RectTransform noteTransform;
    private void Start()
    {
        noteTransform = GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        switch (noteState)
        {
            case NoteState.Miss:
                audioSource.PlayOneShot(miss);
                noteState = NoteState.Despawning;
                break;
            case NoteState.Hit:
                noteState = NoteState.Despawning;
                break;
            case NoteState.Dodge:
                noteState = NoteState.Despawning;
                break;
        }
    }
}
