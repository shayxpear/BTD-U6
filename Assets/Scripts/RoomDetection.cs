using UnityEngine;

public class RoomDetection : MonoBehaviour
{
    public bool playerInRange;
    public GameObject Doors;
    public BetterNoteManager noteManager;
    public TrackHolder trackHolder;

    private int enemiesInRange = 0;
    public void Start()
    {

        noteManager = GameObject.Find("NoteManager").GetComponent<BetterNoteManager>();
        trackHolder = GameObject.Find("Track Holder").GetComponent<TrackHolder>();
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
                trackHolder.backgroundSong.Stop();
            }
            else
            {
                noteManager.StartSong();
                trackHolder.backgroundSong.Stop();
            }

            noteManager.ended = false;
        }
        else
        {
            Doors.SetActive(false);
            noteManager.ended = true;
            if (!trackHolder.backgroundSong.isPlaying) { trackHolder.backgroundSong.Play(); }
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
