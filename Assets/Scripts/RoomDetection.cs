using UnityEngine;
using System.Collections;

public class RoomDetection : MonoBehaviour
{
    [HideInInspector] public bool playerInRange;
    public GameObject Doors;
    private BetterNoteManager noteManager;
    private TrackHolder trackHolder;

    private Coroutine closeRoomCoroutine;
    public int enemiesInRange = 0;
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
            // Cancel any pending close
            if (closeRoomCoroutine != null)
            {
                StopCoroutine(closeRoomCoroutine);
                closeRoomCoroutine = null;
            }

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
            // Start delayed close if not already running
            if (closeRoomCoroutine == null)
            {
                closeRoomCoroutine = StartCoroutine(CloseRoomWithDelay());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

        if (other.CompareTag("Enemy") && other is BoxCollider2D)
        {
            enemiesInRange++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other is BoxCollider2D)
        {
            enemiesInRange = Mathf.Max(0, enemiesInRange - 1);
        }
    }
    private IEnumerator CloseRoomWithDelay() 
    {
        yield return new WaitForSeconds(0.1f);

        // Double-check state in case enemies/player came back during the delay
        if (!(playerInRange && enemiesInRange > 0))
        {
            Doors.SetActive(false);
            noteManager.ended = true;
            if (!trackHolder.backgroundSong.isPlaying)
            {
                trackHolder.backgroundSong.Play();
            }
        }
        closeRoomCoroutine = null;
    }
}
