using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGenerator : MonoBehaviour
{

    // get the inventory script of this object
    private Inventory inventory;
    // get the ship manager script of this object
    private ShipManager shipManager;

    // Start is called before the first frame update
    void Start()
    {
        inventory = transform.parent.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.Log(gameObject.name + " has no inventory script attached");
        }
        shipManager = transform.parent.GetComponent<ShipManager>();
        if (shipManager == null)
        {
            Debug.Log(gameObject.name + " has no ship manager script attached");
        }
    }

    // 60 times a second
    void FixedUpdate()
    {
        // if the ship has fuel in the inventory, and isnt at max energy, then add energy and use fuel
        if (shipManager != null && inventory != null && shipManager.GetEnergy() < shipManager.GetMaxEnergy())
        {
            ItemPair fuel = inventory.getFuel();
            if (fuel != null)
            {
                inventory.removeItemAmount(fuel.item, .05f);
                shipManager.AddEnergy(1f);
            }
        }
    }
}
