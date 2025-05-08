using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
    Sticker,
    Consumable,
}
public class ItemInstance : MonoBehaviour
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    public Image iconImage;

    private void Start()
    {
        if (iconImage != null && icon != null)
            iconImage.sprite = icon;
    }

    public void Initialize(string name, Sprite newIcon, ItemType type)
    {
        itemName = name;
        icon = newIcon;
        itemType = type;

        if (iconImage != null)
            iconImage.sprite = icon;
    }
}
