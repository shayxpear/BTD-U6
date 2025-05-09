using UnityEngine;

public class Inventory : MonoBehaviour
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
    public bool AddItem(GameObject itemPrefab)
    {
        ItemInstance itemData = itemPrefab.GetComponent<ItemInstance>();
        if (itemData == null)
        {
            Debug.LogWarning("Item prefab missing ItemInstance.");
            return false;
        }

        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null && slot.allowedType == itemData.itemType)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                RectTransform rect = newItem.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = Vector2.zero;

                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("No available slot for type: " + itemData.itemType);
        return false;
    }
}