using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemData
{
    public string itemName;
    public int weight;
    public int value;
}

public class Inventory : MonoBehaviour
{
    public int maxWeight = 50;
    public int currentWeight = 0;
    public int totalValue = 0;

    public List<ItemData> collectedItems = new List<ItemData>();

    public bool AddItem(string itemName, int weight, int value)
    {
        if (currentWeight + weight <= maxWeight)
        {
            collectedItems.Add(new ItemData { itemName = itemName, weight = weight, value = value });
            currentWeight += weight;
            totalValue += value;
            return true;
        }
        return false;
    }

    public void RemoveItem(ItemData item)
    {
        if (collectedItems.Contains(item))
        {
            collectedItems.Remove(item);
            currentWeight -= item.weight;
            totalValue -= item.value;
        }
    }
}