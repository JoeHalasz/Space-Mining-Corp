using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmmisionUpAndDown : MonoBehaviour
{

    // store original color
    Color originalColor;

    [SerializeField]
    public float slowness = 3f;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = GetComponent<Renderer>().material.GetColor("_EmissionColor");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // go from original color to original color + 1 over time
        float emission = Mathf.PingPong(Time.time, 4 * slowness);

        Color finalColor = originalColor * ((emission/slowness) + 1);
        // keep alhpa the same
        finalColor.a = originalColor.a;
        GetComponent<Renderer>().material.SetColor("_EmissionColor", finalColor);
    }
}
