using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeStationAlwaysVisible : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        float[] distances = new float[32];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = 1000f;
        }
        distances[7] = 100000;
        camera.layerCullDistances = distances;
    }
}
