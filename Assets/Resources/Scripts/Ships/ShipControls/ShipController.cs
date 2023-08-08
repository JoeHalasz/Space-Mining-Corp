using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField]
    ShipMovementInput movementInput;

    [SerializeField][Range(1000f, 10000f)]
    float thrustForce = 7500f, pitchForce = 6000f, rollForce = 1000f, yawForce = 2000f;

    Rigidbody rigidBody;
    [SerializeField][Range(-1f,1f)]
    float thrustAmount, pitchAmount, rollAmount, yawAmount = 0f;
    [SerializeField] bool playerControlling = false;

    MovementControls controlInput => movementInput.movementControls;
    Rigidbody player;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (controlInput != null)
        {
            thrustAmount = controlInput.thrustAmount;
            pitchAmount = controlInput.pitchAmount;
            rollAmount = controlInput.rollAmount;
            yawAmount = controlInput.yawAmount;
        }
    }

    void FixedUpdate()
    {
        // get player rotation
        if (playerControlling) // updated by PlayerAttachToShip.cs
        {
            Quaternion playerRotation = player.transform.rotation;
            if (!Mathf.Approximately(0f, pitchAmount))
            {
                rigidBody.AddTorque(transform.right * pitchForce * pitchAmount * rigidBody.mass * Time.fixedDeltaTime);
            }

            if (!Mathf.Approximately(0f, rollAmount))
            {
                rigidBody.AddTorque(transform.forward * rollForce * rollAmount * rigidBody.mass * Time.fixedDeltaTime);
            }

            if (!Mathf.Approximately(0f, yawAmount))
            {
                rigidBody.AddTorque(transform.up * yawForce * yawAmount * rigidBody.mass * Time.fixedDeltaTime);
            }
            if (!Mathf.Approximately(0f, thrustAmount))
            {
                rigidBody.AddForce(transform.forward * thrustForce * thrustAmount * Time.fixedDeltaTime);
            }
            // set player rotation to what it was before this
            player.transform.rotation = playerRotation;
        }


    }

    public void SetPlayerControllingTrue(Rigidbody controllingPlayer)
    {
        playerControlling = true;
        player = controllingPlayer;
    }

    public void SetPlayerControllingFalse()
    {
        playerControlling = false;
        player = null;
    }
}
