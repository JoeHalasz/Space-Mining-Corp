using UnityEngine;
using UnityEngine.InputSystem;

public class MineAsteroid : MonoBehaviour
{

    UIManager uiManager;

    // variable that saves the last time something was mined
    float lastMineTime = 0;
    bool held = false;

    void Start()
    {
        uiManager = GetComponent<UIManager>();
    }


    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
            tryMine();
        if (context.performed)
            held = true;
        if (context.canceled)
            held = false;
    }

    void Update()
    {
        if (held)
        {
            tryMine();
        }
    }

    void tryMine()
    {
        
        // get the current time
        float currentTime = Time.time;
        // if it has been .2 seconds
        if (currentTime - lastMineTime > .2f)
        {
            if (!uiManager.getUIOpen())
            {
                lastMineTime = currentTime;
                // cast a ray that stops at the first object it hits
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // if the ray hits an astroid within 10 units then mine it
                    if (hit.distance < 20 && hit.collider.tag == "Asteroid")
                    {
                        if (hit.transform.GetComponent<AsteroidGenerator>() == null)
                            Destroy(hit.transform.gameObject);
                        else
                            hit.transform.GetComponent<AsteroidGenerator>().MineAsteroid(transform.gameObject, ray, hit, ray.direction, 5);
                    }
                }
            }
        }
    }
}
