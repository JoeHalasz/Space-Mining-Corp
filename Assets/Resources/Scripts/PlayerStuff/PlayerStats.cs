using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    
    float playerCredits;
    float playerHealth;

    GameObject playerCurrentShip;
    
    Dictionary<string, float> playerReputation = new Dictionary<string, float>();

    public float getCredits()
    {
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

    public void AddReputation(string faction, float amount)
    {
        if (playerReputation.ContainsKey(faction))
            playerReputation[faction] += amount;
        else
            playerReputation.Add(faction, amount);
    }

    public void RemoveReputation(string faction, float amount)
    {
        if (playerReputation.ContainsKey(faction)) 
            playerReputation[faction] -= amount; 
        else
            playerReputation[faction] = 0;

        if (playerReputation[faction] < 0)
            playerReputation[faction] = 0;
    }

}
