using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Track Name")]
    public string trackName;

    [Header("Intro Notes")]
    public AudioClip[] introNoteClips;

    [HideInInspector] public AudioSource trackSource;

    [Header("Intervals")]
    [SerializeField] private int BPM;

    [Header("AudioClips")]
    [SerializeField] private AudioClip introRiff;
    [SerializeField] private AudioClip guitarRiff;
    [SerializeField] private AudioClip outroRiff;
    [SerializeField] private AudioClip backgroundSong;


    private List<string> notes = new(); //L = left R = right

    private void Start()
    {
        trackSource = GetComponent<AudioSource>();
    }

    //Getters
    public int GetBPM() { return BPM; }

    public AudioClip GetIntroRiff() { return introRiff; }

    public float GetIntroRiffTime() { return introRiff.length; }

    public AudioClip GetGuitarRiff() { return guitarRiff; }

    public AudioClip GetOutroRiff() { return outroRiff; }
    public float GetOutroRiffTime() { return outroRiff.length; }

    public AudioClip GetBackgroundSong() { return backgroundSong; }

    public List<string> GetTrackNotes() { return notes; }

    public void SetTrackNotes(string note) { notes.Add(note); }

    public int GetTrackLength() { return notes.Count; }
}