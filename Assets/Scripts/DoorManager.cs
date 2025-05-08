using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorManager : MonoBehaviour
{
    public string nextScene;
    public NoteManager noteManager;
    public GameObject noteManagerGameObject;

    public void Start()
    {
        noteManagerGameObject = GameObject.Find("NoteManager");
        noteManager = noteManagerGameObject.GetComponent<NoteManager>();
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
