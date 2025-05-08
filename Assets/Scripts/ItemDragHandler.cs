using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    void Start()
    {
        // Initialize canvasGroup (if not already attached)
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save the original parent when drag starts
        originalParent = transform.parent;
        transform.SetParent(transform.root); // Move item to the root canvas for dragging
        canvasGroup.blocksRaycasts = false; // Disable raycasting to allow for drop targets
        canvasGroup.alpha = 0.6f; // Make the item semi-transparent while dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update the item's position as it is being dragged
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // Re-enable raycasting after the drag ends
        canvasGroup.alpha = 1f; // Reset the alpha (opacity) of the item

        // Get the drop slot (where the item is dropped)
        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();

        // If item is dropped on a valid slot
        if (dropSlot != null)
        {
            // If the drop slot already has an item, destroy the old item
            if (dropSlot.currentItem != null)
            {
                Destroy(dropSlot.currentItem);
            }

            // Move the dragged item to the new slot
            transform.SetParent(dropSlot.transform);
            transform.localPosition = Vector3.zero; // Position the item inside the slot
            dropSlot.currentItem = gameObject; // Update the slot's current item to the dragged item
        }
        else
        {
            // If dropped outside of a valid slot, return the item to its original parent
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero; // Position the item in its original slot
        }
    }
}
