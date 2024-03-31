using UnityEngine;
using UnityEngine.InputSystem;

public class MineAsteroid : MonoBehaviour
{

    UIManager uiManager;

    // variable that saves the last time something was mined
    bool held = false;

    public float maxDistOfRay = 8f;
    public float mineDelayStart = .5f;
    public float mineDelayMin = .3f;
    public float mineDelayDelta = .025f;
    float currentMineDelay;
    float mineDelayTimeout = .5f;

    float lastTimeMined = 0;
    float lastTimeAttemptedMine = 0;
    WorldManager worldManager;

    void Start()
    {
        uiManager = GetComponent<UIManager>();
        currentMineDelay = mineDelayStart;
        lastTimeMined = Time.time;
        lastTimeAttemptedMine = Time.time;
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
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
        // if it has been currentMineDelay seconds
        if (currentTime - lastTimeMined > currentMineDelay)
        {
            if (!uiManager.getUIOpen())
            {
                lastTimeAttemptedMine = Time.time;
                // cast a ray that stops at the first object it hits
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // if the ray hits an astroid within 8 units then mine it
                    if (hit.distance < maxDistOfRay && hit.collider.tag == "Asteroid")
                    {
                        if (hit.transform.GetComponent<AsteroidGenerator>() == null)
                            Destroy(hit.transform.gameObject);
                        else{
                            hit.transform.GetComponent<AsteroidGenerator>().MineAsteroid(transform.gameObject, ray, hit, ray.direction, 5, worldManager);
                            if (currentMineDelay > mineDelayMin)
                            {
                                currentMineDelay -= mineDelayDelta;
                            }
                            lastTimeMined = Time.time;
                            Invoke("resetMineDelay", mineDelayTimeout);
                        }
                    }
                }
            }
        }
    }

    void resetMineDelay()
    {
        float currentTime = Time.time;
        if (currentTime - lastTimeAttemptedMine >= mineDelayTimeout)
        {
            currentMineDelay = mineDelayStart;
        }
    }
}
