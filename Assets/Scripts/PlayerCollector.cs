using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class PlayerCollector : MonoBehaviour
{
    private Inventory inventory;
    private List<LootItem> itemsInRange = new List<LootItem>();

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && itemsInRange.Count > 0)
        {
            LootItem itemToCollect = itemsInRange[0];

            if (itemToCollect != null)
            {
                if (inventory.AddItem(itemToCollect.weight, itemToCollect.value))
                {
                    itemsInRange.Remove(itemToCollect);
                    Destroy(itemToCollect.gameObject);
                }
            }
            else
            {
                itemsInRange.RemoveAt(0);
            }
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
            }
        }
    }
}