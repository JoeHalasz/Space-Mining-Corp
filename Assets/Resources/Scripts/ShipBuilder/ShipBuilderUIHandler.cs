using UnityEngine;

public class ShipBuilderUIHandler : MonoBehaviour
{

    bool justClickedUI = false;

    public bool GetJustClickedUI()
    {
        return justClickedUI;
    }

    void LateUpdate()
    {
        justClickedUI = false;
    }

    public void OnBaseButtonPress()
    {
        Debug.Log("Base button pressed");
        justClickedUI = true;
    }

    public void OnMovementButtonPress()
    {
        Debug.Log("Movement button pressed");
        justClickedUI = true;
    }

    public void OnInteriorButtonPress()
    {
        Debug.Log("Interior button pressed");
        justClickedUI = true;
    }

    public void OnAccessoriesButtonPress()
    {
        Debug.Log("Accessories button pressed");
        justClickedUI = true;
    }

    public void OnExtensionsButtonPress()
    {
        Debug.Log("Extensions button pressed");
        justClickedUI = true;
    }
}
