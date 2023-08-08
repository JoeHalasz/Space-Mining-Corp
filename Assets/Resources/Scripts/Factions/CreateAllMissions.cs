using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAllMissions
{

    // this should create all the missions for the factions to take from
    public Dictionary<int, List<Mission>> CreateAllGameMissions()
    {
        Dictionary<int, List<Mission>> AllMissionsByLevel = new Dictionary<int, List<Mission>>();
        Minerals minerals = new Minerals();
        minerals.SetUp();
        // for every mineral type, create a mission for it. Use GetMineralsInGroup
        for (int i = 0; i < 4; i++)
        {
            foreach (Item mineral in minerals.GetMineralsInGroup(i))
            {
                // create a mission for the mineral
                Mission mission = new Mission();

                // make the amount to mine a random number between 100 and 1000
                int amountToMine = 500;
                // make the reward based on the amount to mine with a little bit of randomness
                float creditsReward = amountToMine * mineral.getSellValue();
                // do the same for rep
                float reputationReward = amountToMine * mineral.getSellValue() * .1f;


                string name;
                if (mineral.getName().Contains("Ore"))
                {
                    name = "Mine " + mineral.getName();
                }
                else if (mineral.getName() == "Ice" || mineral.getName() == "Stone")
                {
                    name = "Mine " + mineral.getName();
                }
                else
                {
                    name = "Refine " + mineral.getName();
                }
                // use SetUpMission(string name,string description, int level, float creditsReward, float reputationReward, List<ItemPair> ItemRewards, List<ItemPair> goal)
                mission.SetUpMission(name, mineral.getDescription(), i, creditsReward, reputationReward, new List<ItemPair>(), new List<ItemPair> { new ItemPair(mineral, amountToMine) });
                // add the mission to the dictionary
                if (AllMissionsByLevel.ContainsKey(i))
                {
                    AllMissionsByLevel[i].Add(mission);
                }
                else
                {
                    AllMissionsByLevel.Add(i, new List<Mission> { mission });
                }
            }
        }
        return AllMissionsByLevel;
    }

}
