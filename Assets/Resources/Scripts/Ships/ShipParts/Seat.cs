using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Seat : MonoBehaviour
{

    GameObject allMovableObjects;
    void Start()
    {
        allMovableObjects = GameObject.Find("All Movable Objects");
    }

    public void MakePlayerSitOrUnsit(GameObject player)
    {
        // if the player is already sitting unsit them, otherwise sit them
        if (player.GetComponent<PlayerMovement>().getIsLocked())
            MakePlayerStand(player);
        else
            MakePlayerSit(player);
    }

    void MakePlayerSit(GameObject player)
    {
        // move the player to the seat 
        player.transform.localPosition = transform.localPosition;
        player.transform.rotation = transform.rotation;
        // set ship as player parent
        player.transform.parent = transform.parent;
        // lock the players movement 
        player.GetComponent<PlayerMovement>().LockPlayerMovement(this.gameObject);
        // unlock the parent ships movement
        transform.parent.GetComponent<ShipMovement>().UnlockMovement();
        // turn off the players collider
        player.GetComponent<Collider>().enabled = false;
    }

    void MakePlayerStand(GameObject player)
    {
        // set the players parent to null
        player.transform.parent = allMovableObjects.transform;
        // unlock the players movement 
        player.GetComponent<PlayerMovement>().UnlockPlayerMovement();
        // lock the parent ships movement
        transform.parent.GetComponent<ShipMovement>().LockMovement();
        // turn on the players collider
        player.GetComponent<Collider>().enabled = true;
    }
}
