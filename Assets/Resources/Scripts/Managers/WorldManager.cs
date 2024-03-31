using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public enum WorldState
{
    Intro,
    BeforeBeam1,
    BeforeBeam2,
    BeforeFirstWarp,
    AfterFirstWarp,
    BeforeStationDestroyed,
    AfterStationDestroyed
}

public class WorldManager : MonoBehaviour
{

    int seed;

    string currentlyLoadedSave;

    public int getSeed() { return seed; }
    public void setSeed(int value) { seed = value; }
    // make a hashset for all the removed asteroids
    public HashSet<Vector3> removedAsteroids = new HashSet<Vector3>();

    // make a dictionary for all the edited asteroids where the key is Vector3 and the value is a list of edits
    public Dictionary<Vector3, HashSet<int>> editedAsteroids = new Dictionary<Vector3, HashSet<int>>();

    AsteroidSpawnManager asteroidSpawnManager;
    AsteroidFieldGenerator asteroidFieldGenerator;

    Vector3 currentWorldOffset = new Vector3(0, 0, 0);
    GameObject allMovableObjects;

    public WorldState worldState = 0;

    GameObject player;

    public bool inGame = false;

    DialogManager dialogManager;

    public void nextWorldState()
    {
        worldState++;
    }

    public Vector3 getCurrentWorldOffset()
    {
        return currentWorldOffset;
    }

    public Vector3 getObjectTruePosition(Vector3 pos)
    {
        return pos + currentWorldOffset;
    }

    public void OffsetWorldBy(Vector3 offset)
    {
        // round the offset to the nearest 1
        offset.x = Mathf.Round(offset.x);
        offset.y = Mathf.Round(offset.y);
        offset.z = Mathf.Round(offset.z);
        Debug.Log("Offsetting world by " + offset);
        // loop through all child objects and move them by the offset
        foreach (Transform child in allMovableObjects.transform)
        {
            child.position += offset;
        }
        currentWorldOffset += offset;
    }

    bool first = true;

    GameObject enableAfterFirstLoad;
    GameObject mainMenuCanvas;
    GameObject escapeMenuCanvas;
    GameObject escapeMenuGroup;
    GameObject crosshair;
    MenuManager menuManager;
    void Start()
    {
        seed = 123456789; // TODO delete this
        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();
        asteroidFieldGenerator = GameObject.Find("AsteroidField").GetComponent<AsteroidFieldGenerator>();
        allMovableObjects = GameObject.Find("All Movable Objects");
        player = GameObject.FindGameObjectWithTag("Player");
        dialogManager = GetComponent<DialogManager>();
        enableAfterFirstLoad = GameObject.Find("EnableAfterFirstLoad");
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        escapeMenuCanvas = GameObject.Find("EscapeMenuCanvas");
        escapeMenuGroup = GameObject.Find("EscapeMenuGroup");
        crosshair = GameObject.Find("Crosshair");
        menuManager = GetComponent<MenuManager>();
        // check the players pos and offset if too far from the origin
        InvokeRepeating("offsetWorldIfNecessary", 0, 5);
        Startup();
    }

    public bool startDialog(FactionManager faction, GameObject voicedBy)
    {
        return dialogManager.interactedWithFaction(faction, voicedBy);
    }

    void offsetWorldIfNecessary()
    {
        // if the player is too far from the center of the world, move the entire world so that the player is in the center again
        if (Vector3.Distance(player.transform.position, new Vector3(0, 0, 0)) > 25000)
        {
            OffsetWorldBy(player.transform.position * -1);
        }
    }

    void Startup()
    {
        asteroidSpawnManager.StartAfterWorldManagerSetUp(player.transform.position);
        asteroidFieldGenerator.StartAfterWorldManagerSetUp();
    }

    void firstLoadSinceStartup()
    {
        inGame = true;
        player.GetComponent<SimpleRotate>().enabled = false;
        player.GetComponent<PlayerMovement>().enabled = true;
        enableAfterFirstLoad.SetActive(true);
        mainMenuCanvas.SetActive(false);
        crosshair.SetActive(true);
        player.GetComponent<UIManager>().closeAnyUI();
    }

    public void backToMainMenu()
    {
        player.GetComponent<UIManager>().closeAnyUI();
        player.GetComponent<UIManager>().openAnyUI(this.gameObject, true, true, new List<GameObject> { mainMenuCanvas });
        inGame = false;
        player.transform.SetParent(allMovableObjects.transform);
        player.GetComponent<SimpleRotate>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = false;
        enableAfterFirstLoad.SetActive(false);
        mainMenuCanvas.SetActive(true);
        escapeMenuCanvas.SetActive(false);
        crosshair.SetActive(false);
        currentlyLoadedSave = null;
    }

    public void backToGame()
    {
        if (!inGame)
        {
            Debug.LogError("Cannot go back to game when not in game");
            return;
        }
        player.GetComponent<UIManager>().closeAnyUI();

    }

    public List<char> createNewWorld(string name, int seed)
    {
        List<char> invalidChars = checkSaveName(name);
        if (invalidChars.Count > 0)
        {
            return invalidChars;
        }
        firstLoadSinceStartup();
        currentlyLoadedSave = name;
        GetComponent<CreateNewWorld>().CreateWorld(seed);
        return invalidChars;
    }

    void Update()
    {
        if (first)
        {
            first = false;
            backToMainMenu();
        }
        // when the user presses F5, save the game and F9 to load the game
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (inGame)
                Save();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Load(currentlyLoadedSave);
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            OffsetWorldBy(new Vector3(5000, 0, 0));
        }
        // if esc is pressed, open the escape menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inGame)
            {
                if (!player.GetComponent<UIManager>().getUIOpen())
                {
                    escapeMenuCanvas.SetActive(true);
                    player.GetComponent<UIManager>().openAnyUI(this.gameObject, true, true, new List<GameObject> { escapeMenuGroup });
                    escapeMenuGroup.SetActive(true);
                }
                else
                {
                    player.GetComponent<UIManager>().closeAnyUI();
                    menuManager.turnAllGroupsOff();
                }
            }
        }
    }

    public List<char> checkSaveName(string name)
    {
        if (name == "" || string.IsNullOrWhiteSpace(name))
        {
            return new List<char> { ' ' };
        }
        char[] invalidChars = Path.GetInvalidFileNameChars();
        List<char> invalidCharsList = new List<char>();
        foreach (char c in invalidChars)
        {
            if (name.Contains(c))
            {
                invalidCharsList.Add(c);
            }
        }
        return invalidCharsList;
    }

    public List<char> Save(string newName = null)
    {

        string name;
        if (newName == null)
        {
            name = currentlyLoadedSave;
        }
        else
        {
            name = newName;
        }

        List<char> invalidChars = checkSaveName(name);
        if (invalidChars.Count > 0)
        {
            return invalidChars;
        }

        // time it
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        currentlyLoadedSave = name;
        getRemovedAsteroids();
        getEditedAsteroids();
        if (!Directory.Exists("data"))
        {
            Directory.CreateDirectory("data");
        }
        if (!Directory.Exists("data/saves"))
        {
            Directory.CreateDirectory("data/saves");
        }
        // save a file under data/saves/{name}.dat
        string filePath = "data/saves/" + name + ".dat";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream file = File.Create(filePath);
        if (File.Exists(filePath))
        {
#if UNITY_EDITOR
                Debug.Log("File created successfully [" + filePath + "]");
#endif
        }
        else
        {
#if UNITY_EDITOR
                Debug.LogError("File creation failed");
#endif
            return new List<char>("File creation failed");
        }
        // save the seed in binary
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, seed);
        // save the player data
        savePlayer(file);
        // save the ship data
        saveShip(file);
        // save all factions data
        saveFactions(file);
        // save the asteroid data
        saveAllAsteroidData(file);
        file.Close();

        // create the metadata file
        string metaFilePath = "data/saves/" + name + ".meta";
        if (File.Exists(metaFilePath))
        {
            File.Delete(metaFilePath);
        }
        // format it like this: name;date and time
        string meta = name + ";" + System.DateTime.Now.ToString();
        File.WriteAllText(metaFilePath, meta);

        watch.Stop();
#if UNITY_EDITOR
            Debug.Log("Saved game in " + watch.ElapsedMilliseconds + "ms");
#endif
        return new List<char>();
    }

    public void Load(string name)
    {
        if (!inGame)
            firstLoadSinceStartup();
        currentlyLoadedSave = name;
        // time it
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();

        // clear the data we have
        removedAsteroids = new HashSet<Vector3>();
        editedAsteroids = new Dictionary<Vector3, HashSet<int>>();

        // load a file under data/saves/{name}.dat
        string filePath = "data/saves/" + name + ".dat";
        if (File.Exists(filePath))
        {
#if UNITY_EDITOR
                Debug.Log("File exists");
#endif
        }
        else
        {
#if UNITY_EDITOR
                Debug.Log("File does not exist");
#endif
            return;
        }
        FileStream file = File.Open(filePath, FileMode.Open);
        // load the seed, name and name2
        BinaryFormatter bf = new BinaryFormatter();
        seed = (int)bf.Deserialize(file);
        // load the player data
        loadPlayer(file);
        // load the ship data
        loadShip(file);
        // save all factions data
        loadFactions(file);
        // load the asteroid data
        loadAllAsteroidData(file);
        file.Close();

        watch.Stop();
#if UNITY_EDITOR
            Debug.Log("Loaded game [" + name + "] in " + watch.ElapsedMilliseconds + "ms");
#endif
    }

    // file stream is already open
    void savePlayer(FileStream file)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        BinaryFormatter bf = new BinaryFormatter();
        // save the players position and rotation
        Vector3 playerPos = player.transform.localPosition;
        Vector3 playerRot = player.transform.rotation.eulerAngles;
        // save each float
        bf.Serialize(file, playerPos.x);
        bf.Serialize(file, playerPos.y);
        bf.Serialize(file, playerPos.z);
        bf.Serialize(file, playerRot.x);
        bf.Serialize(file, playerRot.y);
        bf.Serialize(file, playerRot.z);
        // save the players credits
        bf.Serialize(file, playerStats.getCredits());
        // save the players health
        bf.Serialize(file, playerStats.getHealth());
        // save the players inventory
        Inventory playerInventory = player.GetComponent<Inventory>();
        List<ItemPair> inventory = playerInventory.getInventory();
        bf.Serialize(file, inventory);
        // save the players missions
        MissionManager missionManager = player.GetComponent<MissionManager>();
        bf.Serialize(file, missionManager.GetMissions());
        // save the players ship
        bf.Serialize(file, playerStats.playerCurrentShip.name);
    }

    // file stream is already open
    void loadPlayer(FileStream file)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        BinaryFormatter bf = new BinaryFormatter();
        // load the players position and rotation float by float
        Vector3 playerPos = new Vector3((float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file));
        Vector3 playerRot = new Vector3((float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file));
        player.transform.localPosition = playerPos;
        player.transform.rotation = Quaternion.Euler(playerRot);
        // load the players credits
        playerStats.setCredits((float)bf.Deserialize(file));
        // load the players health
        playerStats.setHealth((float)bf.Deserialize(file));
        // load the players inventory
        Inventory playerInventory = player.GetComponent<Inventory>();
        // load the inventory
        List<ItemPair> inventory = (List<ItemPair>)bf.Deserialize(file);
        playerInventory.setInventory(inventory);
        // load the players missions
        MissionManager missionManager = player.GetComponent<MissionManager>();
        missionManager.LoadMissions((List<Mission>)bf.Deserialize(file));
        // load the players ship
        playerStats.playerCurrentShip = GameObject.Find((string)bf.Deserialize(file));
    }
    void saveShip(FileStream file)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        BinaryFormatter bf = new BinaryFormatter();
        Vector3 shipPos = playerStats.playerCurrentShip.transform.localPosition;
        Vector3 shipRot = playerStats.playerCurrentShip.transform.rotation.eulerAngles;
        bf.Serialize(file, shipPos.x);
        bf.Serialize(file, shipPos.y);
        bf.Serialize(file, shipPos.z);
        bf.Serialize(file, shipRot.x);
        bf.Serialize(file, shipRot.y);
        bf.Serialize(file, shipRot.z);
        Inventory shipInventory = playerStats.playerCurrentShip.GetComponent<Inventory>();
        bf.Serialize(file, shipInventory.getInventory());
        ShipManager shipManager = playerStats.playerCurrentShip.GetComponent<ShipManager>();
        bf.Serialize(file, shipManager.GetEnergy());
        bf.Serialize(file, shipManager.GetMaxEnergy());
        bf.Serialize(file, shipManager.GetMaxFuel());
        bf.Serialize(file, shipManager.GetFuel());
        bf.Serialize(file, shipManager.GetNumCargoSlots());
        bf.Serialize(file, shipManager.GetHealth());
        bf.Serialize(file, shipManager.GetMaxHealth());
        bf.Serialize(file, shipManager.GetForwardSpeed());
        bf.Serialize(file, shipManager.GetReverseSpeed());
        bf.Serialize(file, shipManager.GetVerticalTurnSpeed());
        bf.Serialize(file, shipManager.GetHorizontalTurnSpeed());
        bf.Serialize(file, shipManager.GetStrafeSpeed());
        bf.Serialize(file, shipManager.GetShield());
        bf.Serialize(file, shipManager.GetMaxShield());
        bf.Serialize(file, shipManager.GetShieldRegen());
        bf.Serialize(file, shipManager.GetShieldDelay());
    }

    void loadShip(FileStream file)
    {
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        BinaryFormatter bf = new BinaryFormatter();
        Vector3 shipPos = new Vector3((float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file));
        Vector3 shipRot = new Vector3((float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file));
        playerStats.playerCurrentShip.transform.localPosition = shipPos;
        playerStats.playerCurrentShip.transform.rotation = Quaternion.Euler(shipRot);
        Inventory shipInventory = playerStats.playerCurrentShip.GetComponent<Inventory>();
        shipInventory.setInventory((List<ItemPair>)bf.Deserialize(file));
        ShipManager shipManager = playerStats.playerCurrentShip.GetComponent<ShipManager>();
        shipManager.loadState(
            (float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file),
            (float)bf.Deserialize(file), (int)bf.Deserialize(file), (float)bf.Deserialize(file),
            (float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file),
            (float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file),
            (float)bf.Deserialize(file), (float)bf.Deserialize(file), (float)bf.Deserialize(file),
            (float)bf.Deserialize(file)
            );

    }

    void saveFactions(FileStream file)
    {
        // find the game object called factions
        GameObject factionsParentObject = GameObject.Find("Factions");
        // each of its children is a different faction
        List<GameObject> factionsList = new List<GameObject>();
        foreach (Transform child in factionsParentObject.transform)
        {
            factionsList.Add(child.gameObject);
        }
        // save the number of factions
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, factionsList.Count);
        // save each faction
        foreach (GameObject faction in factionsList)
        {
            // save the faction name
            bf.Serialize(file, faction.name);
            // save the faction reputation
            FactionManager factionManager = faction.GetComponent<FactionManager>();
            bf.Serialize(file, factionManager.GetPlayerReputation());
        }
    }

    void loadFactions(FileStream file)
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
        BinaryFormatter bf = new BinaryFormatter();
        int numFactions = (int)bf.Deserialize(file);
        // load each faction
        for (int i = 0; i < numFactions; i++)
        {
            // load the faction name
            string factionName = (string)bf.Deserialize(file);
            // load the faction reputation
            float factionReputation = (float)bf.Deserialize(file);
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



    // file stream is already open
    void saveAllAsteroidData(FileStream file)
    {
        // Only need to save asteroid positions that have been removed, and asteroids that have edits to them.
        // Asteroids will be regenerated on the fly using the seed.
        // the generater will know if there should be an asteroid there based on the position
        // and the rest of the asteroid will be generated the same way every time using the seed and the position.
        // so the only thing that needs to be saved is the edits to the asteroid.

        // to save edits to an asteroid use the removed blocks list. 

        BinaryFormatter bf = new BinaryFormatter();
        // make removed asteroids into a list
        List<Vector3> removedAsteroidsList = new List<Vector3>(removedAsteroids);
        // make removed asteroids into a vector of ints
        List<int> removedAsteroidsVector = ListVec3ToListInt(removedAsteroidsList);
        // save the removed asteroids
        bf.Serialize(file, removedAsteroidsVector);
        // save the amount of edited asteroids
        bf.Serialize(file, editedAsteroids.Count);
        List<int> bigList = new List<int>();
        // save the edited asteroids
        foreach (KeyValuePair<Vector3, HashSet<int>> asteroid in editedAsteroids)
        {
            List<int> pos = Vec3ToListInt(asteroid.Key);
            // add the pos to the big list
            bigList.AddRange(pos);
            // make the hash set into a list and put it in the big list
            List<int> edits = new List<int>(asteroid.Value);
            bigList.Add(edits.Count);
            bigList.AddRange(edits);
        }
        bf.Serialize(file, bigList);


    }

    // file stream is already open
    void loadAllAsteroidData(FileStream file)
    {
        // has to be the same order as save
        BinaryFormatter bf = new BinaryFormatter();
        // load the removed asteroids
        List<int> removedAsteroidsVector = (List<int>)bf.Deserialize(file);
        // make removed asteroids into a list
        List<Vector3> removedAsteroidsList = ListIntToListVec3(removedAsteroidsVector);
        // add the removed asteroids to the hashset
        foreach (Vector3 pos in removedAsteroidsList)
        {
            removedAsteroids.Add(pos);
        }

        // load the edited asteroids
        int numEditedAsteroids = (int)bf.Deserialize(file);
        // load the big list
        List<int> bigList = (List<int>)bf.Deserialize(file);
        // loop through the big list and get the pos, numedits, and edits
        int count = 0;
        for (int i = 0; i < numEditedAsteroids; i++)
        {
            // get the pos
            Vector3 pos = new Vector3(bigList[count], bigList[count + 1], bigList[count + 2]);
            count += 3;
            // get the num edits
            int numEdits = bigList[count++];

            // get the edits
            HashSet<int> edits = new HashSet<int>();
            for (int j = 0; j < numEdits; j++)
            {
                edits.Add(bigList[count++]);
            }
            editedAsteroids.Add(pos, edits);
        }

        asteroidSpawnManager.LoadGame(removedAsteroids, editedAsteroids);

    }

    void getRemovedAsteroids()
    {
        removedAsteroids = asteroidSpawnManager.getAllRemovedAsteroids();
    }

    void getEditedAsteroids()
    {
        editedAsteroids = asteroidSpawnManager.getAllEditedAsteroids();
    }

    List<int> Vec3ToListInt(Vector3 vec)
    {
        List<int> list = new List<int>();
        list.Add((int)vec.x);
        list.Add((int)vec.y);
        list.Add((int)vec.z);
        return list;
    }

    List<int> ListVec3ToListInt(List<Vector3> vecs)
    {
        List<int> vec = new List<int>();
        foreach (Vector3 v in vecs)
        {
            vec.Add((int)v.x);
            vec.Add((int)v.y);
            vec.Add((int)v.z);
        }
        return vec;
    }

    List<Vector3> ListIntToListVec3(List<int> vec)
    {
        List<Vector3> vecs = new List<Vector3>();
        for (int i = 0; i < vec.Count; i += 3)
        {
            vecs.Add(new Vector3(vec[i], vec[i + 1], vec[i + 2]));
        }
        return vecs;
    }

}