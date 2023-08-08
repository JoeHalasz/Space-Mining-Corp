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
    

    // Start is called before the first frame update
    void Start()
    {
        UpdateShipPartsDictionary();
        inventory = GetComponent<Inventory>();
        inventoryUI = GetComponent<OpenInventoryUI>();
        if (inventory == null )
            Debug.Log(gameObject.name + " has no inventory script attached");
        if (inventoryUI == null)
            Debug.Log(gameObject.name + " has no inventory UI script attached");

        ShipMaxCalculations();
    }


    void UpdateShipPartsDictionary()
    {
        shipPartsDictionary = new Dictionary<string, List<GameObject>>();
        // go through all this game objects children and if they have the correct tag then add them to the list
        foreach (Transform child in transform)
        {
            if (child.tag != "Untagged")
            {
                // if the tag isnt in the dictionary then add it as an empty list
                if (!shipPartsDictionary.ContainsKey(child.tag))
                {
                    shipPartsDictionary.Add(child.tag, new List<GameObject>());
                }
                shipPartsDictionary[child.tag].Add(child.gameObject);
            }
        }
    }

    void ShipMaxCalculations()
    {
        MaxCargoCalculation();
        MaxPowerCalculation();
    }

    void MaxCargoCalculation()
    {
        int numCargoSlots = 0;
        // loop the list at cargo in shipPartsDictionary
        foreach (GameObject cargo in shipPartsDictionary["Cargo"])
        {
            // add the scale of the cargo to the max cargo
            numCargoSlots += (int)(cargo.transform.localScale.x * cargo.transform.localScale.y * cargo.transform.localScale.z);
        }

        inventoryUI.SetNumSlots(numCargoSlots);
    }

    void MaxPowerCalculation()
    {
        // loop thje list at power in shipPartsDictionary
        foreach (GameObject power in shipPartsDictionary["Battery"])
        {
            // add 100* the scale of the power to the max energy
            maxEnergy += 100 * power.transform.localScale.x * power.transform.localScale.y * power.transform.localScale.z;
        }
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
