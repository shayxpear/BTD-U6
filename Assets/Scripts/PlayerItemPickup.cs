using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public GameObject itemPrefab;
    public Inventory inventory;

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (inventory != null)
            {
                bool added = inventory.AddItem(itemPrefab);
                if (added)
                {
                    Destroy(gameObject); // Remove item from world
                }
            }
        }
    }
}