using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovementInput : MonoBehaviour
{
    [SerializeField] ShipInputManager.InputType inputType = ShipInputManager.InputType.HumanDesktop;

    public MovementControls movementControls { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        movementControls = ShipInputManager.GetInputControls(inputType);    
    }

    void OnDestroy()
    {
        movementControls = null;
    }


}
