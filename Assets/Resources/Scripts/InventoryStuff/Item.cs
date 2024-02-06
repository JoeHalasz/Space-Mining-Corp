using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    [System.Serializable]
    public enum ItemType
    {
        Weapon,
        Armor,
        Fuel,
        Ore,
        Mineral,
        ShipPart,
        Misc
    }

    string Name="";
    string description="";
    float density;
    float sellValue;
    float buyValue;
    bool stackable;
    int maxStack;
    int type;
    int level;

    public void SetUpItem(string name, string description, float density, float sellValue, float buyValue, bool stackable, int maxStack, ItemType type, int level)
    {
        this.Name = name;
        this.description = description;
        this.density = density;
        this.sellValue = sellValue;
        this.buyValue = buyValue;
        this.stackable = stackable;
        this.maxStack = maxStack;
        this.type = (int)type;
        this.level = level;
    }

    public string getName()        { return Name; }
    public string getDescription() { return description; }
    public float getDensity()       { return density; }
    public float getSellValue()    { return sellValue; }
    public float getBuyValue()     { return buyValue; }
    public bool getStackable()     { return stackable; }
    public int getMaxStack()       { return maxStack; }
    public ItemType getType()      { return (ItemType)type; }
    public int getLevel()          { return level; }

}
