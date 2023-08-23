using UnityEngine;
using UnityEngine.InputSystem;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


public class PlayerMovement : MonoBehaviour
{
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    float GroundedOffset = 0f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    float groundAngle;

    // cinemachine
    private float targetYaw;
    private float targetPitch;
    private float targetRoll;

    // player
    private float _flySpeed = 10f;
    private float _flyAcceleration = 5f;
    Vector3 playerVelocity = Vector3.zero;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private InputActionAsset inputs;

    private bool _hasAnimator;

    Rigidbody _rigidBody;

    Vector3 groundNormal;

    bool lockPlayerMovement = false;
    public bool getIsLocked() { return lockPlayerMovement; }
    
    GameObject lockedReason = null;
    public GameObject GetLockedReason() { return lockedReason; }

    public void LockPlayerMovement(GameObject reason) { 
        lockPlayerMovement = true; 
        _rigidBody.velocity = new Vector3(0, 0, 0); 
        _rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        lockedReason = reason;
    }
    public void UnlockPlayerMovement() { 
        lockPlayerMovement = false; 
        lockedReason = null; 
        // unfreze the rigidbodys position
        _rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    bool lockPlayerInputs = false;
    public bool getPlayerInputsLocked() { return lockPlayerInputs; }
    public void LockPlayerInputs(GameObject reason) { lockPlayerInputs = true; lockedReason = reason; }
    public void UnlockPlayerInputs() { lockPlayerInputs = false; lockedReason = null; }


    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _rigidBody = GetComponent<Rigidbody>();

        AssignAnimationIDs();
        // get the input action reference
        inputs = GetComponent<PlayerInput>().actions;
        // enable it
        inputs.Enable();
    }

    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        GroundedCheck();
        if (!lockPlayerMovement && !lockPlayerInputs)
            NormalMove();

    }

    private void LateUpdate()
    {
        //CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        //_animIDSpeed = Animator.StringToHash("Speed");
        //_animIDGrounded = Animator.StringToHash("Grounded");
        //_animIDJump = Animator.StringToHash("Jump");
        //_animIDFreeFall = Animator.StringToHash("FreeFall");
        //_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    void OnCollisionStay(Collision collision)
    {
        if (Grounded || true)
        {
            ContactPoint contact = collision.contacts[0];
            groundNormal = contact.normal;
            Vector3 temp = Vector3.Cross(contact.normal, Vector3.down);
            var groundSlopeDir = Vector3.Cross(temp, contact.normal);
            var groundSlopeAngle = Vector3.Angle(contact.normal, Vector3.up);
            // debug.log it
            Debug.DrawRay(contact.point, contact.normal, Color.red, 5f);
            Debug.DrawRay(contact.point, groundSlopeDir, Color.green, 5f);
            Debug.Log("here");

            // get angle between contact.point and contact.normal
            float angle = Vector3.Angle(contact.point, contact.normal) - 90;
            // if the player is looking backwards in the y then invert it
            if (transform.localRotation.eulerAngles.y > 180)
                angle *= -1;
            // rotate players z axis to match the angle
            groundAngle = angle;
        }
    }

    private void GroundedCheck()
    {
        RaycastHit hitInfo;
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedRadius*2, transform.position.z);
        Grounded = Physics.SphereCast(spherePosition, GroundedRadius, -transform.up, out hitInfo,
                       GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        // draw that 
        //Debug.DrawRay(spherePosition, -transform.up * (GroundedRadius));
        //Physics.SphereCast(new Ray(this.transform.position + new Vector3(0,this.transform.localScale.y-2,0), -this.transform.up), this.transform.localScale.y * -0.5f, this.transform.localScale.y);
        // draw that 
        //Debug.DrawRay(this.transform.position + new Vector3(0, this.transform.localScale.y, 0), -this.transform.up * (this.transform.localScale.y * 0.5f));
        if (Grounded)
        {
            // rotate the player to match the ground normal
            //transform.rotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, hitInfo.normal.y, transform.localRotation.eulerAngles.z);
        }

        // update animator if using character
        if (_hasAnimator)
        {
            //_animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void NormalMove()
    {
        // this is when the player is floating 

        if (!Grounded && inputs["RollLeft"].ReadValue<float>() == 1)
        {
            transform.rotation *= Quaternion.Euler(0, 0, 100 * Time.deltaTime);
        }
        if (!Grounded && inputs["RollRight"].ReadValue<float>() == 1)
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

        // if the player right clicks, teleport them 100 units in the direction they are facing. This will be removed for real game
        if (Input.GetMouseButtonUp(1))
        {
            transform.position += transform.forward * 100;
        }

        // slowly change playerVelocity to targetVelocity
        playerVelocity = Vector3.Lerp(playerVelocity, targetVelocity, _flyAcceleration * Time.deltaTime);

        // get mouse x movements
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (!Grounded)
        {
            // if the camera has any x rotation, move it to the player
            if (_mainCamera.transform.localRotation.eulerAngles.x != 0)
            {
                transform.localRotation = Quaternion.Euler(_mainCamera.transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
                _mainCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            // change rotation based on mouse movement
            transform.localRotation *= Quaternion.Euler(-mouseY, mouseX, 0);
        }
        else
        {   // if the camera has any x rotation, move it to the player
            if (transform.localRotation.eulerAngles.x != 0)
            {
                _mainCamera.transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, _mainCamera.transform.localRotation.eulerAngles.y, _mainCamera.transform.localRotation.eulerAngles.z);
                transform.localRotation = Quaternion.Euler(transform.localRotation.x, 0, 0);
            }
            // rotate like a normal player on the ground
            transform.localRotation *= Quaternion.Euler(0, mouseX, 0);
            _mainCamera.transform.rotation *= Quaternion.Euler(-mouseY, 0, 0);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, groundAngle);
        }

        _rigidBody.velocity = playerVelocity;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmos()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        //Gizmos.DrawSphere(
        //    new Vector3(transform.position.x, transform.position.y, transform.position.z),
        //    GroundedRadius);

    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}
