using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    
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

    string Name;
    string description;
    float density;
    float sellValue;
    float buyValue;
    bool stackable;
    int maxStack;
    ItemType type;
    Color color;
    int level;
    Sprite sprite;

    public void SetUpItem(string name, string description, float density, float sellValue, float buyValue, bool stackable, int maxStack, ItemType type, Color color, int level, Sprite sprite)
    {
        this.Name = name;
        this.description = description;
        this.density = density;
        this.sellValue = sellValue;
        this.buyValue = buyValue;
        this.stackable = stackable;
        this.maxStack = maxStack;
        this.type = type;
        this.color = color;
        this.level = level;
        this.sprite = sprite;
    }

    public string getName()        { return Name; }
    public string getDescription() { return description; }
    public float getDensity()       { return density; }
    public float getSellValue()    { return sellValue; }
    public float getBuyValue()     { return buyValue; }
    public bool getStackable()     { return stackable; }
    public int getMaxStack()       { return maxStack; }
    public ItemType getType()      { return type; }
    public Color getColor()        { return color; }
    public int getLevel()          { return level; }
    public Sprite getSprite()      { return sprite; }


}
