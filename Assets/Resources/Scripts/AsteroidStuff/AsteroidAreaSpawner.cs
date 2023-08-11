using UnityEngine;

public class AsteroidAreaSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public float density;
    public int radius;
    public int height;

    bool done = false;

    Minerals minerals;

    public void GenerateAsteroids(AsteroidSpawnManager asteroidSpawnManager)
    {
        if (!done)
        {
            done = true;
            for (int i = 0; i < density; i++)
            {
                asteroidSpawnManager.AddToQueue(new Vector3(Random.Range(-1 * radius, radius), Random.Range(-1 * height, height), Random.Range(-1 * radius, radius)) + transform.position);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaSpawner")
        {
            gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().SpawnMoreAreasAt(transform.position);
        }
    }

}
