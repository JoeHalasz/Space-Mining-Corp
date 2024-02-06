using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    Dictionary<string, Material> itemMaterials = new Dictionary<string, Material>();
    Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();


    public void registerItem(string itemName, Material material, Sprite sprite)
    {
        if (!itemMaterials.ContainsKey(itemName))
        {
            itemMaterials.Add(itemName, material);
        }
        else
        {
            itemMaterials[itemName] = material;
        }
        if (!itemSprites.ContainsKey(itemName))
        {
            itemSprites.Add(itemName, sprite);
        }
        else
        {
            itemSprites[itemName] = sprite;
        }
    }

    public Material getMaterial(string name)
    {
        if (itemMaterials.ContainsKey(name))
        {
            return itemMaterials[name];
        }
        return null;
    }

    public Material getMaterial(Item item)
    {
        if (item != null){
            return getMaterial(item.getName());
        }
        return null;
    }

    public Sprite getSprite(string name)
    {
        if (itemSprites.ContainsKey(name))
        {
            return itemSprites[name];
        }
        return null;
    }

    public Sprite getSprite(Item item)
    {
        if (item != null)
        {
            return getSprite(item.getName());
        }
        return null;
    }

}
