using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private Slot originalSlot;

    private ItemInstance itemInstance;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        itemInstance = GetComponent<ItemInstance>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<Slot>();

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GameObject targetObject = eventData.pointerEnter;
        Slot targetSlot = null;

        if (targetObject != null)
        {
            targetSlot = targetObject.GetComponent<Slot>() ?? targetObject.GetComponentInParent<Slot>();
        }

        if (targetSlot != null && targetSlot.currentItem == null && targetSlot.allowedType == itemInstance.itemType)
        {
            // Valid drop
            transform.SetParent(targetSlot.transform);
            rectTransform.anchoredPosition = Vector2.zero;

            targetSlot.currentItem = gameObject;
            if (originalSlot != null) originalSlot.currentItem = null;
        }
        else if (targetSlot == null)
        {
            // Dropped in the void — destroy item
            if (originalSlot != null)
            {
                originalSlot.currentItem = null;
            }

            Destroy(gameObject);
        }
        else
        {
            // Invalid drop — return to original slot
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}