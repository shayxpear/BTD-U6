using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorManager : MonoBehaviour
{
    public string nextScene;
    private BetterNoteManager noteManager;

    public void Start()
    {
        noteManager = GameObject.Find("NoteManager").GetComponent<BetterNoteManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            noteManager.levelsBeaten++;
            SceneManager.LoadScene(nextScene);
            
        }
    }
}
