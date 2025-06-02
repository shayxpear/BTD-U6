using UnityEngine;

public class ItemPickup : MonoBehaviour
{
      // Reference to the player's inventory
    private ItemInstance itemData;  // Reference to the item's data (item name, icon, type)
    public GameObject inventoryManager;
    public GameObject HeartStickerUI;
    public Slot[] slots;

    public PlayerController playerController;
    public InventoryManager inventory;


    private void Start()
    {
        // Automatically find the inventory in the scene
        inventoryManager = GameObject.Find("InventoryManager");
        //inventory =  inventoryManager.GetComponent<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("Inventory not found in the scene.");
        }
        if (HeartStickerUI != null)
        {
            HeartStickerUI.SetActive(false);  // Hide the sticker UI initially
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

        //Automatically pick up the item when entering the trigger
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

   
private void PickupItem()
    {
        if (inventory != null && itemData != null)
        {
            Debug.Log("Attempting to add item to inventory: " + itemData.itemName);
            bool added = inventory.AddItem(itemData);


            if (playerController != null)
            {
                playerController.SetHealth(9);
                Destroy(gameObject);
                Debug.Log("healed");
                
            }
            else
            {
                Debug.LogWarning("PlayerController reference not set.");
            }
            if (added)
            {
                if (HeartStickerUI != null)
                {
                    Instantiate(HeartStickerUI, slots[0].transform);
                    HeartStickerUI.SetActive(true);
                }

                Debug.Log("Item added to inventory: " + itemData.itemName);
                Destroy(gameObject);
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
