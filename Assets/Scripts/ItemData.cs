using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Sticker,
    Consumable
}

public class ItemInstance : MonoBehaviour
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public Image iconImage;


    public bool isHealingItem = false;
    public bool isPurchasable = false;
    public int price = 0;
    public void Initialize(string name, Sprite newIcon, ItemType type, bool purchaseable = false, int itemPrice = 0)
    {
        itemName = name;
        icon = newIcon;
        itemType = type;
        isPurchasable = purchaseable;
        price = itemPrice;

        if (iconImage != null)
        {
            iconImage.sprite = icon;  // Set the UI icon image
        }
    }
}

