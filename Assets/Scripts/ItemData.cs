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

    public void Initialize(string name, Sprite newIcon, ItemType type)
    {
        itemName = name;
        icon = newIcon;
        itemType = type;

        if (iconImage != null)
        {
            iconImage.sprite = icon;  // Set the UI icon image
        }
    }
}

