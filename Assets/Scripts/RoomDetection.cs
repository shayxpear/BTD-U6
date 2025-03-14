using UnityEngine;

public class RoomDetection : MonoBehaviour
{
    public bool playerInRange;
    public GameObject Doors;

    private bool enemyInRange;

    public void Update()
    {
        if(playerInRange && enemyInRange)
        {
            Doors.SetActive(true);
        }
        else
        {
            Doors.SetActive(false);
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
            enemyInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyInRange = false;
        }
    }
}
