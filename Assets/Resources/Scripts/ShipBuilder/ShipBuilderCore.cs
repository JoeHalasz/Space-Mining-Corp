using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

public class ShipBuilderCore : MonoBehaviour
{

    GameObject Ship;
    GameObject Player;
    ShipManager shipManager;

    float mouseX;
    float mouseY;

    GameObject MainCamera;

    GameObject centeredAround;
    GameObject selectedObject;

    int mouseHoldThreshhold = 6; // TODO this should use the same player setting from the other scene

    float maxDist = 10f; // TODO make this a zoom control
    float xMoveSense = 3f; // TODO make this a setting
    float yMoveSense = 6f; // TODO make this a setting

    float currentZoom = 10f; // TODO make this change with keybinds

    bool currentlyLerping = false;
    Vector3 lerpTo;
    float lerpSpeed = .15f;

    // This should be in its own scene
    // User should be able to move the camera freely
    // Attach parts to other parts
    // Remove parts 
    // See the current price in ore or credits for the ship with the changed parts

    // On mouse over a part it should highlight it light gray
    // On mouse over a part while holding another part it should snap to the side that the mouse is over
    // On clicking a part it should select the part and highlight it white


    // When attaching a new part in the editor:
    //      the temp ShipManager parts list should be updated 
    //      temp ship parts attached lists should be updated. (This cannot be done after easily)

    // Before save checks:
    //      current cargo <= new max cargo
    //      can generate energy
    //      

    // When saving a ship:
    //      temp ShipManager parts list should replace the real ShipManager parts list
    //      temp ship part attached parts lists should replace the real ones
    //      update max ship cargo size

    // if the user presses the CENTER ( not sure of keybind yet) button,
    // the centeredAround should turn into the selectedObject


    // This should be run when the scene is started for the first time
    void Start()
    {
        // allow the user to see the mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (MainCamera == null)
        {
            Debug.LogError("ShipBuilderCore could not find camera");
        }

        // get the player
        // TODO load this from the other scene?
        // or maybe load from disk if thats not possible?
        return;
        Player = GameObject.FindGameObjectWithTag("Player");
        if (Player == null)
        {
            Debug.LogError("ShipBuilderCore could not find the player");
        }
        else
        {
            // get the players ship
            Ship = Player.GetComponent<PlayerStats>().playerCurrentShip;
            if (Ship == null)
            {
                Debug.LogError("ShipBuilderCore could not find the players ship");
            }
            else
            {
                shipManager = Ship.GetComponent<ShipManager>();
            }
        }

    }

    void OnDestroy()
    {
        // hide the mouse again
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    int mouseDownFrames = 0;

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            mouseDownFrames += 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        checkAndLerpCamera();

        // TODO delete this and make it an actual key bind
        {
            if (Input.GetKeyDown("c"))
            {
                Debug.Log("Camera Centered (DELETE ME)"); // dont just delete the comment. Make this a real control
                OnCenterCameraPress();
            }
        }

        playerMouseInputs();
    }

    void checkAndLerpCamera()
    {
        if (currentlyLerping)
        {
            // if its close enough then set the pos
            if (Vector3.Distance(MainCamera.transform.position, lerpTo) <= lerpSpeed)
                MainCamera.transform.position = lerpTo;
            if (MainCamera.transform.position != lerpTo)
                MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, lerpTo, lerpSpeed);
            else
                currentlyLerping = false;
        }
    }

    void playerMouseInputs()
    {
        bool clickHeld = false;
        // TODO check if the player is holding click
        if (Input.GetMouseButton(0))
        {
            if (mouseDownFrames >= mouseHoldThreshhold)
            {
                clickHeld = true;
            }
        }
        if (clickHeld && !currentlyLerping)
        {
            orbitCenteredObject();
        }
        else
        {
            bool clicked = false;
            // TODO check if the player clicked an object
            if (Input.GetMouseButton(0) == false && mouseDownFrames != 0 && mouseDownFrames < mouseHoldThreshhold)
            {
                clicked = true;
            }
            if (clicked)
            {
                selectOrDeselectObject();
            }
        }
        if (Input.GetMouseButton(0) == false)
        {
            mouseDownFrames = 0;
        }
    }


    void orbitCenteredObject()
    {
        // orbit camera around the centered object, or around 0,0
        Vector3 centerPos;
        if (centeredAround != null)
            centerPos = centeredAround.transform.position;
        else
            centerPos = new Vector3(0, 0, 0);
        if (Input.GetMouseButton(0))
        {
            MainCamera.transform.RotateAround(centerPos, MainCamera.transform.up, Input.GetAxis("Mouse X") * xMoveSense);

            MainCamera.transform.RotateAround(centerPos, MainCamera.transform.right, -Input.GetAxis("Mouse Y") * yMoveSense);
        }

        MainCamera.transform.LookAt(centerPos);
    }


    void OnCenterCameraPress()
    {
        centeredAround = selectedObject; // also works when selectedObject is null
        Vector3 lookAtPos;
        if (centeredAround == null)
            lookAtPos = new Vector3(0, 0, 0);
        else
            lookAtPos = centeredAround.transform.position;

        //MainCamera.transform.LookAt(lookAtPos);
        Vector3 newPos = lookAtPos - (currentZoom * MainCamera.transform.forward);

        // lerp to newPos
        currentlyLerping = true;
        lerpTo = newPos;
    }

    void selectOrDeselectObject()
    {
        // cast a ray through the scene in the direction that the player clicked 
        // using the direction the camera is facing.
        // if that ray intersects any ship part, then select it.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // draw the ray and keep it there
        Debug.DrawRay(ray.origin, ray.direction * 5, Color.blue, 10);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Object found");
            selectObject(hit.transform.gameObject);
        }
        else
        {
            Debug.Log("Object not found");
            deselectObject();
        }
    }

    void selectObject(GameObject obj)
    {
        if (obj != null)
        {
            if (selectedObject != null)
            {
                deselectObject();
            }
            selectedObject = obj;
            // TODO white highlight the object white somehow
            // TODO change the buttons so that they interact with the object
        }
    }

    void deselectObject()
    {
        if (selectedObject != null)
        {
            // TODO get rid of highlight around the object
            // TODO remove buttons that interact with an object
            selectedObject = null;
        }
    }
}
