using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewWorld : MonoBehaviour
{
    void CreateWorld(int seed = -1)
    {
        // generate a seed if none is provided
        if (seed == -1)
        {
            seed = Random.Range(0, 1000000000);
        }
        // use the seed to do these things in order (if more things are added, add them to the end so world gen isn't destroyed)
            // generate 500 different asteroids for the asteroid field
            // create an equation for where the asteroids should be generated
        

        // save the seed to the manager so it can be used to generate things in the world, and so the world can be loaded
        GameObject.Find("WorldManager").GetComponent<WorldManager>().seed = seed;

    }
}
