using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidAreaSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public int radius;
    public int height;

    Minerals minerals;

    Vector3 spawnedAsteroidPosition;

    AsteroidSpawnManager asteroidSpawnManager;
    AsteroidFieldGenerator asteroidFieldGenerator;

    public Vector3 addedAtPos;

    GameObject player;
    float xMax;
    float yMax;
    float zMax;

    WorldManager worldManager;

    void Start()
    {
        asteroidFieldGenerator = gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>();
        asteroidSpawnManager = asteroidFieldGenerator.asteroidSpawnManager;
        player = GameObject.Find("Player");
        GameObject AsteroidSpawnAreaDeSpawner = player.transform.Find("AsteroidSpawnAreaDeSpawner").gameObject;
        xMax = AsteroidSpawnAreaDeSpawner.transform.localScale.x / 2f;
        yMax = AsteroidSpawnAreaDeSpawner.transform.localScale.y / 2f;
        zMax = AsteroidSpawnAreaDeSpawner.transform.localScale.z / 2f;
        if (worldManager == null)
        {
            worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        }
        // run ensureInRange every 5 seconds
        InvokeRepeating("ensureInCollider", 5, 5);
    }

    public void GenerateAsteroids(AsteroidSpawnManager asteroidSpawnManager, bool allowRandomness, WorldManager worldManager)
    {
        this.asteroidSpawnManager = asteroidSpawnManager;
        if (this.worldManager == null)
        {
            this.worldManager = worldManager;
        }
        Random.InitState((int)(Mathf.Abs((asteroidSpawnManager.getSeed() + 1) / 1000 + ((int)worldManager.getObjectTruePosition(transform.position).x * 10 + (int)worldManager.getObjectTruePosition(transform.position).y * 100 + (int)worldManager.getObjectTruePosition(transform.position).z * 1000))));
        spawnedAsteroidPosition = new Vector3((int)Random.Range(-1 * radius, radius), (int)Random.Range(-1 * height, height), (int)Random.Range(-1 * radius, radius)) + worldManager.getObjectTruePosition(transform.position);
        if (allowRandomness)
        {
            asteroidSpawnManager.AddToQueue(spawnedAsteroidPosition);
        }
        else
        {
            asteroidSpawnManager.AddToQueue(worldManager.getObjectTruePosition(transform.position));
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaSpawner")
        {
            gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().SpawnMoreAreasAt(worldManager.getObjectTruePosition(transform.position));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaDespawner")
        {
            destroyAsteroidAndThis();
        }
    }

    void ensureInCollider()
    {

        // make sure the x y and z are within the size of AsteroidAreaDespawner from the player
        if (Mathf.Abs(player.transform.position.x - transform.position.x) > xMax ||
            Mathf.Abs(player.transform.position.y - transform.position.y) > yMax ||
            Mathf.Abs(player.transform.position.z - transform.position.z) > zMax)
        {
            destroyAsteroidAndThis();
        }
    }

    public void destroyAsteroidAndThis()
    {
        if (asteroidSpawnManager == null)
        {
            Debug.Log("Should not be here"); // can be here if its very very rare
            asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();
        }
        if (asteroidFieldGenerator == null)
        {
            Debug.LogError("Should not be here"); // can be here if its very very rare
            asteroidFieldGenerator = gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>();
        }
        // destroy the asteroid that was spawned by this, then destroy this
        asteroidSpawnManager.destroyAsteroidAt(spawnedAsteroidPosition);
        // delete the spawner from its parents list using remove Spawn Area At on the AsteroidFieldGenerator
        asteroidFieldGenerator.removeSpawnAreaAt(addedAtPos);
    }

}
