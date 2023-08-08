using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInputManager : MonoBehaviour
{
    public enum InputType
    {
        HumanDesktop,
        Bot
    }

    public static MovementControls GetInputControls(InputType type)
    {
        return type switch
        {
            InputType.HumanDesktop => new DesktopMovementControls(),
            InputType.Bot => null,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    
    }
}
