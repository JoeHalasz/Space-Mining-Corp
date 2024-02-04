using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// pair class that holds an item and the amount of that item
[System.Serializable]
public class ItemPair
{
    public Item item;
    public float amount;
    public ItemPair(Item item, float amount) { this.item = item; this.amount = amount; }
    public float getAmount() { return amount; }
    public float getWeightTotal() { return item.getDensity() * amount; }

    public bool addAmount(float amount)
    {
        if (this.amount + amount <= this.item.getMaxStack())
        {
            this.amount += amount;
            return true;
        }
        return false;
    }
    public float addAmountPossible(float amount) // will return the remainder
    {
        float totalToAdd = Mathf.Min(amount, item.getMaxStack() - this.amount);

        this.amount += totalToAdd;
        return amount - totalToAdd;
    }
    public bool removeAmount(float amount)
    {
        if (this.amount - amount >= 0)
        {
            this.amount -= amount;
            return true;
        }
        return false;
    }

    public ItemPair GetCopy()
    {
        return new ItemPair(this.item, this.amount);
    }

    public bool isFull()
    {
        return amount == item.getMaxStack();
    }
}
