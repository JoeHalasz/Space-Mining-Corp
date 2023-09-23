using System.Collections.Generic;
using UnityEngine;

public class InteractWithInventory : MonoBehaviour
{

    // get this game objects inventory
    private Inventory inventory;
    
    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    // Update is called once per frame
    public void HandleHit(Ray ray, RaycastHit hit)
    {
        if (GetComponent<UIManager>().getUIOpen())
            return;

        Inventory hitInventory = hit.collider.GetComponent<Inventory>();
        if (hitInventory != null)
        {
            if (hit.collider.transform.name == "OreRefinery")
            {
                Debug.Log("Trying to open refinery inventory and ship inventory");
                hitInventory.openInventory(GetComponent<PlayerStats>().playerCurrentShip, false);
            }
            else
            {
                hitInventory.openInventory(this.gameObject, false);
            }
            return;
        }

        // check if the thing that was hit has a parent
        if (hit.collider.transform.parent != null)
        {
            // if that parent has an inventory has an inventory
            hitInventory = hit.collider.transform.parent.GetComponent<Inventory>();
            if (hitInventory != null)
            {
                hitInventory.openInventory(this.gameObject, false);
            }
        }
    }
}
