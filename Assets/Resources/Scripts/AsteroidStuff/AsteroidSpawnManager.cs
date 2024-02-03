using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawnManager : MonoBehaviour
{

    List<Vector3> AsteroidPositionsSpawnQueue = new List<Vector3>();
    Minerals minerals = new Minerals();
    [SerializeField]
    GameObject asteroidPrefab;
    private bool lastEmptyCheck = true;
    public bool initialLoadFinished = false;

    List<GameObject> AllPregeneratedAsteroids = new List<GameObject>();
    List<GameObject> AllPregeneratedBigAsteroids = new List<GameObject>();
    Dictionary<Vector3, GameObject> allSpawnedAsteroids = new Dictionary<Vector3, GameObject>();
    int totalDespawnedAsteroidsSinceLastUnload = 0;
    Dictionary<Vector3, List<List<int>>> allEditedAsteroids = new Dictionary<Vector3, List<List<int>>>();
    public Dictionary<Vector3, List<List<int>>> getAllEditedAsteroids() { return allEditedAsteroids; }
    HashSet<Vector3> allEditedAsteroidsBeforeSave = new HashSet<Vector3>();
    public HashSet<Vector3> getAllEditedAsteroidsBeforeSave() { return allEditedAsteroidsBeforeSave; }
    HashSet<Vector3> allRemovedAsteroids = new HashSet<Vector3>();
    public HashSet<Vector3> getAllRemovedAsteroids() { return allRemovedAsteroids; }
    LinkedList<GameObject> AsteroidGameObjectQueue = new LinkedList<GameObject>();
    int numAsteroidsInQueue = 0;

    WorldManager worldManager;
    

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    int seed;
    public int getSeed() { return seed; }

    public void AddToQueue(Vector3 pos)
    {
        AsteroidPositionsSpawnQueue.Add(pos);
    }

    public void addRemovedAsteroid(Vector3 pos)
    {
        Debug.Log("Removing asteroid at " + pos);
        allRemovedAsteroids.Add(pos);
        if (allEditedAsteroids.ContainsKey(pos))
        {
            allEditedAsteroids.Remove(pos);
        }
        if (allEditedAsteroidsBeforeSave.Contains(pos))
        {
            allEditedAsteroidsBeforeSave.Remove(pos);
        }
        destroyAsteroidAt(pos);
    }

    private void destoryAsteroid(Vector3 pos)
    {
        // Destroy(allSpawnedAsteroids[pos]);
        allSpawnedAsteroids[pos].SetActive(false);
        AsteroidGameObjectQueue.AddLast(allSpawnedAsteroids[pos]);
        numAsteroidsInQueue++;
        Debug.Log("AsteroidGameObjectQueue count: " + numAsteroidsInQueue);
        allSpawnedAsteroids.Remove(pos);
        
        totalDespawnedAsteroidsSinceLastUnload++;
        if (totalDespawnedAsteroidsSinceLastUnload > 500)
        {
            Debug.Log("Unloading unused assets");
            totalDespawnedAsteroidsSinceLastUnload = 0;
            //Resources.UnloadUnusedAssets();
        }
    }

    public void setAllRemovedAsteroids(HashSet<Vector3> allRemovedAsteroids) {
        this.allRemovedAsteroids = allRemovedAsteroids;
        // for each removed asteroid, if its spawned in then remove it
        foreach (Vector3 pos in allRemovedAsteroids)
        {
            if (allSpawnedAsteroids.ContainsKey(pos))
            {
                destoryAsteroid(pos);
            }
        }
    }

    // this should only happen when loading a game
    public void setAllEditedAsteroids(Dictionary<Vector3, List<List<int>>> allEditedAsteroids)
    {
        this.allEditedAsteroids = allEditedAsteroids;
        // for each edited asteroid, if its spawned in then add the edits to it
        foreach (KeyValuePair<Vector3, List<List<int>>> entry in allEditedAsteroids)
        {
            if (allSpawnedAsteroids.ContainsKey(entry.Key))
            {
                allSpawnedAsteroids[entry.Key].GetComponent<AsteroidGenerator>().setIndecies(entry.Value);
            }
        }
        allEditedAsteroidsBeforeSave = new HashSet<Vector3>();
    }

    public void setIndeciesForAsteroid(Vector3 pos, List<List<int>> indecies)
    {
        if (allEditedAsteroids.ContainsKey(pos))
        {
            allEditedAsteroids[pos] = indecies;
        }
        else
        {
            allEditedAsteroids.Add(pos, indecies);
        }
        if (!allEditedAsteroidsBeforeSave.Contains(pos))
        {
            allEditedAsteroidsBeforeSave.Add(pos);
        }
    }

    public void destroyAsteroidAt(Vector3 pos)
    {
        if (allSpawnedAsteroids.ContainsKey(pos))
        {
            destoryAsteroid(pos);
        }
    }

    public void reloadAllNonEditedAsteroids()
    {
        // if an asteroid is spawned in, but is not in the allEditedAsteroids dictionary then regenerate it
        foreach (Vector3 entry in allEditedAsteroidsBeforeSave)
        {
            if (allSpawnedAsteroids.ContainsKey(entry))
            {
                allSpawnedAsteroids[entry].GetComponent<AsteroidGenerator>().regenerateAsteroid();
            }
        }
    }

    void Start()
    {
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        minerals.SetUp();

    }

    // should only be called once when the world is set up
    public void StartAfterWorldManagerSetUp()
    {
        seed = worldManager.getSeed();
        Random.InitState(getSeed());
        MakePregeneratedAsteroids();
        MakePregeneratedGameObjectsForAsteroids();
        IEnumerator coroutine = SpawnAsteroidFromQueue();
        StartCoroutine(coroutine);
        Debug.Log("Seed: " + seed);
    }

    // this create all the asteroid presets
    // make sure to call this right after Random.InitState(seed)
    // the totalToPregen will be the amount of different asteroids there are in the world for that seed
    void MakePregeneratedAsteroids()
    {
        int totalToPregen = 200;
        // time this
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        // make normal asteroids
        for (int i = 0; i < totalToPregen; i++)
        {
            AllPregeneratedAsteroids.Add(GenerateOneAsteroid(new Vector3(0, 0, 0), false));
        }
        watch.Stop();
        Debug.Log("Pregenerated " + totalToPregen + " small asteroids in " + watch.ElapsedMilliseconds / 1000f + "s");
        // make big asteroids
        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < totalToPregen/4; i++)
        {
            AllPregeneratedBigAsteroids.Add(GenerateOneAsteroid(new Vector3(0, 0, 0), true));
        }
        watch.Stop();
        Debug.Log("Pregenerated " + totalToPregen/4 + " big asteroids in " + watch.ElapsedMilliseconds / 1000f + "s");
        
    }

    // this will create all the asteroid game objects to use when copying an asteroid
    void MakePregeneratedGameObjectsForAsteroids()
    {
        int numToPregen = 5000;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        GameObject fakeAsteroid = GenerateOneAsteroid(new Vector3(0, 0, 0), false, false);
        for (int i = 0; i < numToPregen; i++)
        {
            GameObject newAsteroid = Instantiate(fakeAsteroid, new Vector3(0,5,0), Quaternion.identity) as GameObject;
            newAsteroid.SetActive(false);
            newAsteroid.transform.SetParent(AllPregeneratedAsteroids[0].transform.parent, false);
            AsteroidGameObjectQueue.AddLast(newAsteroid);
            numAsteroidsInQueue++;
        }
        watch.Stop();
        Debug.Log("Pregenerated " + numToPregen + " asteroid game objects in " + watch.ElapsedMilliseconds / 1000f + "s");
        Debug.Log("AsteroidGameObjectQueue count: " + AsteroidGameObjectQueue.Count);
    }

    private IEnumerator SpawnAsteroidFromQueue()
    {
        stopwatch.Start();
        bool printed = false;
        bool printQueueStatus = false;
        while (true)
        {
            // if there is something in the queue 
            if (AsteroidPositionsSpawnQueue.Count != 0)
            {
                GameObject newAsteroid = null;
                // make sure this asteroids position isn't in the removed asteroids hashset
                if (allSpawnedAsteroids.ContainsKey(AsteroidPositionsSpawnQueue[0]) || allRemovedAsteroids.Contains(AsteroidPositionsSpawnQueue[0]))
                {
                    // remove and continue
                    AsteroidPositionsSpawnQueue.RemoveAt(0);
                    lastEmptyCheck = false;
                    continue;
                }

                if (AsteroidPositionsSpawnQueue[0] != null)
                    newAsteroid = CopyOneAsteroid(AsteroidPositionsSpawnQueue[0]);
                if (newAsteroid != null){
                    allSpawnedAsteroids.Add(AsteroidPositionsSpawnQueue[0], newAsteroid);
                    if (allEditedAsteroids.ContainsKey(AsteroidPositionsSpawnQueue[0]))
                    {
                        newAsteroid.GetComponent<AsteroidGenerator>().setIndecies(allEditedAsteroids[AsteroidPositionsSpawnQueue[0]]);
                    }
                }
                else
                {
                    Debug.LogError("Failed to copy asteroid!");
                }
                AsteroidPositionsSpawnQueue.RemoveAt(0);
                lastEmptyCheck = false;
            }
            else
            {
                if (!lastEmptyCheck)
                {
                    if (printed && printQueueStatus)
                        Debug.Log("Queue emptied");
                    printed = false;
                    lastEmptyCheck = true;
                }
                yield return 0;
            }
            if (AsteroidPositionsSpawnQueue.Count % 10 == 0 && AsteroidPositionsSpawnQueue.Count != 0){
                if (printQueueStatus)
                    Debug.Log("Num asteroids left in queue " + AsteroidPositionsSpawnQueue.Count);
                printed = true;
            }

            if (!initialLoadFinished && AsteroidPositionsSpawnQueue.Count == 1)
            { // cant be 0 because it starts at 0
                Debug.Log("Initial load finished");
                initialLoadFinished = true;
                stopwatch.Stop();
                Debug.Log("Initial load took: " + stopwatch.ElapsedMilliseconds/1000f + "s");
            }

            if (initialLoadFinished && AsteroidPositionsSpawnQueue.Count % 50 == 0)
                // wait 7 frames
                yield return 0;
            else if (AsteroidPositionsSpawnQueue.Count % 1000 == 0)
                yield return 0;
                
        }
    }

    GameObject GenerateOneAsteroid(Vector3 position, bool isBig, bool generate=true)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity) as GameObject;
        
        newAsteroid.tag = "Asteroid";
        // set the parent to this objects parent
        newAsteroid.transform.SetParent(gameObject.transform.parent, false);
        // random mineral based on the world position

        Item mineralType = minerals.GetMineralTypeFromPos(position, isBig);

        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = mineralType;
        newAsteroid.GetComponent<AsteroidGenerator>().isBig = isBig;

        newAsteroid.GetComponent<AsteroidGenerator>().increment = 1.1f;

        // add minerals.GetMineralByName("Stone").getMaterial() to the materials array
        newAsteroid.GetComponent<Renderer>().materials = new Material[] { mineralType.getMaterial(), minerals.GetMineralByName("Stone").getMaterial() };

        if (generate)
        {
            newAsteroid.GetComponent<AsteroidGenerator>().Generate();    
        }
        newAsteroid.SetActive(false);
        return newAsteroid;
    }

    GameObject CopyOneAsteroid(Vector3 position)
    {
        Random.InitState((int)(Mathf.Abs((getSeed()+1)/1000 + (position.x*10 + position.y*100 + position.z*1000))));
        bool isBig = Random.Range(0, 10) == 0;
        GameObject asteroidToCopy;
        int asteroidNumber;
        if (isBig)
        {
            asteroidNumber = Random.Range(0, AllPregeneratedBigAsteroids.Count);
            asteroidToCopy = AllPregeneratedBigAsteroids[asteroidNumber];
        }
        else 
        {
            asteroidNumber = Random.Range(0, AllPregeneratedAsteroids.Count);
            asteroidToCopy = AllPregeneratedAsteroids[asteroidNumber];
        }
        // make a copy of the asteroid
        
        // GameObject newAsteroid = Instantiate(asteroidToCopy, position, Quaternion.identity) as GameObject;
        // get the first asteroid from the AsteroidGameObjectQueue
        GameObject newAsteroid = AsteroidGameObjectQueue.First.Value;
        if (newAsteroid == null)
        {
            Debug.LogError("AsteroidGameObjectQueue is empty! Add more on load");
            return null;
        }
        AsteroidGameObjectQueue.RemoveFirst();
        numAsteroidsInQueue--;
        Debug.Log("AsteroidGameObjectQueue count: " + numAsteroidsInQueue);
        newAsteroid.transform.localPosition = position;
        
        newAsteroid.transform.SetParent(asteroidToCopy.transform.parent, false);
        AsteroidGenerator o = asteroidToCopy.GetComponent<AsteroidGenerator>();
        if (!newAsteroid.GetComponent<AsteroidGenerator>().copyAll(o.mineralType, o.isBig, o.points, o.outsidePoints, o.pointColors,
                        o.pointToCubes, o.mesh, o.increment, o.pointsSetPositions, o.cubesPointIndecies, 
                        o.originalCubesPointIndecies, o.allVerts, o.allTris, o.allNormals, gameObject.GetComponent<AsteroidSpawnManager>()))
        {
            Debug.LogError("Failed to copy asteroid number" + asteroidNumber + " at " + position);
            Destroy(newAsteroid);
            return null;
        }
        newAsteroid.GetComponent<AsteroidGenerator>().setOriginalPosition(position);
        
        // make it a different mineral
        Item mineralType = minerals.GetMineralTypeFromPos(newAsteroid.transform.localPosition, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = mineralType;
        newAsteroid.GetComponent<Renderer>().materials = new Material[] { mineralType.getMaterial(), minerals.GetMineralByName("Stone").getMaterial() };
        
        newAsteroid.GetComponent<AsteroidGenerator>().ApplyMesh();
        
        newAsteroid.SetActive(true);

        return newAsteroid;
    }

    static IEnumerator WaitForFrames(int frameCount)
    {
        if (frameCount <= 0)
        {
            yield break;
        }

        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }
}