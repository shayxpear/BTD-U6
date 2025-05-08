using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryUI;
    public GameObject itemPrefab; // Prefab with ItemInstance
    public List<Slot> slots = new List<Slot>(); // Assign slots in Inspector
    

    private bool isInventoryOpen = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen);

        Time.timeScale = isInventoryOpen ? 0f : 1f;
    }

    public bool AddItem(string itemName, Sprite icon)
    {
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                ItemInstance itemInstance = newItem.GetComponent<ItemInstance>();

                itemInstance.itemName = itemName;
                itemInstance.icon = icon;

                slot.currentItem = newItem;

                return true;
            }
        }

        Debug.Log("Inventory is full.");
        return false;
    }
}
