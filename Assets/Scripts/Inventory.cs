using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public static Inventory main;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSize = 100f;
    public GameObject slotPrefab;
    public GameObject blockedSlotPrefab;
    public LayerMask itemLayerMask;

    public Item lastItem;
    
    InventorySlot[,] slots;
    bool isDragging = false;
    Camera mainCamera;
    RectTransform inventoryRect;
    Canvas parentCanvas;
    Vector3 originalItemPosition;
    Vector3 originalItemScale;
    BlockedSlot[,] blockedSlots;

    void Awake()
    {
        main = this;
    }

    void Start()
    {
        mainCamera = Camera.main;        
        inventoryRect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        blockedSlots = new BlockedSlot[gridWidth, gridHeight];

        if (mainCamera.GetComponent<Physics2DRaycaster>() == null) {
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }

        CreateGrid();
        GenerateRandomBlockedSlots(3);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                HandleTouchEnd(touch);
            }
        }
    }

    void HandleTouchEnd(Touch touch)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryRect,
            touch.position,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out localPoint
        );

        if (lastItem != null && !IsPositionInsideInventory(localPoint))
        {
            lastItem.ClearOccupiedSlots();
            lastItem.GetComponent<RectTransform>().anchoredPosition = lastItem.basePoint;
            lastItem.GetComponent<RectTransform>().sizeDelta = lastItem.baseSize;
        }

        isDragging = false;
        lastItem = null;
    }

    bool IsPositionInsideInventory(Vector2 localPosition)
    {
        Rect rect = inventoryRect.rect;
        return localPosition.x >= rect.xMin && 
            localPosition.x <= rect.xMax &&
            localPosition.y >= rect.yMin && 
            localPosition.y <= rect.yMax;
    }

    public bool CanPlaceItem(Item item, Vector2Int position)
    {
        if (item != null)
        {
            Vector2 size = item.GetSize();
        
            // Checking for inventory out of bounds
            if (position.x < 0 || position.y < 0 || position.x + (int)size.x > gridWidth || position.y + (int)size.y > gridHeight)
            {
                return false;
            }
            
            // Checking occupied slots
            for (int x = 0; x < (int)size.x; x++)
            {
                for (int y = 0; y < (int)size.y; y++)
                {
                    int slotX = position.x + x;
                    int slotY = position.y + y;
                    
                    if (slots[slotX, slotY].isOccupied)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PlaceItem(Item item, Vector2Int position)
    {
        if (item.occupiedSlots != null)
        {
            foreach (InventorySlot slot in item.occupiedSlots)
            {
                if (slot != null) 
                {
                    slot.ClearSlot();
                }
            }
        }

        Vector2 size = item.GetSize();
        List<InventorySlot> slotsToOccupy = new List<InventorySlot>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int slotX = position.x + x;
                int slotY = position.y + y;

                if (slotX < gridWidth && slotY < gridHeight)
                {
                    slots[slotX, slotY].SetItem(item);
                    slotsToOccupy.Add(slots[slotX, slotY]);
                }
            }
        }

        // Centering object in slot
        PositionItemCorrectly(item, position, size);

        item.SetOccupiedSlots(slotsToOccupy.ToArray());            
        item.lastPoint = position;
    }

    void PositionItemCorrectly(Item item, Vector2Int position, Vector2 size)
    {
        RectTransform itemRect = item.GetComponent<RectTransform>();       
        Vector2 pivot = itemRect.pivot;        
        Vector2 itemPos = new Vector2((position.x + size.x * pivot.x) * cellSize, (position.y + size.y * pivot.y) * cellSize);

        if (inventoryRect != null)
        {
            itemPos -= new Vector2(inventoryRect.pivot.x * inventoryRect.rect.width, inventoryRect.pivot.y * inventoryRect.rect.height);
        }

        itemRect.anchoredPosition = itemPos;
        itemRect.sizeDelta = new Vector2(size.x * cellSize, size.y * cellSize);
    }

    void CreateGrid()
    {
        slots = new InventorySlot[gridWidth, gridHeight];
        inventoryRect.sizeDelta = new Vector2(gridWidth * cellSize, gridHeight * cellSize);
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject slotObj = Instantiate(slotPrefab, transform);
                RectTransform slotRect = slotObj.GetComponent<RectTransform>();
                
                slotRect.anchoredPosition = new Vector2(x * cellSize + cellSize/2, y * cellSize + cellSize/2);                
                slotRect.sizeDelta = new Vector2(cellSize, cellSize);
                
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                slot.gridPosition = new Vector2Int(x, y);
                slots[x, y] = slot;
            }
        }
    }

    void GenerateRandomBlockedSlots(int blockedSlotsCount)
    {
        
        blockedSlots = new BlockedSlot[gridWidth, gridHeight];
        int currentBlockedCount = 0;
        
        while (currentBlockedCount < blockedSlotsCount)
        {
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            
            if (blockedSlots[x, y] == null && !slots[x, y].isOccupied)
            {
                GameObject blockedSlotObj = Instantiate(blockedSlotPrefab, transform);
                BlockedSlot blockedSlot = blockedSlotObj.GetComponent<BlockedSlot>();
                blockedSlot.gridPosition = new Vector2Int(x, y);
                
                RectTransform blockedRect = blockedSlotObj.GetComponent<RectTransform>();
                blockedRect.anchoredPosition = new Vector2(x * cellSize, y * cellSize);
                blockedRect.sizeDelta = new Vector2(cellSize, cellSize);
                
                blockedSlots[x, y] = blockedSlot;
                slots[x, y].isOccupied = true;
                currentBlockedCount++;
            }
        }
    }
} 