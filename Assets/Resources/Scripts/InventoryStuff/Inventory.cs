using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    List<ItemPair> items;

    public OpenInventoryUI inventoryUIScript;

    int totalInvSlots = -1;
    
    // Start is called before the first frame update
    void Start()
    {

        inventoryUIScript = GetComponent<OpenInventoryUI>();

        items = new List<ItemPair>();
        if (inventoryUIScript == null)
            Debug.LogError("Inventory UI script is null in Inventory script on game object " + this.gameObject);
        else
        {
            int numRows;
            int numCols;

            numRows = inventoryUIScript.numRows;
            numCols = inventoryUIScript.numCols;
            
            totalInvSlots = numRows * numCols;
            for (int i = 0; i < totalInvSlots; i++)
            {
                items.Add(null);
            }
        }
    }

    public void openInventory(GameObject player)
    {
        if (inventoryUIScript != null)
        {
            // open the players inventory
            inventoryUIScript.ShowInventory(this.gameObject);
            player.GetComponent<OpenInventoryUI>().ShowInventory(this.gameObject);
            inventoryUIScript.UpdateInventory();
        }
    }

    public void closeInventory(GameObject player)
    {
        if (inventoryUIScript != null)
        {
            // close all inventorys
            player.GetComponent<OpenInventoryUI>().HideInventory();
        }
    }

    public void PrintInv()
    {
        int i = 0;
        foreach (ItemPair pair in items)
        {
            if (pair != null)
                Debug.Log(gameObject.name + "Inv " + i + ": " + pair.item.getName() + " " + pair.amount);
            
            i++;
        }
    }
    
    public ItemPair getFirstItem()
    {
        // loop through all items, return the first one that isnt null
        foreach (ItemPair pair in items)
        {
            if (pair != null)
            {
                return pair;
            }
        }
        return null;
    }

    public int getNumItems()
    {
        int num = 0;
        // loop through all items, return the first one that isnt null
        foreach (ItemPair pair in items)
        {
            if (pair != null)
            {
                num++;
            }
        }
        return num;
    }

    public ItemPair addItem(Item item, float amount, int pos) // if pos is -1, add to first available slot
    {
        // make a pair out of item
        ItemPair itemPair = new ItemPair(item, amount);
        
        if (pos != -1)
        {
            // if the position is null, add it there
            if (items[pos] == null)
            {
                items[pos] = itemPair;
                PrintInv();inventoryUIScript.UpdateInventory();
                return null; // this means the other inv should replace the slot this came from with null
            }
            else
            {
                if (items[pos].item.getName() == item.getName())
                {
                    // if the item is the same, add the amount to the item at position
                    if (!items[pos].addAmount(amount))
                    {
                        float totalLeft = items[pos].addAmountPossible(amount);
                        PrintInv();inventoryUIScript.UpdateInventory();
                        // make a new item pair with the amount that couldnt be added
                        return new ItemPair(item, totalLeft);
                    }
                    PrintInv();inventoryUIScript.UpdateInventory();
                    return null; // this means the other inv should replace the slot this came from with null
                }
                else
                {
                    // set this as the thing at position and return the old thing at position
                    // should act like item swap
                    ItemPair oldItemPair = items[pos];
                    items[pos] = itemPair;
                    PrintInv();inventoryUIScript.UpdateInventory();
                    return oldItemPair;
                }
            }
        }
        else
        {
            List<int> sameItemPoses = new List<int> ();
            List<int> nullSpots = new List<int>();

            for (int i = 0; i < items.Count; i++)
            {
                // if the item at this position is null then add it here
                if (items[i] == null)
                {
                    nullSpots.Add(i);
                    continue;
                }
                // if the item is the same and isnt full, add this position to the list of sameItemPoses
                if (items[i].item.getName() == item.getName())
                {
                    if (!items[i].isFull())
                    {
                        sameItemPoses.Add(i);
                    }
                }
            }

            for (int i = 0; i < sameItemPoses.Count;i++)
            {
                float totalLeft = items[sameItemPoses[i]].addAmountPossible(amount);
                itemPair.amount -= totalLeft;
                if (totalLeft == 0)
                {
                    PrintInv();inventoryUIScript.UpdateInventory();
                    return null;
                }
            }
            if (nullSpots.Count != 0)
            {
                items[nullSpots[0]] = itemPair;
                PrintInv();inventoryUIScript.UpdateInventory();
                return null;
            }
            else
            {
                PrintInv();inventoryUIScript.UpdateInventory();
                return itemPair;
            }
        }

    }

    public void moveItem(int index, int newIndex)
    {
        ItemPair itemPair = items[index];
        ItemPair itemPair2 = items[newIndex];
        items[index] = itemPair2;
        items[newIndex] = itemPair;
        PrintInv();inventoryUIScript.UpdateInventory();
    }

    public void removeItem(int index)
    {
        items[index] = null;
        PrintInv();inventoryUIScript.UpdateInventory();
    }

    public ItemPair getItemAtPos(int index)
    {
        return items[index];
    }

    // this function should only be used by the inventory UI
    public List<ItemPair> getAllItems()
    {
        return items;
    }

    public bool removeItemAmount(Item item, float amount)
    {
        if (!HasItemAmount(item, amount))
        {
            return false;
        }
        float totalLeft = amount;

        // make a copy of items to loop through
        foreach (ItemPair pair in items)
        {
            if (pair != null && pair.item.getName() == item.getName())
            {
                float removeAmount = Mathf.Min(totalLeft, pair.amount);
                pair.amount -= removeAmount;
                totalLeft -= removeAmount;
                if (totalLeft == 0)
                {
                    return true;
                    // loop through all the items and remove any that have 0 amount
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] != null && items[i].amount == 0)
                        {
                            removeItem(i);
                        }
                    }
                }
            }
        }
        Debug.LogError("Something went wrong in removeItemAmount");
        return false;
    }


    public ItemPair getFuel()
    {
        foreach (ItemPair pair in items)
        {
            if (pair != null && pair.item.getType() == Item.ItemType.Fuel)
            {
                return pair;
            }
        }
        return null;
    }

    public bool HasItemAmount(Item item, float amount)
    {
        float total = 0;
        foreach (ItemPair pair in items)
        {
            if (pair != null && pair.item.getName() == item.getName())
            {
                total += pair.amount;
            }
        }
        if (total >= amount)
        {
            return true;
        }
        return false;
    }

    public float GetAmountOfItem(Item item)
    {
        float total = 0;
        foreach (ItemPair pair in items)
        {
            if (pair != null && pair.item.getName() == item.getName())
            {
                total += pair.amount;
            }
        }
        return total;
    }

}
