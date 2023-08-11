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
    bool initialLoadFinished = false;

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
                    GenerateOneAsteroid(AsteroidPositionsSpawnQueue[0], Random.Range(0, 10) == 0 ? true : false);
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

            if (AsteroidPositionsSpawnQueue.Count == 1)
            { // cant be 0 because it starts at 0
                Debug.Log("Initial load finished");
                initialLoadFinished = true;
                stopwatch.Stop();
                Debug.Log("Initial load took: " + stopwatch.ElapsedMilliseconds/1000f + "s");
            }

            if (initialLoadFinished)
                yield return WaitForFrames(7);
                // wait 7 frames
        }
    }

    void GenerateOneAsteroid(Vector3 position, bool isBig)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity) as GameObject;
        // add the asteroid to the parents asteroid field
        gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().AsteroidField.Add(newAsteroid);
        
        newAsteroid.tag = "Asteroid";
        // set the parent to this objects parent
        newAsteroid.transform.SetParent(gameObject.transform.parent, false);
        // random mineral based on the world position
        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = minerals.GetMineralTypeFromPos(position, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().isBig = isBig;

        newAsteroid.GetComponent<AsteroidGenerator>().increment = 1.1f;

        newAsteroid.GetComponent<AsteroidGenerator>().Generate();
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
