using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRenderDistances : MonoBehaviour
{
    float stationRenderDist = 100000f;
    float asteroidRenderDist = 5000f;
    void Awake()
    {
        Camera camera = GetComponent<Camera>();
        float[] distances = new float[32];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = 1000f;
        }
        Debug.Log("Setting render distance for station to " + stationRenderDist);
        distances[7] = stationRenderDist; // station
        Debug.Log("Setting render distance for asteroids to " + asteroidRenderDist);
        distances[8] = asteroidRenderDist; // asteroids
        camera.layerCullDistances = distances;
    }
}
