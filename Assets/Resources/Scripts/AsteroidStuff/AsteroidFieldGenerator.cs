using System.Collections.Generic;
using UnityEngine;

public class AsteroidFieldGenerator : MonoBehaviour
{
    GameObject asteroidAreaPrefab;
    GameObject asteroidPrefab;
    public List<GameObject> AsteroidField;

    // defaults are 1000 asteroids in a 1000m radius
    [SerializeField]
    [Range(1, 15)]
    public float density = 3f;

    int sizeOfPartitions = 500;

    [SerializeField]
    [Range(1000, 100000)]
    public int radius = 1000;

    [SerializeField]
    [Range(1000, 100000)]
    public int height = 10000;

    [SerializeField]
    bool SpawnAsteroidField = true;

    // dictionary of all area positions
    public Dictionary<Vector3, bool> areaPositions = new Dictionary<Vector3, bool>();

    public Minerals minerals;

    AsteroidSpawnManager asteroidSpawnManager;

    // Start is called before the first frame update
    void Start()
    {
        minerals = new Minerals();
        minerals.SetUp();

        asteroidSpawnManager = GameObject.Find("AsteroidSpawnManager").GetComponent<AsteroidSpawnManager>();

        if (SpawnAsteroidField)
        {
            // load the prefabs
            asteroidAreaPrefab = Resources.Load<GameObject>("Prefabs/Asteroids/AsteroidArea") as GameObject;

            // generate an asteroid field around this object
            SpawnNewArea(transform.position);
            SpawnNewArea(new Vector3(0, 0, 3000000));
        }
    }


    void Refresh()
    {
        // time it
        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();
        int size = AsteroidField.Count;
        for (int x = 0; x < size; x++)
        {
            Destroy(AsteroidField[0]);
            AsteroidField.Remove(AsteroidField[0]);
        }
        Start();
        watch.Stop();
        UnityEngine.Debug.Log("Refreshed asteroid field in " + watch.ElapsedMilliseconds + "ms");
    }


    void SpawnNewArea(Vector3 pos)
    {
        int negativeRad = -1 * radius;
        int negativeHeight = -1*height;
        // make AsteroidAreaPrefab
        GameObject newAsteroidArea = Instantiate(asteroidAreaPrefab, pos, Quaternion.identity) as GameObject;
        // set the parent to this object
        newAsteroidArea.transform.parent = gameObject.transform;
        // set newAsteroidAreas density, radius, and height
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().density = density * (sizeOfPartitions / 1000f);
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().radius = (radius / ((radius - negativeRad) / sizeOfPartitions));
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().height = (height / ((height - negativeHeight) / sizeOfPartitions));
        newAsteroidArea.GetComponent<AsteroidAreaSpawner>().GenerateAsteroids(asteroidSpawnManager);
        // add to the dictionary
        areaPositions.Add(pos, true);
    }

    public void SpawnMoreAreasAt(Vector3 middlePos)
    {
        int s = sizeOfPartitions;
        // if the 6 spots around middlePos are not in the dictionary, spawn a new area there and add it to the dictionary
        // if they are in the dictionary, do nothing

        if (!areaPositions.ContainsKey(middlePos + new Vector3(s, 0, 0)))
            SpawnNewArea(middlePos + new Vector3(s, 0, 0));
        if (!areaPositions.ContainsKey(middlePos + new Vector3(-1*s, 0, 0)))
            SpawnNewArea(middlePos + new Vector3(-1 * s, 0, 0));
        if (!areaPositions.ContainsKey(middlePos + new Vector3(0, s, 0)))
            SpawnNewArea(middlePos + new Vector3(0, s, 0));
        if (!areaPositions.ContainsKey(middlePos + new Vector3(0, -1 * s, 0)))
            SpawnNewArea(middlePos + new Vector3(0, -1 * s, 0));
        if (!areaPositions.ContainsKey(middlePos + new Vector3(0, 0, s)))
            SpawnNewArea(middlePos + new Vector3(0, 0, s));
        if (!areaPositions.ContainsKey(middlePos + new Vector3(0, 0, -1 * s)))
            SpawnNewArea(middlePos + new Vector3(0, 0, -1 * s));

    }



}

