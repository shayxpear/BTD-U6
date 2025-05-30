using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int totalScore;
    public int scoreMultiplier;
    
    private BetterNoteManager noteManager;

    void Start()
    {
        noteManager = GameObject.Find("NoteManager").GetComponent<BetterNoteManager>();
    }

    // Update is called once per frame
    void Update()
    {
        scoreMultiplier = noteManager.noteCombo;
        totalScore = totalScore * scoreMultiplier;
    }
}
