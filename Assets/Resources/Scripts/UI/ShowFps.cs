using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFps : MonoBehaviour
{

    GameObject fpsText;
    private Queue<float> frameTimes = new Queue<float>();
    float x = 1;
    private float timeAgo;
    bool needsUpdate = true;
    
    void Start()
    {
        fpsText = GameObject.Find("FPS counter");
        // updateFPS 10 times per second
        InvokeRepeating("updateFps", 0, .1f);
    }

    void Update()
    {
        timeAgo = Time.time - x;

        // Add the current frame time to the queue
        frameTimes.Enqueue(Time.time);

        // Remove frame times more than one second old
        while (frameTimes.Count > 0 && frameTimes.Peek() < timeAgo)
        {
            frameTimes.Dequeue();
        }
        
        if (needsUpdate)
        {
            needsUpdate = false;
            // The average FPS over the last second is the number of frame times in the queue
            float averageFPS = frameTimes.Count / x;

            fpsText.GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.Round(averageFPS).ToString();
        }
    }

    void updateFps()
    {
        needsUpdate = true;
    }
}
