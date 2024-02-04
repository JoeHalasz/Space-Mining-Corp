using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    List<ItemPair> items = new List<ItemPair>();

    public OpenInventoryUI inventoryUIScript;

    int totalInvSlots = -1;
    public int GetTotalInvSlots() { return totalInvSlots; }
    
    public int numRows;
    public int numCols;
    public int leftOver;

    bool calculatedSize = false;

    GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inventoryUIScript = GetComponent<OpenInventoryUI>();

        // if this gameObject has a shipManager, then get the numslots from it
        if (GetComponent<ShipManager>() != null)
        {
            totalInvSlots = GetComponent<ShipManager>().GetNumCargoSlots();
            calculatedSize = true;
        }
        else if (GetComponent<PlayerStats>() != null)
        {
            totalInvSlots = GetComponent<PlayerStats>().GetInvSlots();
            calculatedSize = true;
        }

        if (inventoryUIScript == null)
            Debug.LogError("Inventory UI script is null in Inventory script on game object " + this.gameObject);
        else
        {
            updateInvSize();
        }
    }

    public void updateInvSize()
    {
        if (totalInvSlots < 0)
        {
            totalInvSlots = numRows * numCols;
            for (int i = 0; i < totalInvSlots; i++)
            {
                items.Add(null);
            }
        }
        if (calculatedSize) {
            SetNumSlots(totalInvSlots);
            if (isEmpty())
            {
                totalInvSlots = numRows * numCols + leftOver;
                for (int i = 0; i < totalInvSlots; i++)
                {
                    items.Add(null);
                }
            }
            else
            {
                Debug.Log("Cant update inventory size, its not empty");
            }
        }
    }

    // sets the number of rows and columns based on the number of slots in the inventory
    // only changes for the otherInventory
    public void SetNumSlots(int numSlots)
    {
        bool good = false;
        numRows = 4;
        int originalNumSlots = numSlots;
        while (!good)
        {
            numCols = 0;
            numSlots = originalNumSlots;
            leftOver = 0;
            while (numSlots > 0)
            {
                if (numSlots - numRows < 0)
                {
                    leftOver = numSlots;
                    numSlots = 0;
                }
                else
                {
                    numSlots -= numRows;
                    numCols++;
                }

            }
            if (numCols < 12)
                good = true;
            else
                numRows++;
        }
    }


    public bool isEmpty()
    { 
        foreach (ItemPair pair in items)
        {
            if (pair != null)
                return false;
        }
        return true;
    }

    bool localIsLeft = true;

    public void openInventory(GameObject other, bool isLeft)
    {
        if (inventoryUIScript != null)
        {
            localIsLeft = isLeft;
            // open the other inventory
            inventoryUIScript.ShowInventory(this.gameObject, isLeft);
            if (other != null)
                other.GetComponent<OpenInventoryUI>().ShowInventory(this.gameObject, !isLeft);
            else
                Debug.LogError("Error in Inventory.cs, other is null");

            inventoryUIScript.UpdateInventory();
            player.GetComponent<UIManager>().OpenOrCloseInventory(this.gameObject);
        }
    }

    public void closeInventory(GameObject player)
    {
        if (inventoryUIScript != null)
        {
            // close all inventorys
            player.GetComponent<OpenInventoryUI>().HideInventory(localIsLeft);
            player.GetComponent<UIManager>().OpenOrCloseInventory(this.gameObject);
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

    // will return the leftover
    public ItemPair addItem(Item item, float amount, int pos) // if pos is -1, add to first available slot
    {
        PrintInv();
        // make a pair out of item
        ItemPair itemPair = new ItemPair(item, amount);
        
        if (pos != -1)
        {
            if (totalInvSlots <= pos)
            {
                return itemPair;
            }
            // if the position is null, add it there
            if (items[pos] == null)
            {
                items[pos] = itemPair;
                inventoryUIScript.UpdateInventory();
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
                        inventoryUIScript.UpdateInventory();
                        // make a new item pair with the amount that couldnt be added
                        return new ItemPair(item, totalLeft);
                    }
                    inventoryUIScript.UpdateInventory();
                    return null; // this means the other inv should replace the slot this came from with null
                }
                else
                {
                    // set this as the thing at position and return the old thing at position
                    // should act like item swap
                    ItemPair oldItemPair = items[pos];
                    items[pos] = itemPair;
                    inventoryUIScript.UpdateInventory();
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
                    inventoryUIScript.UpdateInventory();
                    return null;
                }
            }
            if (nullSpots.Count != 0)
            {
                items[nullSpots[0]] = itemPair;
                inventoryUIScript.UpdateInventory();
                return null;
            }
            else
            {
                inventoryUIScript.UpdateInventory();
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
        inventoryUIScript.UpdateInventory();
    }

    public void removeItem(int index)
    {
        items[index] = null;
        inventoryUIScript.UpdateInventory();
    }

    public ItemPair getItemAtPos(int index)
    {
        if (items.Count <= index)
            return null;
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
                if (pair.amount == 0)
                {
                    removeItem(items.IndexOf(pair));
                }
                totalLeft -= removeAmount;
                if (totalLeft == 0)
                    return true;
                
            }
        }
        if (totalLeft == 0)
            return true;
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

    // only used for saving and loading
    public List<ItemPair> getInventory()
    {
        return items;
    }

    public void setInventory(List<ItemPair> items)
    {
        this.items = items;
    }

}
