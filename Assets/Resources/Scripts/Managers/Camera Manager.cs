using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [SerializeField] Camera firstPersonCamera;
    [SerializeField] Camera thirdPersonCamera;

    void Start()
    {
        // set the third person camera to be active
        firstPersonCamera.enabled = false;
        thirdPersonCamera.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // when the user presses f3, switch the camera
        if (Input.GetKeyDown(KeyCode.F3))
        {
            // if the first person camera is active, switch to the third person camera
            if (firstPersonCamera.enabled)
            {
                firstPersonCamera.enabled = false;
                thirdPersonCamera.enabled = true;
            }
            // if the third person camera is active, switch to the first person camera
            else
            {
                firstPersonCamera.enabled = true;
                thirdPersonCamera.enabled = false;
            }
        }
    }
}
