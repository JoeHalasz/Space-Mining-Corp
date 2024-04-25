using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonGenerator : MonoBehaviour
{
    GameObject asteroidPrefab;
    ItemManager itemManager;
    Minerals minerals;

    void Start()
    {
        asteroidPrefab = Resources.Load<GameObject>("Prefabs/Asteroids/Asteroid") as GameObject;
        GameObject worldManagerObject = GameObject.Find("WorldManager");
        itemManager = worldManagerObject.GetComponent<ItemManager>();
        minerals = worldManagerObject.GetComponent<Minerals>();
        GenerateMoon();
    }

    GameObject GenerateMoon()
    {
        Debug.Log("here");
        GameObject newAsteroid = Instantiate(asteroidPrefab, new Vector3(0, 100, 0), Quaternion.identity) as GameObject;
        newAsteroid.name = "Moon";
        newAsteroid.tag = "Asteroid";
        // set the parent to this objects parent
        newAsteroid.transform.SetParent(gameObject.transform.parent, false);


        Item stone = minerals.GetMineralByName("Stone");
        Item mineralType = stone;

        newAsteroid.GetComponent<AsteroidGenerator>().mineralType = mineralType;

        newAsteroid.GetComponent<Renderer>().materials = new Material[] { itemManager.getMaterial(mineralType.getName()), itemManager.getMaterial("Stone") };

        newAsteroid.SetActive(true);

        newAsteroid.GetComponent<AsteroidGenerator>().isMoon = true;
        newAsteroid.GetComponent<AsteroidGenerator>().Generate();

        return newAsteroid;
    }

}
