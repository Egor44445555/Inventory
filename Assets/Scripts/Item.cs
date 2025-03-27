using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public string itemName;
    public Sprite itemSprite;
    public Vector2 size = Vector2.one;
    public InventorySlot[] occupiedSlots;

    [HideInInspector] public Vector2Int lastPoint;
    [HideInInspector] public Vector3 basePoint;
    [HideInInspector] public Vector2 baseSize;

    void Start()
    {
        basePoint = GetComponent<RectTransform>().anchoredPosition;
        baseSize = GetComponent<RectTransform>().sizeDelta;
    }
    
    public Vector2 GetSize()
    {
        return size;
    }
    
    public void SetOccupiedSlots(InventorySlot[] slots)
    {
        occupiedSlots = slots;
    }
    
    public void ClearOccupiedSlots()
    {
        if (occupiedSlots != null)
        {
            foreach (var slot in occupiedSlots)
            {
                
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }
            occupiedSlots = null;
        }
    }
} 