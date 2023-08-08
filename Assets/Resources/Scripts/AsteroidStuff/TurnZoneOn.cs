
using UnityEngine;

public class TurnZoneOn : MonoBehaviour
{

    Minerals minerals;

    void Start()
    {
        minerals = new Minerals();
        minerals.SetUp();
    }

    // if this enters a trigger that is tagged as "AsteroidSpawnZone", spawn asteroids in that zone
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnZone")
        {
            // call the GenerateAsteroids funtion of other
            other.gameObject.GetComponent<AsteroidAreaSpawner>().GenerateAsteroids(minerals);
        }
    }
}
