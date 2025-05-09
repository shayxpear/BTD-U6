using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Slot[] slots;
    public GameObject inventoryPanel;

    private bool isInventoryOpen = false;

    // Update is called once per frame
    void Update()
    {
        // Listen for the Tab key press
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();  // Toggle the inventory on Tab press
            Debug.Log("Tab key pressed");
        }
    }

    // Toggle the visibility of the inventory
    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;  // Toggle the inventory state
        inventoryPanel.SetActive(isInventoryOpen);  // Show or hide the inventory UI
    }
    public bool AddItem(ItemInstance itemData)
    {
        // Find an empty slot that can hold the item type
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null && slot.allowedType == itemData.itemType)
            {
                // Create the item in the inventory
                GameObject newItem = new GameObject(itemData.itemName);  // Create a new GameObject for the item
                ItemInstance newItemData = newItem.AddComponent<ItemInstance>();
                newItemData.Initialize(itemData.itemName, itemData.icon, itemData.itemType);  // Copy data from the ItemInstance

                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                slot.currentItem = newItem;  // Set the slot to hold the new item
                return true;
            }
        }

        Debug.Log("No available slot for type: " + itemData.itemType);
        return false;
    }
    }
