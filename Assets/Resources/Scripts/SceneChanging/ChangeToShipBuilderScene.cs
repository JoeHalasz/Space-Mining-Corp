using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeToShipBuilderScene : MonoBehaviour
{
    GameObject Player;
    PlayerStats Stats;
    GameObject CurrentShip;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Stats = Player.GetComponent<PlayerStats>();
        // dont destroy the object that this is on
        DontDestroyOnLoad(this);
    }

    void ChangeScene()
    {
        Debug.Log("Changing Scenes");
        // if the current scene is MainScene
        Debug.Log(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main Scene")
        {
            // TODO save the game


            // make sure the player has a playerCurrentShip in the Stats
            if (Stats.playerCurrentShip == null)
            {
                Debug.Log("Player does not have a ship to edit");
                return;
            }
            CurrentShip = Stats.playerCurrentShip;
            DontDestroyOnLoad(CurrentShip);

            // stop ship movement 
            CurrentShip.transform.position = new Vector3(0, 0, 0);
            CurrentShip.transform.rotation = new Quaternion(0, 0, 0, 0);
            CurrentShip.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

            // cant have these or when user clicks on a part of the ship it will focus on the center of the ship
            Destroy(CurrentShip.GetComponent<Rigidbody>());
            Destroy(CurrentShip.GetComponent<MeshCollider>());

            // change scene to the ship builder
            UnityEngine.SceneManagement.SceneManager.LoadScene("Ship Builder");
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Ship Builder")
        {
            // TODO save the ship
            Destroy(CurrentShip); // have to delete this instance of the ship because the rigid body and collider was removed.
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Scene");
        }
    }

    void Update()
    {
        // if the user presses "x" call ChangeScene
        if (Input.GetKeyUp(KeyCode.X))
        {
            ChangeScene();
        }
    }
}
