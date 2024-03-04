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

    [SerializeField]
    bool remakeMissions = false;

    float playerReputation = 0; // this should be a float between 0 and 4
    public float GetPlayerReputation() { return playerReputation; }
    public void SetPlayerReputation(float rep) { playerReputation = rep; }
    public void AddPlayerReputation(float add) { playerReputation += add; remakeMissions = true; }

    GameObject Player;

    string factionName;
    public string getFactionName() { return factionName; }


    PlayerStats playerStats;

    Minerals minerals;
    WorldManager worldManager;

    // Start is called before the first frame update
    void Start()
    {
        factionName = transform.name;
        Player = GameObject.FindGameObjectWithTag("Player");
        playerStats = Player.GetComponent<PlayerStats>();
        CreateAllMissions c = new CreateAllMissions();
        minerals = GameObject.Find("WorldManager").GetComponent<Minerals>();
        AllDefaultMissionsByLevel = c.CreateAllGameMissions(minerals);
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        MakeMissions();
    }

    void Update()
    {
        if (remakeMissions)
        {
            MakeMissions();
            transform.Find("NPC").GetComponent<NPCmanager>().RefreshMissions();
            remakeMissions = false;
        }
    }

    void HandInMission(Mission mission, Inventory playerInventory, Inventory shipInventory)
    {
        if (mission.HandInMission(playerInventory, shipInventory))
        {
            // give the player the rewards
            playerReputation += mission.GetReputationReward();
            playerStats.AddCredits(mission.GetCreditsReward());

            // remake the missions pool
            MakeMissions();
        }
    }

    public void MakeMissions()
    {
        currentMissions = new List<Mission>();
        int playerLevel = (int)playerReputation / 100;
        MakeMainMissions(playerLevel);
        // number of missions to make should be a random number between 4 and 7
        int numberOfMissionsToMake = Random.Range(4, 8);
        int tries = 0;
        while (currentMissions.Count < numberOfMissionsToMake)
        {
            if (tries++ > 100)
                break;

            // get a random number from 1 to 3.
            int numMissionsToCombine = Random.Range(1, 4);
            List<Mission> missionsToCombine = new List<Mission>();
            for (int i = 0; i < numMissionsToCombine; i++)
            {
                int randLevel = 0;
                if (playerLevel < 1)
                    randLevel = 0;
                else
                    randLevel = (int)Random.Range(1, playerLevel + 1);

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
                if (newMission.getIsMainMission())
                    newMission.SetDescription(newMission.GetDescription());
                else
                    newMission.SetDescription(factionName + " needs some " + newMission.GetDescription());
                currentMissions.Add(newMission);
            }
        }

    }

    void MakeMainMissions(int playerLevel) // TODO change this to new plan
    {
        int worldState = (int)worldManager.worldState;
        if (worldState == 0)
        {
            Mission mission = new Mission();
            List<ItemPair> goal = new List<ItemPair>();
            // add 1000 ice
            goal.Add(new ItemPair(minerals.GetMineralByName("Ice"), 0));
            mission.SetUpMission("Main Mission Mine Ice", "Board your ship, fly out and mine some ice, and bring it back here", 0, 1000, 1, new List<ItemPair>(), goal, true);
            currentMissions.Add(mission);
        }
        if (worldState == 1 && playerReputation > 180)
        {
            Mission mission = new Mission();
            List<ItemPair> goal = new List<ItemPair>();
            goal.Add(new ItemPair(minerals.GetMineralByName("Uranium"), 1000));
            mission.SetUpMission("Main Mission Refine Uranium", "We want to restart the station core, we need some tier 1 fuel to get that done.", 0, 5000, .2f, new List<ItemPair>(), goal, true);
            currentMissions.Add(mission);
        }
        if (worldState == 2 && playerReputation > 280)
        {
            Mission mission = new Mission();
            List<ItemPair> goal = new List<ItemPair>();
            goal.Add(new ItemPair(minerals.GetMineralByName("Pentolium"), 1000));
            mission.SetUpMission("Main Mission Refine Pentolium", "We want to restart the warpgate, we need some tier 2 fuel to get that done.", 0, 10000, .2f, new List<ItemPair>(), goal, true);
            currentMissions.Add(mission);
        }
        if (worldState == 3 && playerReputation > 380)
        {
            Mission mission = new Mission();
            List<ItemPair> goal = new List<ItemPair>();
            goal.Add(new ItemPair(minerals.GetMineralByName("Exlite"), 1500));
            goal.Add(new ItemPair(minerals.GetMineralByName("Gravitite"), 1500));
            mission.SetUpMission("Main Mission Refine Exlite, Refine Gravitite", "Its time to destroy the threat once and for all.", 0, 100000, .2f, new List<ItemPair>(), goal, true);
            currentMissions.Add(mission);
        }
    }
}
