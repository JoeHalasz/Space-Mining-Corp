using System.Collections.Generic;
using UnityEngine;

public class AsteroidFieldGenerator : MonoBehaviour
{
    GameObject asteroidAreaPrefab;
    GameObject asteroidPrefab;

    // defaults are 1000 asteroids in a 1000m radius
    
    // change this for more or less asteroids
    int sizeOfPartitions = 390;

    [SerializeField]
    [Range(1000, 100000)]
    public int radius = 1000;

    [SerializeField]
    [Range(1000, 100000)]
    public int height = 10000;

    [SerializeField]
    bool SpawnAsteroidField = true;

    // HashSet of all area positions
    public Dictionary<Vector3, GameObject> allSpawnAreas = new Dictionary<Vector3, GameObject>();

    public Minerals minerals;

    AsteroidSpawnManager asteroidSpawnManager;
    WorldManager worldManager;

    LinkedList<GameObject> AsteroidAreaGameObjectQueue = new LinkedList<GameObject>();
    int numAreasInQueue = 0;


    // Start is called before the first frame update
    void Start()
    {
        minerals = new Minerals();
        minerals.SetUp();
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();

        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();

        if (SpawnAsteroidField)
        {
            // load the prefabs
            asteroidAreaPrefab = Resources.Load<GameObject>("Prefabs/Asteroids/AsteroidArea") as GameObject;
            // pregenerate game objects for asteroid areas
            MakePregeneratedGameObjectsForAsteroidAreas();
            // generate an asteroid field around this object
            SpawnNewArea(transform.localPosition);
        }

        // every second debug.log how many children this object has
        // InvokeRepeating("DebugChildren", 1, 1);
    }

    void MakePregeneratedGameObjectsForAsteroidAreas()
    {
        int numToPregen = 5000;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        for (int i = 0; i < numToPregen; i++)
        {
            GameObject newAsteroidArea = Instantiate(asteroidAreaPrefab, new Vector3(0,0,0), Quaternion.identity) as GameObject;
            newAsteroidArea.SetActive(false);
            newAsteroidArea.transform.SetParent(gameObject.transform, false);
            AsteroidAreaGameObjectQueue.AddLast(newAsteroidArea);
            numAreasInQueue++;
        }
        watch.Stop();
        Debug.Log("Pregenerated " + numToPregen + " spawn area game objects in " + watch.ElapsedMilliseconds / 1000f + "s");
        
    }

    void DebugChildren()
    {
        UnityEngine.Debug.Log("AsteroidFieldGenerator has " + transform.childCount + " children");
        // figure out how many of each child there is 
        Dictionary<string, int> childCounts = new Dictionary<string, int>();
        foreach (Transform child in transform)
        {
            if (childCounts.ContainsKey(child.name))
            {
                childCounts[child.name]++;
            }
            else
            {
                childCounts[child.name] = 1;
            }
        }
        foreach (KeyValuePair<string, int> kvp in childCounts)
        {
            UnityEngine.Debug.Log(kvp.Key + " : " + kvp.Value);
        }
    }


    void SpawnNewArea(Vector3 pos)
    {
        int negativeRad = -1 * radius;
        int negativeHeight = -1*height;
        // make AsteroidAreaPrefab
        // GameObject newAsteroidArea = Instantiate(asteroidAreaPrefab, pos, Quaternion.identity) as GameObject;
        if (AsteroidAreaGameObjectQueue.First == null)
        {
            Debug.LogError("AsteroidAreaGameObjectQueue is empty! Add more on load. There should be " + numAreasInQueue);
            return;
        }
        GameObject newAsteroidArea = AsteroidAreaGameObjectQueue.First.Value;
        AsteroidAreaGameObjectQueue.RemoveFirst();
        numAreasInQueue--;
        Debug.Log("AsteroidAreaGameObjectQueue count: " + numAreasInQueue);
        
        newAsteroidArea.transform.position = pos;
        newAsteroidArea.SetActive(true);
        
        // set the parent to this object
        newAsteroidArea.transform.SetParent(gameObject.transform, false);
        // set newAsteroidAreas density, radius, and height
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().radius = (radius / ((radius - negativeRad) / sizeOfPartitions));
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().height = (height / ((height - negativeHeight) / sizeOfPartitions));
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().GenerateAsteroids(asteroidSpawnManager);
        // add to the Dictionary
        allSpawnAreas.Add(pos, newAsteroidArea);
    }

    public void SpawnMoreAreasAt(Vector3 middlePos)
    {
        int s = sizeOfPartitions;
        // if the 6 spots around middlePos are not in the Dictionary, spawn a new area there and add it to the Dictionary
        // if they are in the Dictionary, do nothing
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(s, 0, 0)))
            SpawnNewArea(middlePos + new Vector3(s, 0, 0));
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(-1*s, 0, 0)))
            SpawnNewArea(middlePos + new Vector3(-1 * s, 0, 0));
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(0, s, 0)))
            SpawnNewArea(middlePos + new Vector3(0, s, 0));
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(0, -1 * s, 0)))
            SpawnNewArea(middlePos + new Vector3(0, -1 * s, 0));
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(0, 0, s)))
            SpawnNewArea(middlePos + new Vector3(0, 0, s));
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(0, 0, -1 * s)))
            SpawnNewArea(middlePos + new Vector3(0, 0, -1 * s));

    }

    public void removeSpawnAreaAt(Vector3 pos)
    {
        AsteroidAreaGameObjectQueue.AddLast(allSpawnAreas[pos]);
        numAreasInQueue++;
        Debug.Log("AsteroidAreaGameObjectQueue count: " + numAreasInQueue);
        allSpawnAreas[pos].SetActive(false);
        allSpawnAreas.Remove(pos);
    }

}

