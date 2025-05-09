using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public InventoryManager inventory;  // Reference to the player's inventory
    //private bool playerInRange = false;  // Whether the player is in range of the item
    private ItemInstance itemData;  // Reference to the item's data (item name, icon, type)
    public GameObject inventoryManager;
    public GameObject HeartStickerUI;
    public Slot[] slots;

    private void Start()
    {
        // Automatically find the inventory in the scene
        inventoryManager = GameObject.Find("InventoryManager");
        //inventory =  inventoryManager.GetComponent<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("Inventory not found in the scene.");
        }
    }

    // Called when the player enters the trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Check if the player enters the item trigger
        {
            Debug.Log("Player entered item trigger.");
            //playerInRange = true;
            itemData = GetComponent<ItemInstance>();  // Get the ItemInstance data from the pickup object

            // Automatically pick up the item when entering the trigger
            PickupItem();
        }
    }

    // Called when the player exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // If the player leaves the trigger zone
        {
            Debug.Log("Player left item trigger.");
            //playerInRange = false;
            itemData = null;  // Clear the itemData reference
        }
    }

    // Function to handle the item pickup logic
    private void PickupItem()
    {
        if (inventory != null && itemData != null)
        {
            Debug.Log("Attempting to add item to inventory: " + itemData.itemName);
            bool added = inventory.AddItem(itemData);  // Add the item to the inventory

            if (added)
            {
                Instantiate(HeartStickerUI, slots[0].transform);
                Debug.Log("Item added to inventory: " + itemData.itemName);
                Destroy(gameObject);  // Remove the item from the world
            }
            else
            {
                Debug.Log("Inventory is full or unable to pick up the item.");
            }
        }
        else
        {
            Debug.LogError("Inventory or ItemData is not set.");
        }
    }
}
