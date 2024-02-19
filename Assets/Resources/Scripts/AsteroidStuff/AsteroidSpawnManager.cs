using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawnManager : MonoBehaviour
{

    List<Vector3> AsteroidPositionsSpawnQueue = new List<Vector3>();
    Minerals minerals;
    [SerializeField]
    GameObject asteroidPrefab;
    private bool lastEmptyCheck = true;
    public bool initialLoadFinished = false;

    List<GameObject> AllPregeneratedAsteroids = new List<GameObject>();
    List<GameObject> AllPregeneratedBigAsteroids = new List<GameObject>();
    Dictionary<Vector3, GameObject> allSpawnedAsteroids = new Dictionary<Vector3, GameObject>();
    int totalDespawnedAsteroidsSinceLastUnload = 0;
    Dictionary<Vector3, HashSet<int>> allEditedAsteroids = new Dictionary<Vector3, HashSet<int>>();
    public Dictionary<Vector3, HashSet<int>> getAllEditedAsteroids() { return allEditedAsteroids; }
    HashSet<Vector3> allEditedAsteroidsBeforeSave = new HashSet<Vector3>();
    public HashSet<Vector3> getAllEditedAsteroidsBeforeSave() { return allEditedAsteroidsBeforeSave; }
    HashSet<Vector3> allRemovedAsteroids = new HashSet<Vector3>();
    public HashSet<Vector3> getAllRemovedAsteroids() { return allRemovedAsteroids; }
    LinkedList<GameObject> AsteroidGameObjectQueue = new LinkedList<GameObject>();
    int numAsteroidsInQueue = 0;

    GameObject lowPolyRender;
    WorldManager worldManager;
    ItemManager itemManager;

    GameObject player;
    public bool loadingGame = false;

    AsteroidFieldGenerator asteroidFieldGenerator;

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    int seed;
    public int getSeed() { return seed; }

    public void AddToQueue(Vector3 pos)
    {
        AsteroidPositionsSpawnQueue.Add(pos);
    }

    public void addRemovedAsteroid(Vector3 pos)
    {
#if UNITY_EDITOR
            Debug.Log("Removing asteroid at " + pos);
#endif
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

    private void destroyAsteroid(Vector3 pos)
    {
        // Destroy(allSpawnedAsteroids[pos]);
        allSpawnedAsteroids[pos].GetComponent<AsteroidGenerator>().unloadAsteroid(); // should free the memory taken by mesh creation
        allSpawnedAsteroids[pos].SetActive(false);
        AsteroidGameObjectQueue.AddFirst(allSpawnedAsteroids[pos]);
        numAsteroidsInQueue++;
        //Debug.Log("AsteroidGameObjectQueue count: " + (5000-numAsteroidsInQueue));
#if UNITY_EDITOR
            // Debug.Log("AsteroidGameObjectQueue count: " + numAsteroidsInQueue);
#endif
        allSpawnedAsteroids.Remove(pos);

        totalDespawnedAsteroidsSinceLastUnload++;
        if (totalDespawnedAsteroidsSinceLastUnload > 10000)
        {
            // Debug.Log("Unloading unused assets");
            totalDespawnedAsteroidsSinceLastUnload = 0;
            Resources.UnloadUnusedAssets();
        }
    }

    public void setRemovedChunksForAsteroid(Vector3 pos, HashSet<int> removedChunksIndecies)
    {
        if (allEditedAsteroids.ContainsKey(pos))
        {
            allEditedAsteroids[pos] = removedChunksIndecies;
        }
        else
        {
            allEditedAsteroids.Add(pos, removedChunksIndecies);
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
            destroyAsteroid(pos);
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
        GameObject worldManagerObject = GameObject.Find("WorldManager");
        worldManager = worldManagerObject.GetComponent<WorldManager>();
        itemManager = worldManagerObject.GetComponent<ItemManager>();
        minerals = worldManagerObject.GetComponent<Minerals>();
        asteroidPrefab = Resources.Load<GameObject>("Prefabs/Asteroids/Asteroid") as GameObject;
        asteroidFieldGenerator = gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>();
        player = GameObject.Find("Player");
    }

    // should only be called once when the world is set up
    public void StartAfterWorldManagerSetUp(Vector3 playerSpawnPos)
    {
        seed = worldManager.getSeed();
        Random.InitState(getSeed());
        MakeLowPolyRender();
        MakePregeneratedAsteroids();
        MakePregeneratedGameObjectsForAsteroids();
        IEnumerator coroutine = SpawnAsteroidFromQueue();
        StartCoroutine(coroutine);
        Debug.Log("Seed: " + seed);
    }

    public void LoadGame(HashSet<Vector3> newRemovedAsteroids, Dictionary<Vector3, HashSet<int>> newEditedAsteroids)
    {
        Debug.Log("Loading game");
        loadingGame = true;
        AsteroidPositionsSpawnQueue = new List<Vector3>();

        // delete all asteroidAreas
        asteroidFieldGenerator.destroyAllSpawnAreas();

        allRemovedAsteroids = newRemovedAsteroids;
        allEditedAsteroids = newEditedAsteroids;

        loadingGame = false;
        // spawn the first area spawner near the players current pos
        asteroidFieldGenerator.SpawnNewArea(player.transform.localPosition);
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
        for (int i = 0; i < totalToPregen / 4; i++)
        {
            AllPregeneratedBigAsteroids.Add(GenerateOneAsteroid(new Vector3(0, 0, 0), true));
        }
        watch.Stop();
        Debug.Log("Pregenerated " + totalToPregen / 4 + " big asteroids in " + watch.ElapsedMilliseconds / 1000f + "s");

    }

    GameObject MakeLowPolyRender()
    {
        // this will only happen once
        Mesh lowPolyMesh = new Mesh();
        List<Vector3> lowPolyPoints = new List<Vector3>();
        List<Vector3> lowPolyVerts = new List<Vector3>();
        List<int> lowPolyTris = new List<int>();
        List<Vector3> lowPolyNormals = new List<Vector3>();
        float size = 4f;
        // make a big square out of the from the size 
        lowPolyPoints.Add(new Vector3(-size, -size, -size));
        lowPolyPoints.Add(new Vector3(size, -size, -size));
        lowPolyPoints.Add(new Vector3(size, size, -size));
        lowPolyPoints.Add(new Vector3(-size, size, -size));
        lowPolyPoints.Add(new Vector3(-size, -size, size));
        lowPolyPoints.Add(new Vector3(size, -size, size));
        lowPolyPoints.Add(new Vector3(size, size, size));
        lowPolyPoints.Add(new Vector3(-size, size, size));

        ConvexHullCalculator ConvexHullCalcGlobal = new ConvexHullCalculator();
        ConvexHullCalcGlobal.GenerateHull(lowPolyPoints, true, ref lowPolyVerts, ref lowPolyTris, ref lowPolyNormals);
        lowPolyMesh.vertices = lowPolyVerts.ToArray();
        lowPolyMesh.triangles = lowPolyTris.ToArray();
        lowPolyMesh.normals = lowPolyNormals.ToArray();

        GameObject lowPolyRender = new GameObject("lowPolyRender");
        lowPolyRender.layer = 8;
        lowPolyRender.transform.parent = transform;
        lowPolyRender.transform.localPosition = Vector3.zero;
        lowPolyRender.AddComponent<MeshRenderer>();
        lowPolyRender.AddComponent<MeshFilter>();
        lowPolyRender.GetComponent<MeshFilter>().mesh = lowPolyMesh;
        lowPolyRender.GetComponent<Renderer>().materials = new Material[] { itemManager.getMaterial("Stone") };
        lowPolyRender.SetActive(false);
        return lowPolyRender;
    }

    // this will create all the asteroid game objects to use when copying an asteroid
    void MakePregeneratedGameObjectsForAsteroids()
    {
        int numToPregen = 10000;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        GameObject fakeAsteroid = GenerateOneAsteroid(new Vector3(0, 0, 0), false, false);
        fakeAsteroid.SetActive(false);
        for (int i = 0; i < numToPregen; i++)
        {
            GameObject newAsteroid = Instantiate(fakeAsteroid, new Vector3(0, 5, 0), Quaternion.identity) as GameObject;
            newAsteroid.SetActive(false);
            newAsteroid.transform.SetParent(AllPregeneratedAsteroids[0].transform.parent, false);
            AsteroidGameObjectQueue.AddFirst(newAsteroid);
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
                if (newAsteroid != null)
                {
                    allSpawnedAsteroids.Add(AsteroidPositionsSpawnQueue[0], newAsteroid);
                    if (allEditedAsteroids.ContainsKey(AsteroidPositionsSpawnQueue[0]))
                    {
                        newAsteroid.GetComponent<AsteroidGenerator>().setRemovedCubeIndecies(allEditedAsteroids[AsteroidPositionsSpawnQueue[0]]);
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
                    {
                        // Debug.Log("Queue emptied");
                        printed = false;
                    }
                    lastEmptyCheck = true;
                }
                yield return 0;
            }
            if (AsteroidPositionsSpawnQueue.Count % 10 == 0 && AsteroidPositionsSpawnQueue.Count != 0)
            {
                if (printQueueStatus)
                {
                    Debug.Log("Num asteroids left in queue " + AsteroidPositionsSpawnQueue.Count);
                    printed = true;
                }
            }

            if (!initialLoadFinished && AsteroidPositionsSpawnQueue.Count == 1)
            { // cant be 0 because it starts at 0
                Debug.Log("Initial load finished");
                initialLoadFinished = true;
                stopwatch.Stop();
                Debug.Log("Initial load took: " + stopwatch.ElapsedMilliseconds / 1000f + "s");
            }

            if (initialLoadFinished && AsteroidPositionsSpawnQueue.Count % 1000 == 0)
                // wait 7 frames
                yield return 0;
            else if (AsteroidPositionsSpawnQueue.Count % 1000 == 0)
                yield return 0;

        }
    }

    GameObject GenerateOneAsteroid(Vector3 position, bool isBig, bool generate = true)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity) as GameObject;
        // add a copy of the lowPolyRender to the asteroid
        if (lowPolyRender == null)
        {
            lowPolyRender = MakeLowPolyRender();
        }
        GameObject lowPolyRenderCopy = Instantiate(lowPolyRender, position, Quaternion.identity) as GameObject;
        lowPolyRenderCopy.transform.SetParent(newAsteroid.transform, false);
        lowPolyRenderCopy.SetActive(true);
        
        newAsteroid.tag = "Asteroid";
        // set the parent to this objects parent
        newAsteroid.transform.SetParent(gameObject.transform.parent, false);
        // random mineral based on the world position

        Item mineralType = minerals.GetMineralTypeFromPos(position, isBig);
        Item stone = minerals.GetMineralByName("Stone");

        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = mineralType;
        newAsteroid.GetComponent<AsteroidGenerator>().stone = stone;
        newAsteroid.GetComponent<AsteroidGenerator>().isBig = isBig;

        newAsteroid.GetComponent<Renderer>().materials = new Material[] { itemManager.getMaterial(mineralType.getName()), itemManager.getMaterial("Stone") };

        newAsteroid.SetActive(false);
        if (generate)
        {
            newAsteroid.GetComponent<AsteroidGenerator>().Generate();
        }
        return newAsteroid;
    }

    GameObject CopyOneAsteroid(Vector3 position)
    {
        Random.InitState((int)(Mathf.Abs((getSeed() + 1) / 1000 + (position.x * 10 + position.y * 100 + position.z * 1000))));
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
        if (AsteroidGameObjectQueue.First == null)
        {
            Debug.LogError("AsteroidGameObjectQueue is empty! Add more on load. There should be " + numAsteroidsInQueue);
            return null;
        }
        GameObject newAsteroid = AsteroidGameObjectQueue.First.Value;
        AsteroidGameObjectQueue.RemoveFirst();
        numAsteroidsInQueue--;
        newAsteroid.transform.localPosition = position;

        newAsteroid.transform.SetParent(asteroidToCopy.transform.parent, false);
        AsteroidGenerator o = asteroidToCopy.GetComponent<AsteroidGenerator>();
        if (!newAsteroid.GetComponent<AsteroidGenerator>().copyAll(ref o, gameObject.GetComponent<AsteroidSpawnManager>()))
        {
            Debug.LogError("Failed to copy asteroid number" + asteroidNumber + " at " + position);
            Destroy(newAsteroid);
            return null;
        }
        newAsteroid.GetComponent<AsteroidGenerator>().setOriginalPosition(position);

        // make it a different mineral
        Item mineralType = minerals.GetMineralTypeFromPos(newAsteroid.transform.localPosition, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = mineralType;
        newAsteroid.GetComponent<Renderer>().materials = new Material[] { itemManager.getMaterial(mineralType.getName()), itemManager.getMaterial("Stone") };

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