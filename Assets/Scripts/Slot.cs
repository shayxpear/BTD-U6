using UnityEngine;

public class Slot : MonoBehaviour
{
    public GameObject currentItem; // current item in the slot
    public ItemType allowedType;

    public bool CanAcceptItem(ItemInstance item)
    {
        return item.itemType == allowedType;
    }
}
