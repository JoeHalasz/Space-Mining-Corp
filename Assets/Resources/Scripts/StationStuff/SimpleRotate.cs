using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{

    public float xRotateAmount;
    public float yRotateAmount;
    public float zRotateAmount;


    void FixedUpdate()
    {
        // rotate this
        transform.Rotate(xRotateAmount * Time.deltaTime, yRotateAmount * Time.deltaTime, zRotateAmount * Time.deltaTime);
    }
}
