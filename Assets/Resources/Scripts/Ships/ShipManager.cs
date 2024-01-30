using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    // dictionary of string and a list of gameobjects
    public Dictionary<string, List<GameObject>> shipPartsDictionary;

    private float maxEnergy = 0;
    public float GetMaxEnergy() { return maxEnergy; }
    private float maxFuel = 0;
    public float GetMaxFuel() { return maxFuel; }

    private float energy = 0;
    public float GetEnergy() { return energy; }

    // get the ships inventory script
    private Inventory inventory;
    private OpenInventoryUI inventoryUI;

    int numCargoSlots = -1;
    public int GetNumCargoSlots() { return numCargoSlots; }
    

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
        inventoryUI = GetComponent<OpenInventoryUI>();
        if (inventory == null )
            Debug.Log(gameObject.name + " has no inventory script attached");
        if (inventoryUI == null)
            Debug.Log(gameObject.name + " has no inventory UI script attached");

    }

    public bool UseEnergy(float useEnergy)
    {
        if (energy - useEnergy >= 0)
        {
            energy -= useEnergy;
            return true;
        }
        return false;
    }

    public bool AddEnergy(float addEnergy)
    {    
        if (energy + addEnergy <= maxEnergy)
        {
            energy += addEnergy;
            Debug.Log("Ship has " + energy + " energy");
            return true;
        }
        return false;
    }

}
