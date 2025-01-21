using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHandler : MonoBehaviour
{
    public Button inventoryBTN;
    public GameObject Inventory;
    public GameObject inventoryItem;
    public GameObject content;  // The parent object for the inventory items
    public bool inventoryOpened = false;
    public int invSlots = 10;  // Default slot count
    public Dictionary<int, InventoryItem> inventoryItems = new Dictionary<int, InventoryItem>();

    // Method to set slot count from another script
    public void SetInventorySlotCount(int newSlotCount)
    {
        invSlots = newSlotCount;
        // Clear the previous inventory items before spawning new ones
        ClearInventory();
        // Spawn new inventory items based on the updated slot count
        for (int i = 0; i < invSlots; i++)
        {
            SpawnInventoryItem(i);
        }
    }

    public void openInventory()
    {
        inventoryOpened = !inventoryOpened;
        Inventory.SetActive(inventoryOpened);
    }

    private void Start()
    {
        // Initially create inventory items based on the default invSlots
        for (int i = 0; i < invSlots; i++)
        {
            SpawnInventoryItem(i);
        }
    }

    private void SpawnInventoryItem(int slotIndex)
    {
        // Instantiate the inventory item, but set 'content' as the parent instead of 'Inventory'
        GameObject item = Instantiate(inventoryItem, content.transform);  // Set content as the parent

        // Optionally, you can position them using a grid-based layout
        item.GetComponent<RectTransform>().anchoredPosition = new Vector2(100 * (slotIndex % 5), -100 * (slotIndex / 5));

        // Retrieve the InventoryItem component from the spawned item
        InventoryItem invItem = item.GetComponent<InventoryItem>();

        // Store the item in the dictionary with the slot index
        inventoryItems.Add(slotIndex, invItem);
    }

    private void ClearInventory()
    {
        // Clear the dictionary and destroy all existing items in the inventory
        foreach (var item in inventoryItems)
        {
            Destroy(item.Value.gameObject);
        }
        inventoryItems.Clear();
    }
}
