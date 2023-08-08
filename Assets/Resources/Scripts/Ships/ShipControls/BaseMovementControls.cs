using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMovementControls : MovementControls
{ 
    public abstract float yawAmount { get; }
    public abstract float pitchAmount { get; }
    public abstract float rollAmount { get; }
    public abstract float thrustAmount { get; }


}
