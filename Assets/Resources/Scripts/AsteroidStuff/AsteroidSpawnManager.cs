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

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    public void AddToQueue(Vector3 pos)
    {
        AsteroidPositionsSpawnQueue.Add(pos);
    }

    void Start()
    {
        minerals.SetUp();
        IEnumerator coroutine = SpawnAsteroidFromQueue();
        StartCoroutine(coroutine);
        MakePregeneratedAsteroids();
    }

    void MakePregeneratedAsteroids()
    {
        // make 100 normal asteroids
        for (int i = 0; i < 100; i++)
        {
            AllPregeneratedAsteroids.Add(GenerateOneAsteroid(new Vector3(0, 0, 0), false));
        }
        // make 100 big asteroids
        for (int i = 0; i < 100; i++)
        {
            AllPregeneratedBigAsteroids.Add(GenerateOneAsteroid(new Vector3(0, 0, 0), true));
        }
    }

    private IEnumerator SpawnAsteroidFromQueue()
    {
        stopwatch.Start();
        while (true)
        {
            // if there is something in the queue 
            if (AsteroidPositionsSpawnQueue.Count != 0)
            {
                if (AsteroidPositionsSpawnQueue[0] != null)
                    CopyOneAsteroid(AsteroidPositionsSpawnQueue[0]);
                AsteroidPositionsSpawnQueue.RemoveAt(0);
                lastEmptyCheck = false;
            }
            else
            {
                if (!lastEmptyCheck)
                {
                    Debug.Log("Queue emptied");
                    lastEmptyCheck = true;
                }
                yield return WaitForFrames(60);
            }
            if (AsteroidPositionsSpawnQueue.Count % 10 == 0 && AsteroidPositionsSpawnQueue.Count != 0)
                Debug.Log("Num asteroids left in queue " + AsteroidPositionsSpawnQueue.Count);

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

    GameObject GenerateOneAsteroid(Vector3 position, bool isBig)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity) as GameObject;
        // add the asteroid to the parents asteroid field
        gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().AsteroidField.Add(newAsteroid);
        
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

        newAsteroid.GetComponent<AsteroidGenerator>().Generate();
        newAsteroid.SetActive(false);
        return newAsteroid;
    }

    void CopyOneAsteroid(Vector3 position)
    {
        bool isBig = Random.Range(0, 10) == 0;
        GameObject asteroidToCopy;
        if (isBig)
            asteroidToCopy = AllPregeneratedBigAsteroids[Random.Range(0, AllPregeneratedBigAsteroids.Count)];
        else
            asteroidToCopy = AllPregeneratedBigAsteroids[Random.Range(0, AllPregeneratedBigAsteroids.Count)];
        
        // make a copy of the asteroid
        GameObject newAsteroid = Instantiate(asteroidToCopy, position, Quaternion.identity) as GameObject;
        newAsteroid.transform.SetParent(asteroidToCopy.transform.parent, false);
        AsteroidGenerator o = asteroidToCopy.GetComponent<AsteroidGenerator>();
        newAsteroid.GetComponent<AsteroidGenerator>().copyAll(o.mineralType, o.isBig, o.points, o.outsidePoints, o.pointColors,
                        o.pointToCubes, o.mesh, o.increment, o.pointsSetPositions, o.cubesPointIndecies, o.allVerts, o.allTris, o.allNormals);

        // make it a different mineral
        Item mineralType = minerals.GetMineralTypeFromPos(position, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = mineralType;
        newAsteroid.GetComponent<Renderer>().materials = new Material[] { mineralType.getMaterial(), minerals.GetMineralByName("Stone").getMaterial() };

        newAsteroid.SetActive(true);
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
