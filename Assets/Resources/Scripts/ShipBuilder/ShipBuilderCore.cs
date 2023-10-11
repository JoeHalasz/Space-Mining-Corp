using UnityEngine;


public class ShipBuilderCore : MonoBehaviour
{

    GameObject Ship;
    GameObject Player;
    ShipManager shipManager;



    // This should be in its own scene
    // User should be able to move the camera freely
    // Attach parts to other parts
    // Remove parts 
    // See the current price in ore or credits for the ship with the changed parts

    // On mouse over a part it should highlight it
    // On mouse over a part while holding another part it should snap to the side that the mouse is over

    // When attaching a new part in the editor:
    //      the temp ShipManager parts list should be updated 
    //      temp ship parts attached lists should be updated. (This cannot be done after easily)

    // Before save checks:
    //      current cargo <= new max cargo

    // When saving a ship:
    //      temp ShipManager parts list should replace the real ShipManager parts list
    //      temp ship part attached parts lists should replace the real ones
    //      update max ship cargo size


    // This should be run when the scene is started for the first time
    void Start()
    {
        // get the player
        Player = GameObject.FindGameObjectWithTag("Player");
        // get the players ship
        Ship = Player.GetComponent<PlayerStats>().playerCurrentShip;
        if (Ship == null)
        {
            Debug.LogError("ShipBuilderCore could not find the players ship");
            return;
        }

        shipManager = Ship.GetComponent<ShipManager>();

        // allow the user to see the mouse




    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        // hide the mouse again
    }
}
