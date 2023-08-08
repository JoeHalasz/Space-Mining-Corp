using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpGateEvents : MonoBehaviour
{

    [SerializeField]
    GameObject WarpGateCasing;

    [SerializeField]
    GameObject CenterDisk;

    [SerializeField]
    GameObject Portals;

    int step = 0;
    Quaternion WarpGateStartRotation;
    // when the event triggers it should move slowly rotate the WarpGateCasing to the correct position, and then create the warp gate

    bool doEvent = false;
    bool SlowPortalSpin = false;
    public void StartEvent(int timesDone)
    {
        if (timesDone == 1) {
            doEvent = true;
            // get current rotation of the WarpGateCasing
            WarpGateStartRotation = WarpGateCasing.transform.rotation;
            Debug.Log("Starting warp gate event");
        }
    }

    public void StopEvent(int timesDone)
    {
        if (timesDone == 1)
        {
            doEvent = false;
            Debug.Log("Stopping warp gate event");
            SlowPortalSpin = true;
            Portals.SetActive(true);
        }
    }

    float timeCount = 0f;
    float originalWarpGateCasingRotateSpeed;

    void Update()
    {
        if (doEvent)
        {
            // lerp warp gate casing to x = 0, y = 0
            if (step == 0)
            {
                WarpGateCasing.transform.rotation = Quaternion.Lerp(WarpGateStartRotation, Quaternion.Euler(0, 0, WarpGateCasing.transform.rotation.eulerAngles.z), timeCount * .8f);
                timeCount += Time.deltaTime;
                if (WarpGateCasing.transform.rotation.eulerAngles.x == 0 && WarpGateCasing.transform.rotation.eulerAngles.y == 0)
                {
                    step++;
                    timeCount = 0;
                    originalWarpGateCasingRotateSpeed = WarpGateCasing.GetComponent<SlowZRotate>().rotateSpeed;
                }
            }
            else if (step == 1)
            {
                // speed up the rotation of the warp gate casing
                WarpGateCasing.GetComponent<SlowZRotate>().rotateSpeed += .01f;
            }


        }
        if (SlowPortalSpin)
        {
            if (WarpGateCasing.GetComponent<SlowZRotate>().rotateSpeed > originalWarpGateCasingRotateSpeed)
            {
                WarpGateCasing.GetComponent<SlowZRotate>().rotateSpeed -= .1f;
            }
            else
            {
                WarpGateCasing.GetComponent<SlowZRotate>().rotateSpeed = originalWarpGateCasingRotateSpeed;
                SlowPortalSpin = false;
            }
        }
    }
}
