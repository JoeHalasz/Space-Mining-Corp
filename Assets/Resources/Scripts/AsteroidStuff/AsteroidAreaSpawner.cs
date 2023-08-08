using UnityEngine;

public class AsteroidAreaSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    public float density;
    public int radius;
    public int height;

    bool done = false;

    Minerals minerals;

    public void GenerateAsteroids(Minerals _minerals)
    {
        minerals = _minerals;
        if (!done)
        {
            done = true;
            for (int i = 0; i < density; i++)
            {
                GenerateOneAsteroid(new Vector3(Random.Range(-1 * radius, radius), Random.Range(-1 * height, height), Random.Range(-1 * radius, radius)) + transform.position, Random.Range(0, 10) == 0 ? true : false);
            }
        }
    }


    private void GenerateOneAsteroid(Vector3 position, bool isBig)
    {
        GameObject newAsteroid = Instantiate(asteroidPrefab, position, Quaternion.identity) as GameObject;
        newAsteroid.tag = "Asteroid";
        // set the parent to this objects parent
        newAsteroid.transform.parent = gameObject.transform.parent;

        // random mineral based on the world position
        // worldPosition = asteroid pos + asteroid area spawner position + asteroid field position
        Vector3 worldPosition = position + transform.position + transform.parent.position;

        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = minerals.GetMineralTypeFromPos(worldPosition, isBig);
        newAsteroid.GetComponent<AsteroidGenerator>().isBig = isBig;
        
        // add the asteroid to the parents asteroid field
        gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().AsteroidField.Add(newAsteroid);
        newAsteroid.GetComponent<AsteroidGenerator>().Generate();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AsteroidSpawnAreaSpawner")
        {
            gameObject.transform.parent.GetComponent<AsteroidFieldGenerator>().SpawnMoreAreasAt(transform.position);
        }
    }

}
