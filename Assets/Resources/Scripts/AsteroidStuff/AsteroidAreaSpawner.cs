using UnityEngine;

public class AsteroidAreaSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public int radius;
    public int height;

    bool done = false;

    Minerals minerals;

    int total = 0;

    Vector3 spawnedAsteroidPosition;

    AsteroidSpawnManager asteroidSpawnManager;
    AsteroidFieldGenerator asteroidFieldGenerator;

    void Start()
    {
        asteroidFieldGenerator = gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>();
    }

    public void GenerateAsteroids(AsteroidSpawnManager asteroidSpawnManager)
    {   
        this.asteroidSpawnManager = asteroidSpawnManager;
        Random.InitState((int)(Mathf.Abs((asteroidSpawnManager.getSeed()+1)/1000 + ((int)transform.position.x*10 + (int)transform.position.y*20 + (int)transform.position.z*30))));
        spawnedAsteroidPosition = new Vector3((int)Random.Range(-1 * radius, radius), (int)Random.Range(-1 * height, height), (int)Random.Range(-1 * radius, radius)) + transform.position;
        // spawnedAsteroidPosition = transform.position;
        asteroidSpawnManager.AddToQueue(spawnedAsteroidPosition);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaSpawner")
        {
            gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().SpawnMoreAreasAt(transform.position);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaDespawner" && asteroidSpawnManager != null)
        {
            // destroy the asteroid that was spawned by this, then destroy this
            asteroidSpawnManager.destroyAsteroidAt(spawnedAsteroidPosition);
            // delete the spawner from its parents list using removeSpawnAreaAt on the AsteroidFieldGenerator
            asteroidFieldGenerator.removeSpawnAreaAt(transform.position);
            Destroy(gameObject);
        }
    }

}
