using System.Collections.Generic;
using UnityEngine;

public class AsteroidFieldGenerator : MonoBehaviour
{
    GameObject asteroidAreaPrefab;
    GameObject asteroidPrefab;

    // defaults are 1000 asteroids in a 1000m radius

    // change this for more or less asteroids
    [SerializeField]
    int sizeOfPartitions = 1200;

    [SerializeField]
    [Range(1000, 100000)]
    public int radius = 5000;

    public bool allowRandomness = true;

    int height;

    [SerializeField]
    bool SpawnAsteroidField = true;

    // HashSet of all area's real positions
    public Dictionary<Vector3, GameObject> allSpawnAreas = new Dictionary<Vector3, GameObject>();

    public Minerals minerals;

    public AsteroidSpawnManager asteroidSpawnManager;
    WorldManager worldManager;

    LinkedList<GameObject> AsteroidAreaGameObjectQueue = new LinkedList<GameObject>();
    int numAreasInQueue = 0;


    // Start is called before the first frame update
    void Start()
    {
        height = radius / 2;
        GameObject worldManagerObject = GameObject.Find("WorldManager");
        worldManager = worldManagerObject.GetComponent<WorldManager>();
        minerals = worldManagerObject.GetComponent<Minerals>();
        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();

        // every second debug.log how many children this object has
#if UNITY_EDITOR
            InvokeRepeating("DebugChildren", 30, 5);
#endif
    }

    public void StartAfterWorldManagerSetUp()
    {

        if (SpawnAsteroidField)
        {
            // load the prefabs
            asteroidAreaPrefab = Resources.Load<GameObject>("Prefabs/Asteroids/AsteroidArea") as GameObject;
            // pregenerate game objects for asteroid areas
            MakePregeneratedGameObjectsForAsteroidAreas();
            // generate an asteroid field around this object
            SpawnNewArea(worldManager.getObjectTruePosition(transform.position));
        }

    }

    public void destroyAllSpawnAreas()
    {
        List<AsteroidAreaSpawner> allSpawnAreaScripts = new List<AsteroidAreaSpawner>();
        foreach (KeyValuePair<Vector3, GameObject> kvp in allSpawnAreas)
        {
            allSpawnAreaScripts.Add(kvp.Value.GetComponent<AsteroidAreaSpawner>());
        }

        for (int i = 0; i < allSpawnAreaScripts.Count; i++)
        {
            allSpawnAreaScripts[i].destroyAsteroidAndThis();
        }
    }

    void MakeOnePregeneratedAsteroidArea()
    {
        GameObject newAsteroidArea = Instantiate(asteroidAreaPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        newAsteroidArea.SetActive(false);
        newAsteroidArea.transform.SetParent(gameObject.transform, false);
        AsteroidAreaGameObjectQueue.AddFirst(newAsteroidArea);
        numAreasInQueue++;
    }

    void MakePregeneratedGameObjectsForAsteroidAreas()
    {
        int numToPregen = 10000;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        for (int i = 0; i < numToPregen; i++)
        {
            MakeOnePregeneratedAsteroidArea();
        }
        watch.Stop();
#if UNITY_EDITOR
            Debug.Log("Pregenerated " + numToPregen + " spawn area game objects in " + watch.ElapsedMilliseconds / 1000f + "s");
#endif
    }

    void DebugChildren()
    {
        UnityEngine.Debug.Log("AsteroidFieldGenerator has " + transform.childCount + " children");
        // figure out how many of each child there is 
        Dictionary<string, int> childCounts = new Dictionary<string, int>();
        foreach (Transform child in transform)
        {
            // if the child object is active
            if (!child.gameObject.activeSelf)
            {
                continue;
            }
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

    public Vector3 makeSurePosIsOnGrid(Vector3 pos) // EZMOD
    {
        int xOffset = ((int)pos.x) % sizeOfPartitions;
        int yOffset = ((int)pos.y) % sizeOfPartitions;
        int zOffset = ((int)pos.z) % sizeOfPartitions;
        return new Vector3(((int)pos.x) - xOffset, ((int)pos.y) - yOffset, ((int)pos.z) - zOffset);
    }

    public void SpawnNewArea(Vector3 pos)
    {
        Vector3 beforePos = pos;
        pos = makeSurePosIsOnGrid(pos);
        if (beforePos != pos)
        {
            Debug.Log("SpawnNewArea: pos was not on grid, changed from " + beforePos + " to " + pos);
        }
        int negativeRad = -1 * radius;
        int negativeHeight = -1 * height;
        // make AsteroidAreaPrefab
        // GameObject newAsteroidArea = Instantiate(asteroidAreaPrefab, pos, Quaternion.identity) as GameObject;
        if (AsteroidAreaGameObjectQueue.First == null)
        {
            MakeOnePregeneratedAsteroidArea();
        }
        GameObject newAsteroidArea = AsteroidAreaGameObjectQueue.First.Value;
        AsteroidAreaGameObjectQueue.RemoveFirst();
        numAreasInQueue--;
#if UNITY_EDITOR
            // Debug.Log("AsteroidAreaGameObjectQueue count: " + numAreasInQueue);
#endif
        // set the parent to this object
        newAsteroidArea.transform.SetParent(gameObject.transform, false);
        // Debug.Log("pos is " + pos + " and worldManager.getCurrentWorldOffset() is " + worldManager.getCurrentWorldOffset() + " so the new pos is " + (pos + worldManager.getCurrentWorldOffset()));
        newAsteroidArea.transform.position = pos + worldManager.getCurrentWorldOffset();
        newAsteroidArea.SetActive(true);
        AsteroidAreaSpawner asteroidAreaSpawner = newAsteroidArea.GetComponent<AsteroidAreaSpawner>();
        // set newAsteroidAreas density, radius, and height
        asteroidAreaSpawner.radius = (radius / ((radius - negativeRad) / sizeOfPartitions));
        asteroidAreaSpawner.height = (height / ((height - negativeHeight) / sizeOfPartitions));

        // if it exists, delete the old one
        if (allSpawnAreas.ContainsKey(pos))
        {
            allSpawnAreas[pos].GetComponent<AsteroidAreaSpawner>().destroyAsteroidAndThis();
        }
        asteroidAreaSpawner.GenerateAsteroids(asteroidSpawnManager, allowRandomness, worldManager);
        // add to the Dictionary
        asteroidAreaSpawner.addedAtPos = pos;
        allSpawnAreas.Add(pos, newAsteroidArea);
    }

    public void SpawnMoreAreasAt(Vector3 middlePos)
    {
        int s = sizeOfPartitions;
        // if the 6 spots around middlePos are not in the Dictionary, spawn a new area there and add it to the Dictionary
        // if they are in the Dictionary, do nothing
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(s, 0, 0)))
            SpawnNewArea(middlePos + new Vector3(s, 0, 0));
        if (!allSpawnAreas.ContainsKey(middlePos + new Vector3(-1 * s, 0, 0)))
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
        // if it exists, delete the old one
        if (allSpawnAreas.ContainsKey(pos))
        {
            AsteroidAreaGameObjectQueue.AddFirst(allSpawnAreas[pos]);
            numAreasInQueue++;
#if UNITY_EDITOR
                // Debug.Log("AsteroidAreaGameObjectQueue count: " + numAreasInQueue);
#endif
            allSpawnAreas[pos].SetActive(false);
            allSpawnAreas.Remove(pos);
        }
    }

}

