using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{

    private InputActionAsset inputs;
    Transform flashlight;
    bool pressed = false;

    // Start is called before the first frame update
    void Start()
    {
        // get the flashlight child
        flashlight = transform.Find("Flashlight");
        inputs = GetComponent<PlayerInput>().actions;
        // enable it
        inputs.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputs["Flashlight"].ReadValue<float>() == 1)
        {
            if (!pressed)
            {
                // if the flashlight is on turn it off, and if its off turn it on
                if (flashlight.gameObject.activeSelf)
                    flashlight.gameObject.SetActive(false);
                else
                    flashlight.gameObject.SetActive(true);
                pressed = true;
            }
        }
        else
            pressed = false;
    }
}
