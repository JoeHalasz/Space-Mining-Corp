using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidAreaSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public int radius;
    public int height;

    bool done = false;

    Minerals minerals;

    int total = 0;

    bool insideDespawner = true;

    Vector3 spawnedAsteroidPosition;

    AsteroidSpawnManager asteroidSpawnManager;
    AsteroidFieldGenerator asteroidFieldGenerator;

    public Vector3 addedAtPos;

    void Start()
    {
        asteroidFieldGenerator = gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>();
    }

    public void GenerateAsteroids(AsteroidSpawnManager asteroidSpawnManager)
    {   
        this.asteroidSpawnManager = asteroidSpawnManager;
        Random.InitState((int)(Mathf.Abs((asteroidSpawnManager.getSeed()+1)/1000 + (transform.localPosition.x*10 + transform.localPosition.y*100 + transform.localPosition.z*1000))));
        spawnedAsteroidPosition = new Vector3((int)Random.Range(-1 * radius, radius), (int)Random.Range(-1 * height, height), (int)Random.Range(-1 * radius, radius)) + transform.localPosition;
        // spawnedAsteroidPosition = transform.localPosition;
        asteroidSpawnManager.AddToQueue(spawnedAsteroidPosition);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaSpawner")
        {
            gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().SpawnMoreAreasAt(transform.localPosition);
        }
        else if (other.gameObject.tag == "AsteroidSpawnAreaDespawner")
        {
            insideDespawner = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaDespawner")
        {
            insideDespawner = true;
            // wait half a second and if insideDespawner is still true then destroy the asteroid and this
            StartCoroutine(waitAndDestroy());
        }
    }

    // wait and destory
    IEnumerator waitAndDestroy()
    {
        // wait for half a second
        yield return new WaitForSeconds(0.5f);
        if (insideDespawner)
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
            Debug.Log("Should not be here"); // can be here if its very very rare
            asteroidFieldGenerator = gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>();
        }
        // destroy the asteroid that was spawned by this, then destroy this
        asteroidSpawnManager.destroyAsteroidAt(spawnedAsteroidPosition);
        // delete the spawner from its parents list using remove Spawn Area At on the AsteroidFieldGenerator
        asteroidFieldGenerator.removeSpawnAreaAt(addedAtPos);
    }

}
