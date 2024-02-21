using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectPosition : MonoBehaviour
{
    [SerializeField]
    GameObject other;

    void Start()
    {
        // if other is null set it to the player
        if (other == null)
        {
            other = GameObject.Find("Player");
        }
    }

    void LateUpdate()
    {
        if (other != null)
        {
            transform.position = other.transform.position;
        }
    }
}
