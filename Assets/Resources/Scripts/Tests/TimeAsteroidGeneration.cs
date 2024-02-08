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
    int maxAsteroidsY = 1;

    [SerializeField]
    int maxAsteroidsZ = 50;

    [SerializeField]
    int spacing = 20;

    Minerals minerals;

    [SerializeField]
    bool redoTest = false;

    List<GameObject> allAsteroids = new List<GameObject>();

    int numAsteroids = 0;

    AsteroidSpawnManager asteroidSpawnManager;

    void Start()
    {
        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();
        asteroidPrefab = Resources.Load<GameObject>("Prefabs/Asteroids/Asteroid") as GameObject;
    }

    void doTest()
    {
        asteroidSpawnManager.initialLoadFinished = false;
        // delete all asteroids
        foreach (GameObject asteroid in allAsteroids)
        {
            Destroy(asteroid);
        }
        allAsteroids.Clear();

        minerals = GameObject.Find("WorldManager").GetComponent<Minerals>();

        // make a grid of maxAsteroids
        for (int x = 0; x < maxAsteroidsX * spacing; x += spacing)
        {
            for (int y = 10; y < (maxAsteroidsY * spacing) + 10; y += spacing)
            {
                for (int z = 0; z < maxAsteroidsZ * spacing; z += spacing)
                {
                    asteroidSpawnManager.AddToQueue(new Vector3(x, y, z));
                    numAsteroids++;
                }
            }
        }
    }

    void FixedUpdate()
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
        Vector3 worldPosition = position + transform.localPosition;

        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = minerals.GetMineralTypeFromPos(worldPosition, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().isBig = isBig;

        // add the asteroid to the parents asteroid field
        allAsteroids.Add(newAsteroid);

        if (numAsteroids > (maxAsteroidsX * maxAsteroidsZ) / 2)
        {
            newAsteroid.GetComponent<AsteroidGenerator>().increment = 1.1f;
        }
        else
        {
            newAsteroid.GetComponent<AsteroidGenerator>().increment = 1.1f;
        }

        newAsteroid.GetComponent<AsteroidGenerator>().Generate();
    }

}
