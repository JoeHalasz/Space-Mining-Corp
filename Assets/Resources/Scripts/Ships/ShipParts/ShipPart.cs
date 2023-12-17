using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPart : MonoBehaviour
{
    string name = "default";

    enum PossibleDirections { None, Left, Right, Top, Bottom, Front, Back };

    List<PossibleDirections> directionsToBuildFrom = new List<PossibleDirections>();

    public void addAllConnections()
    {
        directionsToBuildFrom.Clear();
        addBottomConnection();
        addLeftConnection();
        addRightConnection();
        addTopConnection();
        addFrontConnection();
        addBackConnection();
    }

    public void addBottomConnection() { directionsToBuildFrom.Add(PossibleDirections.Bottom); }
    public void addLeftConnection() { directionsToBuildFrom.Add(PossibleDirections.Left); }
    public void addRightConnection() { directionsToBuildFrom.Add(PossibleDirections.Right); }
    public void addTopConnection() { directionsToBuildFrom.Add(PossibleDirections.Top); }
    public void addFrontConnection() { directionsToBuildFrom.Add(PossibleDirections.Front); }
    public void addBackConnection() { directionsToBuildFrom.Add(PossibleDirections.Back); }


    void Execute()
    {
        Debug.Log("Execute must be overridden for " + name);
        // this is where code will live for each ship part.
        // Must be overridden by each ship part
    }

}
