using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchRotation : MonoBehaviour
{
    [SerializeField]
    Transform target;


    void LateUpdate()
    {
        if (target != null)
        {
            transform.rotation = target.rotation;
        }
    }
}
