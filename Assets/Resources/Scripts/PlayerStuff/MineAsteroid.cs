using UnityEngine;

public class MineAsteroid : MonoBehaviour
{

    UIManager uiManager;

    void Start()
    {
        uiManager = GetComponent<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // when the player left clicks, cast a ray
        if (Input.GetMouseButtonDown(0) && !uiManager.UIOpen)
        {
            // cast a ray that stops at the first object it hits
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // draw the ray and keep it there
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.red, 10);
      
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
