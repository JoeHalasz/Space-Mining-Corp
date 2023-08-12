using UnityEngine;
using UnityEngine.InputSystem;


/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


public class ShipMovement : MonoBehaviour
{
    
    private float targetYaw;
    private float targetPitch;
    private float targetRoll;

    // player
    private float _flySpeed = 30f;
    private float _flyAcceleration = 3f;
    Vector3 velocity = Vector3.zero;

    Rigidbody _rigidBody;

    GameObject player;

    private InputActionAsset inputs;

    bool lockMovement = true;
    public bool getIsLocked() { return lockMovement; }

    public void LockMovement() { lockMovement = true; }
    public void UnlockMovement() { lockMovement = false; }

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        // find the player
        player = GameObject.Find("Player");
        // get the input action reference
        inputs = player.GetComponent<PlayerInput>().actions;
        // enable it
        inputs.Enable();
    }

    private void Update()
    {
        if (!lockMovement)
            NormalMove();

    }

    private void NormalMove()
    {
        // rotate in the z axis with q and e
        if (inputs["RollLeft"].ReadValue<float>() == 1)
        {
            transform.rotation *= Quaternion.Euler(0, 0, 100 * Time.deltaTime);
        }
        else if (inputs["RollRight"].ReadValue<float>() == 1)
        {
            transform.rotation *= Quaternion.Euler(0, 0, -100 * Time.deltaTime);
        }
        float zBefore = transform.rotation.z;

        Vector3 targetVelocity = new Vector3();

        if (inputs["MoveForward"].ReadValue<float>() == 1)
        {
            targetVelocity += transform.forward * _flySpeed;
        }
        else if (inputs["MoveBackward"].ReadValue<float>() == 1)
        {
            targetVelocity -= transform.forward * _flySpeed;
        }
        // side to side movements
        if (inputs["MoveRight"].ReadValue<float>() == 1)
        {
            if (targetPitch < -90 || targetPitch > 90)
            {
                targetVelocity -= transform.right * _flySpeed;
            }
            else
            {
                targetVelocity += transform.right * _flySpeed;
            }
        }
        else if (inputs["MoveLeft"].ReadValue<float>() == 1)
        {
            if (targetPitch < -90 || targetPitch > 90)
            {
                targetVelocity += transform.right * _flySpeed;
            }
            else
            {
                targetVelocity -= transform.right * _flySpeed;
            }
        }
        // up and down movements
        if (inputs["MoveUp"].ReadValue<float>() == 1)
        {
            if (targetPitch < -90 || targetPitch > 90)
            {
                targetVelocity -= transform.up * _flySpeed;
            }
            else
            {
                targetVelocity += transform.up * _flySpeed;
            }
        }
        else if (inputs["MoveDown"].ReadValue<float>() == 1)
        {
            if (targetPitch < -90 || targetPitch > 90)
            {
                targetVelocity += transform.up * _flySpeed;
            }
            else
            {
                targetVelocity -= transform.up * _flySpeed;
            }
        }

        // slowly change velocity to targetVelocity
        velocity = Vector3.Lerp(velocity, targetVelocity, _flyAcceleration * Time.deltaTime);

        // get mouse x movements
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // change rotation based on mouse movement
        transform.rotation *= Quaternion.Euler(-mouseY, mouseX, 0);

        _rigidBody.velocity = velocity;
    }

}
