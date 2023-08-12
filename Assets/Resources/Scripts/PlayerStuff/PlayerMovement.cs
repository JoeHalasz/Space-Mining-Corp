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

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.5f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

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

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

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

        // change rotation based on mouse movement
        transform.rotation *= Quaternion.Euler(-mouseY, mouseX, 0);


        // get the gameObject that the transform is grounded to 
        if (Grounded)
        {
            GameObject groundedTo = null;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, GroundLayers))
                groundedTo = hit.collider.gameObject;

            // make that this transforms parent
            if (groundedTo != null)
            {
                // if groundedTo has a rigidbody, add its velocity to this transform
                Rigidbody rb = groundedTo.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // get this transforms rb
                    playerVelocity += rb.velocity;
                }
                playerVelocity -= groundedTo.transform.up * 9.81f * Time.deltaTime; // gravity 
                // slowly change the players z rotation to match the groundedTo
                Quaternion targetRotation = Quaternion.LookRotation(transform.forward, groundedTo.transform.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, targetRotation.eulerAngles.z);
            }

        }

        _rigidBody.velocity = playerVelocity;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
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
