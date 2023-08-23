using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachOtherToStation : MonoBehaviour
{

    GameObject getFinalParent(GameObject obj)
    {
        // if parent is null return obj
        if (obj.transform.parent == null || obj.transform.parent.name == "Station")
            return obj;
        return getFinalParent(obj.transform.parent.gameObject);
    }


    // if a gameobject goes into the trigger, attach it to the station
    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            getFinalParent(other.gameObject).transform.SetParent(transform);
    }

    // if a gameobject leaves the trigger, detach it from the station
    void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            GameObject obj = getFinalParent(other.gameObject);
            obj.transform.parent = null;
        }
    }
}
