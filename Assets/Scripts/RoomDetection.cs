using UnityEngine;

public class RoomDetection : MonoBehaviour
{
    public bool playerInRange;
    public GameObject Doors;
    public NoteManager noteManager;
    public GameObject noteManagerGameObject;

    private int enemiesInRange = 0;
    public void Start()
    {
        noteManagerGameObject = GameObject.Find("NoteManager");
        noteManager = noteManagerGameObject.GetComponent<NoteManager>();
        noteManager.started = false;
        noteManager.startedRiff = false;
    }

    public void AddEnemy()
    {
        enemiesInRange++;
    }

    public void RemoveEnemy()
    {
        enemiesInRange = Mathf.Max(0, enemiesInRange - 1);
    }
    public void Update()
    {
        if (playerInRange && enemiesInRange > 0)
        {
            Doors.SetActive(true);
            if (!noteManager.started)
            {
                noteManager.StartSong();
            }
            else
            {
                noteManager.StartSong();
            }

            noteManager.ended = false;
        }
        else
        {
            Doors.SetActive(false);
            noteManager.ended = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

        if (other.CompareTag("Enemy"))
        {
            enemiesInRange++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange = Mathf.Max(0, enemiesInRange - 1);
        }
    }
}
