
using UnityEngine;

public class TurnZoneOn : MonoBehaviour
{

    AsteroidSpawnManager asteroidSpawnManager;

    void Start()
    {
        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();
    }

    // if this enters a trigger that is tagged as "AsteroidSpawnZone", spawn asteroids in that zone
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnZone")
        {
            // call the GenerateAsteroids funtion of other
            other.gameObject.GetComponent<AsteroidAreaSpawner>().GenerateAsteroids(asteroidSpawnManager);
        }
    }
}
