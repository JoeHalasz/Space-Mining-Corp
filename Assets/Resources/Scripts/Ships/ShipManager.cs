using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    // dictionary of string and a list of gameobjects
    public Dictionary<string, List<GameObject>> shipPartsDictionary;


    float energy = 0;
    public float GetEnergy() { return energy; }
    float maxEnergy = 0;
    public float GetMaxEnergy() { return maxEnergy; }
    float maxFuel = 0;
    public float GetMaxFuel() { return maxFuel; }
    float fuel = 0;
    public float GetFuel() { return fuel; }
    float health;
    public float GetHealth() { return health; }
    float maxHealth;
    public float GetMaxHealth() { return maxHealth; }
    float forwardSpeed; 
    public float GetForwardSpeed() { return forwardSpeed; }
    float reverseSpeed;
    public float GetReverseSpeed() { return reverseSpeed; }
    float verticalTurnSpeed;
    public float GetVerticalTurnSpeed() { return verticalTurnSpeed; }
    float horizontalTurnSpeed;
    public float GetHorizontalTurnSpeed() { return horizontalTurnSpeed; }
    float strafeSpeed;
    public float GetStrafeSpeed() { return strafeSpeed; }
    float shield;
    public float GetShield() { return shield; }
    float maxShield;
    public float GetMaxShield() { return maxShield; }
    float shieldRegen;
    public float GetShieldRegen() { return shieldRegen; }
    float shieldDelay;
    public float GetShieldDelay() { return shieldDelay; }
    
    // TODO for all ship parts add to save and load file
    // TODO add weapons
    // TODO add thrusters
    

    // used for loading the game

    public void loadState( float energy, float maxEnergy, float maxFuel, float fuel, float health, 
                    float maxHealth, float forwardSpeed, float reverseSpeed, float verticalTurnSpeed, 
                    float horizontalTurnSpeed, float strafeSpeed, float shield, float maxShield, 
                    float shieldRegen, float shieldDelay )
    {
        this.energy = energy;
        this.maxEnergy = maxEnergy;
        this.maxFuel = maxFuel;
        this.fuel = fuel;
        this.health = health;
        this.maxHealth = maxHealth;
        this.forwardSpeed = forwardSpeed;
        this.reverseSpeed = reverseSpeed;
        this.verticalTurnSpeed = verticalTurnSpeed;
        this.horizontalTurnSpeed = horizontalTurnSpeed;
        this.strafeSpeed = strafeSpeed;
        this.shield = shield;
        this.maxShield = maxShield;
        this.shieldRegen = shieldRegen;
        this.shieldDelay = shieldDelay;
    }

    // get the ships inventory script
    Inventory inventory;
    OpenInventoryUI inventoryUI;

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
