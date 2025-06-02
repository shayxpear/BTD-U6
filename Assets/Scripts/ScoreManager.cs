using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    //public int enemyScorePoint;
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
        Debug.Log("Total Score: " + totalScore);
    }
    public void AddScore(int points)
    {
        totalScore += points * scoreMultiplier;
        Debug.Log("Score Added: " + points * scoreMultiplier + ", Total Score: " + totalScore);
    }
}
