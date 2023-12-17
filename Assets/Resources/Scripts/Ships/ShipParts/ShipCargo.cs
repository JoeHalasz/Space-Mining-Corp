using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCargo : ShipPart
{
    void Start()
    {
        name = "ShipCargo";
        addAllConnections();
    }
}
