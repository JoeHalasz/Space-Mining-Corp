using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    List<Mission> missions = new List<Mission>();
    public List<Mission> GetMissions() { return missions; }
    public void RemoveMission(Mission mission) { missions.Remove(mission); }

    int maxMissions = 5;

    PlayerStats playerStats;
    Inventory playerInventory;
    Inventory shipInventory;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerInventory = GetComponent<Inventory>();
    }
    
    public bool AddMission(Mission mission)
    {
        if (missions.Count < maxMissions)
        {
            missions.Add(mission);
            return true;
        }
        return false;
    }

    public bool HandInMission(Mission mission, FactionManager faction)
    {
        // get shipInv from player stats curernt ship

        if (missions.Contains(mission))
        {
            if (mission.HandInMission(playerInventory, shipInventory))
            {
                missions.Remove(mission);
                // add the rewards to player stats
                playerStats.AddReputation(faction.getFactionName(), mission.GetReputationReward());
                // add the rewards to player inventory
                playerStats.AddCredits(mission.GetCreditsReward());
                // add the new rep to the player stats and the faction stats that this mission came from
                faction.AddPlayerReputation(mission.GetReputationReward());
                return true;
            }
        }
        return false;
    }

}
