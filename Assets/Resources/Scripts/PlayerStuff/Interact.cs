using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{

    InteractWithInventory inventoryHitHandler;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        // set up scripts that handle different types of interactions
        inventoryHitHandler = GetComponent<InteractWithInventory>();
        playerMovement = GetComponent<PlayerMovement>();
    }


    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {

            // if the player is sitting, then unsit them
            if (playerMovement.getIsLocked())
            {
                GameObject seat = playerMovement.GetLockedReason();
                seat.GetComponent<Seat>().MakePlayerSitOrUnsit(transform.gameObject);
            }
            else if (playerMovement.getPlayerInputsLocked() && (playerMovement.GetLockedReason().tag == "Inventory" || playerMovement.GetLockedReason().tag == "Ship"))
            {
                GameObject reason = playerMovement.GetLockedReason();
                if (reason != null)
                {
                    reason.GetComponent<Inventory>().closeInventory(this.gameObject);
                    GetComponent<UIManager>().UIOpen = false;
                }
                // player is in a menu, so close it
                playerMovement.UnlockPlayerInputs();
            }
            else if (playerMovement.getPlayerInputsLocked() && playerMovement.GetLockedReason().tag == "NPC")
            {
                GameObject reason = playerMovement.GetLockedReason();
                if (reason != null)
                {
                    reason.GetComponent<NPCmanager>().InteractWithPlayer(this.gameObject);
                }
                // player is in a menu, so close it
                playerMovement.UnlockPlayerInputs();

            }
            else
            {
                // cast a ray that stops at the first object it hits
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                // draw the ray and keep it there
                Debug.DrawRay(ray.origin, ray.direction * 5, Color.yellow, 10);

                if (Physics.Raycast(ray, out hit))
                {
                    // if the ray hits something within 5 units then mine it
                    if (hit.distance < 5)
                    {
                        HandleValidHit(ray, hit);
                    }
                }
            }
        }
    }


    public void HandleValidHit(Ray ray, RaycastHit hit)
    {
        // if we hit cargo
        if (hit.collider.transform.tag == "Cargo")
            inventoryHitHandler.HandleHit(ray, hit);
        
        // if we hit a Seat
        else if (hit.collider.transform.tag == "Seat")
            hit.collider.transform.GetComponent<Seat>().MakePlayerSitOrUnsit(transform.gameObject);

        else if (hit.collider.transform.tag == "NPC")
            hit.collider.transform.GetComponent<NPCmanager>().InteractWithPlayer(gameObject);
        
        
    }
}
