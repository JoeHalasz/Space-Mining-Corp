using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class WorldManager : MonoBehaviour
{
    int seed = 123456789;

    public int getSeed() { return seed; }
    public void setSeed(int value) { seed = value; }
    // make a hashset for all the removed asteroids
    public HashSet<Vector3> removedAsteroids = new HashSet<Vector3>();
    
    // make a dictionary for all the edited asteroids where the key is Vector3 and the value is a list of edits
    public Dictionary<Vector3, List<List<int>>> editedAsteroids = new Dictionary<Vector3, List<List<int>>>();

    AsteroidSpawnManager asteroidSpawnManager;

    Vector3 currentWorldOffset = new Vector3(0, 0, 0);
    GameObject allMovableObjects;

    GameObject player;

    public Vector3 getCurrentWorldOffset()
    {
        return currentWorldOffset;
    }

    public void OffsetWorldBy(Vector3 offset)
    {
        allMovableObjects.transform.position += offset;
        currentWorldOffset += offset;
    }

    void Start()
    {
        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();
        allMovableObjects = GameObject.Find("All Movable Objects");
        player = GameObject.FindGameObjectWithTag("Player");
        // check the players pos every 30 seconds
        InvokeRepeating("offsetWorldIfNecessary", 0, 3);
        asteroidSpawnManager.StartAfterWorldManagerSetUp();
    }

    void offsetWorldIfNecessary()
    {
        // if the player is too far from the center of the world, move the entire world so that the player is in the center again
        if (Vector3.Distance(player.transform.position, new Vector3(0, 0, 0)) > 25000)
        {
            OffsetWorldBy(player.transform.position * -1);
        }
    }

    void Update()
    {
        // when the user presses F5, save the game and F9 to load the game
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save("test");
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Load("test");
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            OffsetWorldBy(new Vector3(50000, 0, 0));
        }

    }

    public void Save(string name)
    {
        // time it
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        
        getRemovedAsteroids();
        getEditedAsteroids();

        if(!Directory.Exists("saves"))
        {
            Directory.CreateDirectory("saves");
        }
        // save a file under saves/{name}.dat
        string filePath = "saves/" + name + ".dat";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream file = File.Create(filePath);
        if (File.Exists(filePath))
        {
            Debug.Log("File created successfully");
        }
        else
        {
            Debug.Log("File creation failed");
        }
        // save the seed in binary
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, seed);
        // save the player data
        savePlayer(file);
        // save the asteroid data
        saveAllAsteroidData(file);
        file.Close();

        watch.Stop();
        Debug.Log("Saved game in " + watch.ElapsedMilliseconds + "ms");

    }

    public void Load(string name)
    {
        // time it
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();

        // clear the data we have
        removedAsteroids = new HashSet<Vector3>();
        editedAsteroids = new Dictionary<Vector3, List<List<int>>>();

        // load a file under saves/{name}.dat
        string filePath = "saves/" + name + ".dat";
        if (File.Exists(filePath))
        {
            Debug.Log("File exists");
        }
        else
        {
            Debug.Log("File does not exist");
            return;
        }
        FileStream file = File.Open(filePath, FileMode.Open);
        // load the seed, name and name2
        BinaryFormatter bf = new BinaryFormatter();
        seed = (int)bf.Deserialize(file);
        // load the player data
        loadPlayer(file);
        // load the asteroid data
        loadAllAsteroidData(file);
        file.Close();

        setRemovedAsteroids();
        asteroidSpawnManager.reloadAllNonEditedAsteroids();
        setEditedAsteroids();

        watch.Stop();
        Debug.Log("Loaded game in " + watch.ElapsedMilliseconds + "ms");
    }

    // file stream is already open
    void savePlayer(FileStream file)
    {

    }

    // file stream is already open
    void loadPlayer(FileStream file)
    {

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
        foreach (KeyValuePair<Vector3, List<List<int>>> asteroid in editedAsteroids)
        {
            List<int> pos = Vec3ToListInt(asteroid.Key);
            // add the pos to the big list
            bigList.AddRange(pos);
            // add the number of edits to the big list
            bigList.Add(asteroid.Value.Count);
            
            // add the edits to the big list
            foreach (List<int> index in asteroid.Value)
            {
                // add the number of indecies to the big list
                bigList.Add(index.Count);
                // add the indecies to the big list
                bigList.AddRange(index);
            }
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
            Vector3 pos = new Vector3(bigList[count], bigList[count+1], bigList[count+2]);
            count += 3;
            // get the num edits
            int numEdits = bigList[count++];

            // get the edits
            List<List<int>> edits = new List<List<int>>();
            for (int j = 0; j < numEdits; j++)
            {
                // get the num indecies
                int numIndecies = bigList[count++];
                // get the indecies
                List<int> indecies = new List<int>();
                for (int k = 0; k < numIndecies; k++)
                {
                    indecies.Add(bigList[count++]);
                }
                edits.Add(indecies);
            }
            // add the asteroid to the dictionary
            editedAsteroids.Add(pos, edits);
        }
        
    }

    void getRemovedAsteroids(){
        removedAsteroids = asteroidSpawnManager.getAllRemovedAsteroids();
    }
    
    void setRemovedAsteroids()
    {
        asteroidSpawnManager.setAllRemovedAsteroids(removedAsteroids);
    }

    void getEditedAsteroids()
    {
        editedAsteroids = asteroidSpawnManager.getAllEditedAsteroids();
    }

    void setEditedAsteroids()
    {
        asteroidSpawnManager.setAllEditedAsteroids(editedAsteroids);
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