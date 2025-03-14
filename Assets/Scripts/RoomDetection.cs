using UnityEngine;

public class RoomDetection : MonoBehaviour
{
    public bool playerInRange;
    public GameObject Doors;

    private int enemiesInRange = 0;

    public void Update()
    {
        if (playerInRange && enemiesInRange > 0)
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
