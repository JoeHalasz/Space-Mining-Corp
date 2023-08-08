using UnityEngine;


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

    bool lockMovement = true;
    public bool getIsLocked() { return lockMovement; }

    public void LockMovement() { lockMovement = true; }
    public void UnlockMovement() { lockMovement = false; }

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!lockMovement)
            NormalMove();

    }

    private void NormalMove()
    {
        // rotate in the z axis with q and e
        if (Input.GetKey(KeyCode.Q))
        {
            transform.rotation *= Quaternion.Euler(0, 0, 100 * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.rotation *= Quaternion.Euler(0, 0, -100 * Time.deltaTime);
        }
        float zBefore = transform.rotation.z;

        Vector3 targetVelocity = new Vector3();

        if (Input.GetKey(KeyCode.W))
        {
            targetVelocity += transform.forward * _flySpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            targetVelocity -= transform.forward * _flySpeed;
        }
        // side to side movements
        if (Input.GetKey(KeyCode.D))
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
        else if (Input.GetKey(KeyCode.A))
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
        if (Input.GetKey(KeyCode.Space))
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
        else if (Input.GetKey(KeyCode.LeftControl))
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
