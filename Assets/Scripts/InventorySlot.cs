using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item currentItem;
    public bool isOccupied;
    public Vector2Int gridPosition;
    
    RectTransform rectTransform;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetItem(Item item)
    {
        currentItem = item;
        isOccupied = true;
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        isOccupied = false;
    }
    
    public Vector2 GetCenterPosition()
    {
        return rectTransform.anchoredPosition + new Vector2(rectTransform.rect.width * 0.5f, rectTransform.rect.height * 0.5f);
    }
} 