using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class WorldManager : MonoBehaviour
{
    public int seed;

    // make a hashset for all the removed asteroids
    public HashSet<Vector3> removedAsteroids = new HashSet<Vector3>();
    
    // make a dictionary for all the edited asteroids where the key is Vector3 and the value is a list of edits
    public Dictionary<Vector3, List<Vector3>> editedAsteroids = new Dictionary<Vector3, List<Vector3>>();
    

    void Start()
    {
        TestSave();
    }

    void TestSave()
    {
        seed = 123456789;
        // add 100 random removed asteroids
        for (int i = 0; i < 1000; i++)
        {
            removedAsteroids.Add(new Vector3(Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000)));
        }
        
        // add 100 random edited asteroids
        for (int i = 0; i < 10000; i++)
        {
            Vector3 pos = new Vector3(Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000));
            List<Vector3> edits = new List<Vector3>();
            for (int j = 0; j < 50; j++)
            {
                edits.Add(new Vector3(Random.Range(0, 1000), Random.Range(0, 1000), Random.Range(0, 1000)));
            }
            editedAsteroids.Add(pos, edits);
        }
        
        // save and load
        // time how long it takes to save and load
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Save("test");
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("Save time: " + elapsedMs);
        // clear the data we have 
        seed = 0;
        removedAsteroids.Clear();
        editedAsteroids.Clear();
        // load the data
        watch = System.Diagnostics.Stopwatch.StartNew();
        Load("test");
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;
        Debug.Log("Load time: " + elapsedMs);
        // print the data
        Debug.Log("Seed: " + seed);
        Debug.Log("num Removed Asteroids: " + removedAsteroids.Count);
        
        Debug.Log("num Edited Asteroids: " + editedAsteroids.Count);
        
        

    }

    public void Save(string name)
    {
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

    }

    public void Load(string name)
    {
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
    }

    public void addRemovedAsteroid(Vector3 pos)
    {
        removedAsteroids.Add(pos);
    }

    public void addEditedAsteroid(Vector3 pos, List<Vector3> removedPoints)
    {
        editedAsteroids.Add(pos, removedPoints);
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
        // Format: 
            // position of asteroid
            // num of edits
            // list of edits
        
        
        BinaryFormatter bf = new BinaryFormatter();
        // make removed asteroids into a list
        List<Vector3> removedAsteroidsList = new List<Vector3>(removedAsteroids);
        // make removed asteroids into a vector of floats
        List<float> removedAsteroidsVector = ListVec3ToListFloat(removedAsteroidsList);
        // save the removed asteroids
        bf.Serialize(file, removedAsteroidsVector);
        // save the amount of edited asteroids
        bf.Serialize(file, editedAsteroids.Count);
        // save the edited asteroids
        foreach (KeyValuePair<Vector3, List<Vector3>> asteroid in editedAsteroids)
        {
            // Vec3ToListFloat for the pos of the asteroid
            List<float> pos = Vec3ToListFloat(asteroid.Key);
            // Vec3ToListFloat for the edits
            List<float> edits = ListVec3ToListFloat(asteroid.Value);
            // save the pos
            bf.Serialize(file, pos);
            // save the edits
            bf.Serialize(file, edits);
        }
        
        
    }

    // file stream is already open
    void loadAllAsteroidData(FileStream file)
    {
        // has to be the same order as save
        BinaryFormatter bf = new BinaryFormatter();
        // load the removed asteroids
        List<float> removedAsteroidsVector = (List<float>)bf.Deserialize(file);
        // make removed asteroids into a list
        List<Vector3> removedAsteroidsList = ListFloatToListVec3(removedAsteroidsVector);
        // add the removed asteroids to the hashset
        foreach (Vector3 pos in removedAsteroidsList)
        {
            removedAsteroids.Add(pos);
        }

        // load the edited asteroids
        int numEditedAsteroids = (int)bf.Deserialize(file);
        for (int i = 0; i < numEditedAsteroids; i++)
        {
            // load the pos
            List<float> pos = (List<float>)bf.Deserialize(file);
            // load the edits
            List<float> edits = (List<float>)bf.Deserialize(file);
            // make the pos into a vector3
            Vector3 posVec3 = new Vector3(pos[0], pos[1], pos[2]);
            // make the edits into a list of vector3
            List<Vector3> editsVec3 = ListFloatToListVec3(edits);
            // add the asteroid to the dictionary
            editedAsteroids.Add(posVec3, editsVec3);
        }
        
        
    }

    List<float> Vec3ToListFloat(Vector3 vec)
    {
        List<float> list = new List<float>();
        list.Add(vec.x);
        list.Add(vec.y);
        list.Add(vec.z);
        return list;
    }

    List<float> ListVec3ToListFloat(List<Vector3> vecs)
    {
        List<float> vec = new List<float>();
        foreach (Vector3 v in vecs)
        {
            vec.Add(v.x);
            vec.Add(v.y);
            vec.Add(v.z);
        }
        return vec;
    }

    List<Vector3> ListFloatToListVec3(List<float> vec)
    {
        List<Vector3> vecs = new List<Vector3>();
        for (int i = 0; i < vec.Count; i += 3)
        {
            vecs.Add(new Vector3(vec[i], vec[i + 1], vec[i + 2]));
        }
        return vecs;
    }

}