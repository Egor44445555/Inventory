using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Item item;

    Canvas canvas;
    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        item = GetComponent<Item>();        
        canvas = FindObjectOfType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Inventory.main.lastItem = item;

        originalPosition = rectTransform.anchoredPosition;

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Checking for slot availability
        InventorySlot targetSlot = null;

        foreach (var result in eventData.hovered)
        {
            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();

            if (slot != null)
            {
                targetSlot = slot;
                break;
            }
        }

        if (targetSlot != null)
        {
            if (Inventory.main != null && Inventory.main.CanPlaceItem(item, targetSlot.gridPosition))
            {
                item.lastPoint = targetSlot.gridPosition;
                Inventory.main.PlaceItem(item, targetSlot.gridPosition);
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    void ReturnToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }
} 