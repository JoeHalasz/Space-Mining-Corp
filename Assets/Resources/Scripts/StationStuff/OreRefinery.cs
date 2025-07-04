using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreRefinery : MonoBehaviour
{

    // get the inventory script of this object
    private Inventory inventory;

    int waitFrames = 5;
    int count = 0;

    Minerals minerals;
    // Start is called before the first frame update
    void Start()
    {
        minerals = GameObject.Find("WorldManager").GetComponent<Minerals>();
        inventory = GetComponent<Inventory>();
    }

    // 60 times a second
    void FixedUpdate()
    {
        // wait for waitFrames
        if (count++ < waitFrames)
            return;
        else
            count = 0;
        
        // if theres something in the ore inv and both invs exist
        if (inventory != null && inventory.getNumItems() > 0)
        {
            ItemPair oreToRefine = null;
            List<ItemPair> allItems = inventory.getAllItems();
            // loop through all the items and look for the word "Ore" in the name
            foreach (ItemPair item in allItems)
            {
                if (item != null && item.item != null && item.item.getName().Contains("Ore") && item.getAmount() >= 1)
                {
                    Debug.Log(item.item.getName() + " is an ore");
                    // if the item is an ore, then move it to the mineral inv
                    oreToRefine = item;
                    break;
                }
            }

            // this will return the remainder of the item if it cant be added, because we are trying to move only 1, this must return null for it to move any items
            if (oreToRefine != null && inventory.addItem(minerals.GetMineralByOre(oreToRefine.item), 1, -1) == null)
            {
                inventory.removeItemAmount(oreToRefine.item, 1);
                Debug.Log("Refined 1 " + oreToRefine.item.getName() + " into 1 " + minerals.GetMineralByOre(oreToRefine.item).getName());
            }
        }
    }

    public void InteractWithPlayer(GameObject player)
    {
        Debug.Log("Interacted");
    }

}
