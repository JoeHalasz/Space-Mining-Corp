using UnityEngine;

public class ShipBuilderUIHandler : MonoBehaviour
{
    public void OnBaseButtonPress()
    {
        Debug.Log("Base button pressed");
    }

    public void OnMovementButtonPress()
    {
        Debug.Log("Movement button pressed");
    }

    public void OnInteriorButtonPress()
    {
        Debug.Log("Interior button pressed");
    }

    public void OnAccessoriesButtonPress()
    {
        Debug.Log("Accessories button pressed");
    }

    public void OnExtensionsButtonPress()
    {
        Debug.Log("Extensions button pressed");
    }
}
