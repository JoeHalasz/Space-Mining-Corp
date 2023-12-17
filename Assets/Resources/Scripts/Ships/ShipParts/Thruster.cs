using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : ShipPart
{
    // Start is called before the first frame update
    void Start()
    {
        name = "Thruster";
        addFrontConnection();
        addLeftConnection();
        addRightConnection();
        addTopConnection();
        addBottomConnection();
    }
}
