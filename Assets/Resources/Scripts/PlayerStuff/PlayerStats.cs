using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    float playerCredits;
    float playerHealth;
    float maxHealth;

    public GameObject playerCurrentShip; // should always be the same 

    public int numMainMissionsDone = 0;

    MissionManager missionManager;

    int invSlots = 8;
    public int GetInvSlots() { return invSlots; }

    void Start()
    {
        missionManager = GetComponent<MissionManager>();
    }

    public void GetMissions()
    {
        missionManager.GetMissions();
    }

    public void SetMissions(List<Mission> missions)
    {
        missionManager.LoadMissions(missions);
    }

    public float getHealth()
    {
        return playerHealth;
    }

    // used for loading game
    public float setHealth(float amount)
    {
        playerHealth = amount;
        return playerHealth;
    }

    public void addHealth(float amount)
    {
        playerHealth += amount;
        if (playerHealth > maxHealth)
        {
            playerHealth = maxHealth;
        }
    }
    
    public void removeHealth(float amount)
    {
        playerHealth -= amount;
        if (playerHealth < 0)
        {
            playerHealth = 0;
        }
        die();
    }

    public float getCredits()
    {
        return playerCredits;
    }

    // used for loading game
    public float setCredits(float amount)
    {
        playerCredits = amount;
        return playerCredits;
    }
    
    public void AddCredits(float amount)
    {
        playerCredits += amount;
    }

    public bool RemoveCredits(float amount)
    {
        if (playerCredits - amount >= 0)
        {
            playerCredits -= amount;
            return true;
        }
        Debug.Log("Not enough credits");
        return false;
    }

    public void die()
    {
        // TODO make the player respawn at their ship without anything in their invnetory
        // if the ship explodes the player has to reload the game 
    }

}
