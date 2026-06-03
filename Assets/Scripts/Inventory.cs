using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemData
{
    public int weight;
    public int value;
}

public class Inventory : MonoBehaviour
{
    public int maxWeight = 50;
    public int currentWeight = 0;
    public int totalValue = 0;

    public List<ItemData> collectedItems = new List<ItemData>();

    public bool AddItem(int weight, int value)
    {
        if (currentWeight + weight <= maxWeight)
        {
            collectedItems.Add(new ItemData { weight = weight, value = value });
            currentWeight += weight;
            totalValue += value;
            return true;
        }
        return false;
    }
}