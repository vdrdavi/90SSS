using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class PlayerCollector : MonoBehaviour
{
    public GameObject lootPrefab;

    private Inventory inventory;
    private List<LootItem> itemsInRange = new List<LootItem>();
    private TooltipController tooltip;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    private void Start()
    {
        tooltip = FindObjectOfType<TooltipController>(true);
    }

    private void Update()
    {
        HandleCollectionInput();
    }

    private void HandleCollectionInput()
    {
        bool pressCollect = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
            pressCollect = UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame;
#else
        pressCollect = Input.GetKeyDown(KeyCode.E);
#endif

        if (pressCollect && itemsInRange.Count > 0)
        {
            LootItem itemToCollect = itemsInRange[0];

            if (itemToCollect != null)
            {
                if (inventory.AddItem(itemToCollect.itemName, itemToCollect.weight, itemToCollect.value))
                {
                    itemsInRange.Remove(itemToCollect);
                    Destroy(itemToCollect.gameObject);
                    UpdateTooltipState();
                }
            }
            else
            {
                itemsInRange.RemoveAt(0);
                UpdateTooltipState();
            }
        }
    }

    public bool TryDropItem(ItemData itemData)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Loot"))
            {
                return false;
            }
        }

        GameObject droppedItem = Instantiate(lootPrefab, transform.position, Quaternion.identity);
        LootItem lootScript = droppedItem.GetComponent<LootItem>();

        if (lootScript != null)
        {
            lootScript.itemName = itemData.itemName;
            lootScript.weight = itemData.weight;
            lootScript.value = itemData.value;
        }

        inventory.RemoveItem(itemData);
        return true;
    }

    private void UpdateTooltipState()
    {
        if (tooltip == null) return;

        if (itemsInRange.Count == 0)
        {
            tooltip.HideTooltip();
            return;
        }

        LootItem currentItem = itemsInRange[0];

        if (currentItem != null)
        {
            tooltip.ShowTooltip(currentItem.itemName, currentItem.weight, currentItem.value, transform);
        }
        else
        {
            itemsInRange.RemoveAt(0);
            UpdateTooltipState();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Loot"))
        {
            LootItem item = collision.GetComponent<LootItem>();
            if (item != null && !itemsInRange.Contains(item))
            {
                itemsInRange.Add(item);
                UpdateTooltipState();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Loot"))
        {
            LootItem item = collision.GetComponent<LootItem>();
            if (item != null && itemsInRange.Contains(item))
            {
                itemsInRange.Remove(item);
                UpdateTooltipState();
            }
        }
    }
}