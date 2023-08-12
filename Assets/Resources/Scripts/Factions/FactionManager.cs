using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{

    // dictionary of int and list of Mission 
    // these are just the default missions, the amount needs to be changed randomly
    Dictionary<int, List<Mission>> AllDefaultMissionsByLevel;

    List<Mission> currentMissions;
    public List<Mission> GetCurrentMissions() { return currentMissions; }
    public void RemoveMission(Mission mission) { currentMissions.Remove(mission); }

    float playerReputation = 1; // this should be a float between 0 and 4
    public float GetPlayerReputation() { return playerReputation; }
    public float AddPlayerReputation(float add) { return playerReputation += add; }
    
    GameObject Player;

    string factionName;
    public string getFactionName() {  return factionName; }

    [SerializeField]
    bool remakeMissions = false;

    // Start is called before the first frame update
    void Start()
    {
        factionName = transform.name;
        Player = GameObject.FindGameObjectWithTag("Player");
        CreateAllMissions c = new CreateAllMissions();
        AllDefaultMissionsByLevel = c.CreateAllGameMissions();
        MakeMissions();
    }

    void Update()
    {
        if (remakeMissions)
        {
            MakeMissions();
            transform.Find("NPC").GetComponent<NPCmanager>().RefreshMissions();
        }
    }

    void HandInMission(Mission mission, Inventory playerInventory, Inventory shipInventory)
    {
        if (mission.HandInMission(playerInventory, shipInventory))
        {
            // give the player the rewards
            playerReputation += mission.GetReputationReward();
            // add the rep to the playerstats
            Player.GetComponent<PlayerStats>().AddReputation(factionName, mission.GetReputationReward());
            Player.GetComponent<PlayerStats>().AddCredits(mission.GetCreditsReward());

            // remake the missions pool
            MakeMissions();
        }
    }

    public void MakeMissions()
    {
        currentMissions = new List<Mission>();
        int playerLevel = (int)playerReputation;
        // number of missions to make should be a random number between 4 and 7
        int numberOfMissionsToMake = Random.Range(4, 8);
        int tries = 0;
        while (currentMissions.Count < numberOfMissionsToMake)
        {
            if (tries++ > 100)
            {
                Debug.Log("Oh fuck oh god FactionManager.cs");
                break;
            }
            // get a random number from 1 to 3.
            int numMissionsToCombine = Random.Range(1, 4);
            List<Mission> missionsToCombine = new List<Mission>();
            for (int i = 0; i < numMissionsToCombine; i++)
            {
                int randLevel;
                if (playerLevel < 2)
                    randLevel = 1;
                else
                    randLevel = Random.Range(1, playerLevel + 1);

                // 10% chance to change to 0
                if (Random.Range(0, 10) == 0)
                    randLevel = 0;

                Mission addMission = new Mission();
                addMission.SetUpMission(AllDefaultMissionsByLevel[randLevel][Random.Range(0, AllDefaultMissionsByLevel[randLevel].Count)]);
                addMission.RandomlyChangeAmount();
                missionsToCombine.Add(addMission);
            }
            Mission newMission = missionsToCombine[0];

            for (int i = 1; i < missionsToCombine.Count; i++)
            {
                newMission.CombineMission(missionsToCombine[i]);
            }
            // if there isnt a mission with the same name already then add newMission
            bool found = false;
            for (int i = 0; i < currentMissions.Count; i++)
            {
                if (currentMissions[i].GetName() == newMission.GetName())
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                newMission.SetDescription(factionName + " needs some " + newMission.GetDescription());
                currentMissions.Add(newMission);
            }
        }

    }

}
