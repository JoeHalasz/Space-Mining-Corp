using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewWorld : MonoBehaviour
{
    WorldManager worldManager;
    GameObject player;
    GameObject defaultShip;
    AsteroidSpawnManager asteroidSpawnManager;

    void Start()
    {
        worldManager = GetComponent<WorldManager>();
        player = GameObject.Find("Player");
        defaultShip = GameObject.Find("Ship 1");
        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();
    }
    public void CreateWorld(string name, int seed = -1)
    {
        // generate a seed if none is provided
        if (seed == -1)
        {
            seed = Random.Range(0, 1000000000);
        }
        // use the seed to do these things in order (if more things are added, add them to the end so world gen isn't destroyed)
        // generate 500 different asteroids for the asteroid field
        // create an equation for where the asteroids should be generated

        // save the seed to the manager so it can be used to generate things in the world, and so the world can be loaded
        worldManager.setSeed(seed);
        loadBlankWorld();
        worldManager.Save(name);
    }


    void loadBlankWorld()
    {
        // do the same thing as WorldManager load but with defaults
        loadPlayer();
        loadShip();
        loadFactions();
        loadAllAsteroidData();
    }

    void loadPlayer()
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        Vector3 playerPos = new Vector3(0, 10, 0);
        Vector3 playerRot = new Vector3(0, 0, 0);
        player.transform.localPosition = playerPos;
        player.transform.rotation = Quaternion.Euler(playerRot);
        playerStats.setCredits(0);
        playerStats.setHealth(100);
        Inventory playerInventory = player.GetComponent<Inventory>();
        List<ItemPair> inventory = new List<ItemPair>();
        playerInventory.setInventory(inventory);
        MissionManager missionManager = player.GetComponent<MissionManager>();
        missionManager.LoadMissions(new List<Mission>());
        playerStats.playerCurrentShip = defaultShip;
    }

    void loadShip()
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        Vector3 shipPos = new Vector3(0, 10, 100);
        Vector3 shipRot = new Vector3(0, 180, 0);
        playerStats.playerCurrentShip.transform.localPosition = shipPos;
        playerStats.playerCurrentShip.transform.rotation = Quaternion.Euler(shipRot);
        Inventory shipInventory = playerStats.playerCurrentShip.GetComponent<Inventory>();
        shipInventory.setInventory(new List<ItemPair>());
        ShipManager shipManager = playerStats.playerCurrentShip.GetComponent<ShipManager>();
        shipManager.loadState(0, 100, 0, 100, -1, 100, 100, 1, 1, 1, 1, 1, 0, 100, 1, 5);
    }

    void loadFactions()
    {
        // find the game object called factions
        GameObject factionsParentObject = GameObject.Find("Factions");
        // each of its children is a different faction
        List<GameObject> factionsList = new List<GameObject>();
        foreach (Transform child in factionsParentObject.transform)
        {
            factionsList.Add(child.gameObject);
        }
        // load the number of factions
        int numFactions = factionsList.Count;
        // load each faction
        for (int i = 0; i < numFactions; i++)
        {
            // load the faction name
            string factionName = factionsList[i].name;
            // load the faction reputation
            float factionReputation = 0;
            // find the faction in the list
            foreach (GameObject faction in factionsList)
            {
                if (faction.name == factionName)
                {
                    FactionManager factionManager = faction.GetComponent<FactionManager>();
                    factionManager.SetPlayerReputation(factionReputation);
                }
            }
        }
    }

    void loadAllAsteroidData()
    {
        asteroidSpawnManager.LoadGame(new HashSet<Vector3>(), new Dictionary<Vector3, HashSet<int>>());
    }
}
