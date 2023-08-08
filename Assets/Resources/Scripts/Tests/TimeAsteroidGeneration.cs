using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAsteroidGeneration : MonoBehaviour
{
    [SerializeField]
    public GameObject asteroidPrefab;

    [SerializeField]
    int maxAsteroidsX = 50;

    [SerializeField]
    int maxAsteroidsZ = 50;

    [SerializeField]
    int spacing = 20;

    Minerals minerals;

    [SerializeField]
    bool redoTest = false;

    List<GameObject> allAsteroids = new List<GameObject>();

    void doTest()
    {
        
        // delete all asteroids
        foreach (GameObject asteroid in allAsteroids)
        {
            Destroy(asteroid);
        }
        allAsteroids.Clear();

        minerals = new Minerals();
        minerals.SetUp();

        // time how long it takes to generate the asteroids
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        // make a grid of maxAsteroids
        for (int x = 0; x < maxAsteroidsX * spacing; x += spacing)
        {
            for (int z = 0; z < maxAsteroidsZ * spacing; z += spacing)
            {
                // random chance to spawn a big asteroid
                if (Random.Range(0, 100) < 10)
                {
                    GenerateOneAsteroid(new Vector3(x, 0, z), true);
                }
                else
                {
                    GenerateOneAsteroid(new Vector3(x, 0, z), false);
                }
            }
        }
        stopwatch.Stop();
        Debug.Log("Time to generate " + maxAsteroidsX * maxAsteroidsZ + " asteroids: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    void Update()
    {
        if (redoTest)
        {
            redoTest = false;
            doTest();
        }
    }

    private void GenerateOneAsteroid(Vector3 position, bool isBig)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity) as GameObject;
        newAsteroid.tag = "Asteroid";
        // set the parent to this objects parent
        newAsteroid.transform.SetParent(transform, false);

        // random mineral based on the world position
        // worldPosition = asteroid pos + asteroid area spawner position + asteroid field position
        Vector3 worldPosition = position + transform.position + transform.parent.position;

        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = minerals.GetMineralTypeFromPos(worldPosition, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().isBig = isBig;

        // add the asteroid to the parents asteroid field
        allAsteroids.Add(newAsteroid);
        newAsteroid.GetComponent<AsteroidGenerator>().Generate();
    }
}
