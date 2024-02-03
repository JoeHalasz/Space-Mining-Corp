using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse_move : MonoBehaviour
{

    GameObject _mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = GameObject.Find("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Debug.Log("Moving to flying view movement");
        transform.localRotation = Quaternion.Euler(_mainCamera.transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
        _mainCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        // change rotation based on mouse movement
        transform.localRotation *= Quaternion.Euler(-mouseY, mouseX, 0);
    }
}
