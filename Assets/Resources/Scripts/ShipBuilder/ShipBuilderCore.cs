using UnityEngine;

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
    GameObject hoveredObject;

    int mouseHoldThreshhold = 6; // TODO this should use the same player setting from the other scene

    float xMoveSense = 3f; // TODO make this a setting
    float yMoveSense = 6f; // TODO make this a setting

    float currentZoom = 20f; // TODO make this change with keybinds
    float minZoom = 5f;
    float maxZoom = 50f;

    Vector3 lookAtPos;

    bool currentlyLerping = false;
    Vector3 lerpTo;
    float lerpSpeed = .15f;

    Color selectedObjectColor = Color.white;
    Color hoveredObjectColor = Color.white;

    ShipBuilderUIHandler UIHandler;


    // DONE This should be in its own scene
    // DONE User should be able to move the camera freely
    // TODO Attach parts to other parts
    // TODO Remove parts 
    // TODO See the current price in ore or credits for the ship with the changed parts

    // DONE On mouse over a part it should highlight it light gray
    // TODO On mouse over a part while holding another part it should snap to the side that the mouse is over
    // DONE On clicking a part it should select the part and highlight it white


    // When attaching a new part in the editor:
    //      TODO the temp ShipManager parts list should be updated 
    //      TODO temp ship parts attached lists should be updated. (This cannot be done after easily)

    // Before save checks:
    //      TODO current cargo <= new max cargo
    //      TODO can generate energy
    //      

    // When saving a ship:
    //      TODO temp ShipManager parts list should replace the real ShipManager parts list
    //      TODO temp ship part attached parts lists should replace the real ones
    //      TODO update max ship cargo size

    // DONE if the user presses the CENTER ( not sure of keybind yet) button,
    //      the centeredAround should turn into the selectedObject


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

        OnCenterCameraPress();

        UIHandler = GetComponent<ShipBuilderUIHandler>();
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

        // TODO delete this and make actual key binds
        {
            if (Input.GetKeyDown("c"))
            {
                Debug.Log("Camera Centered (DELETE ME)"); // dont just delete the comment. Make this a real control
                OnCenterCameraPress();
            }
            // up arrow
            if (Input.GetKeyDown("up"))
            {
                Debug.Log("Camera Zoomed In (DELETE ME)"); // dont just delete the comment. Make this a real control
                OnZoomInPress();
            }
            // down arrow
            if (Input.GetKeyDown("down"))
            {
                Debug.Log("Camera Zoomed Out (DELETE ME)"); // dont just delete the comment. Make this a real control
                OnZoomOutPress();
            }
        }

        playerMouseInputs();
    }

    void OnCenterCameraPress()
    {
        centeredAround = selectedObject; // also works when selectedObject is null
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

    void OnZoomInPress()
    {
        currentZoom -= 1f;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        Vector3 newPos = lookAtPos - (currentZoom * MainCamera.transform.forward);
        currentlyLerping = true;
        lerpTo = newPos;
    }

    void OnZoomOutPress()
    {
        currentZoom += 1f;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        Vector3 newPos = lookAtPos - (currentZoom * MainCamera.transform.forward);
        currentlyLerping = true;
        lerpTo = newPos;
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
            if (Input.GetMouseButton(0) == false && mouseDownFrames != 0 && mouseDownFrames < mouseHoldThreshhold && !UIHandler.GetJustClickedUI())
            {
                clicked = true;
            }
            if (clicked)
            {
                selectOrDeselectObject();
            }
            else
            {
                hoverOrUnhoverObject();
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


    void hoverOrUnhoverObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject != hoveredObject)
            {
                unhoverObject();
            }
            if (hit.transform.gameObject != selectedObject)
            {
                hoverObject(hit.transform.gameObject);
            }
        }
        else
        {
            unhoverObject();
        }
    }

    void hoverObject(GameObject obj)
    {
        if (obj != null && obj != hoveredObject)
        {
            // get all the objects materials and add some emission to them
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", hoveredObjectColor * .02f);
            }
            hoveredObject = obj;
            Debug.Log("hovering");
        }
    }

    void unhoverObject()
    {
        if (hoveredObject != null)
        {
            Renderer[] renderers = hoveredObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.DisableKeyword("_EMISSION");
            }
            Debug.Log("Unhovering");
            hoveredObject = null;
        }
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
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject == hoveredObject)
                unhoverObject();
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

            // get all the objects materials and add some emission to them
            Renderer[] renderers = selectedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", selectedObjectColor*.05f);
            }
            
            // TODO change the buttons so that they interact with the object
        }
    }

    void deselectObject()
    {
        if (selectedObject != null)
        {
            Renderer[] renderers = selectedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // TODO get rid of highlight around the object
                renderer.material.DisableKeyword("_EMISSION");
            }
            // TODO remove buttons that interact with an object
            selectedObject = null;
        }
    }
}
