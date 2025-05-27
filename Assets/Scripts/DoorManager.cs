using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorManager : MonoBehaviour
{
    public string nextScene;
    public BetterNoteManager noteManager;
    public GameObject noteManagerGameObject;

    public void Start()
    {
        noteManagerGameObject = GameObject.Find("NoteManager");
        noteManager = noteManagerGameObject.GetComponent<BetterNoteManager>();
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
