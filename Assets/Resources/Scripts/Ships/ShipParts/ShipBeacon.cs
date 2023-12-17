using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBeacon : ShipPart
{
    void Start()
    {
        name = "ShipBeacon";
        addBottomConnection();
    }
}
