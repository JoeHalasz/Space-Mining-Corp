using System;
using UnityEngine;

[Serializable]
public class DesktopMovementControls : BaseMovementControls
{
    [SerializeField] float deadZoneRadius = .1f;

    Vector2 ScreenCenter => new Vector2(Screen.width * .5f, Screen.height * .5f);

    public override float yawAmount
    {
        get
        {
            Vector3 mousePosition = Input.mousePosition;
            float yaw = (mousePosition.x - ScreenCenter.x) / ScreenCenter.x;
            return Mathf.Abs(yaw) < deadZoneRadius ? 0f : yaw;
        }
    }

    public override float pitchAmount
    {
        get
        {
            Vector3 mousePosition = Input.mousePosition;
            float pitch = (mousePosition.y - ScreenCenter.y) / ScreenCenter.y;
            return Mathf.Abs(pitch) < deadZoneRadius ? 0f : pitch*-1f;
        }
    }

    public override float rollAmount
    {
        get
        {
            if (Input.GetKey(KeyCode.Q))
            {
                return 1f;
            }
            
            return Input.GetKey(KeyCode.E) ? -1f : 0f;
        }
    }

    public override float thrustAmount
    {
        get
        {
            if (Input.GetKey(KeyCode.W))
            {
                return 1f;
            }
            
            return Input.GetKey(KeyCode.S) ? -1f : 0f;
        }
    }

}
