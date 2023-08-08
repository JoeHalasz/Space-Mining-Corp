using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZRotate : MonoBehaviour
{
    [SerializeField]
    public float rotateSpeed = -.0025f;

    [SerializeField]
    bool ResetChildren = true;

    // list of all childObjects
    List<GameObject> childObjects = new List<GameObject>();
    // list of all childObjects original positions
    List<Vector3> childObjectsOriginalPositions = new List<Vector3>();
    // list of all childObjects original rotations
    List<Vector3> childObjectsOriginalRotations = new List<Vector3>();

    int numRotatesBeforeReset = 0;
    int numRotates = 0;

    void AddAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            childObjects.Add(child.gameObject);
            childObjectsOriginalPositions.Add(child.position);
            childObjectsOriginalRotations.Add(child.rotation.eulerAngles);
            AddAllChildren(child);
        }
    }

    void Start()
    {
        AddAllChildren(transform);
        numRotatesBeforeReset = (int)(360 / Mathf.Abs(rotateSpeed));
    }

    void FixedUpdate()
    {
        // if the rotation gets back to 0, then reset all the child objects
        if (ResetChildren && numRotates == numRotatesBeforeReset)
        {
            numRotates = 0;
            Debug.Log("Resetting child positions");
            for (int i = 0; i < childObjects.Count; i++)
            {
                childObjects[i].transform.position = childObjectsOriginalPositions[i];
                childObjects[i].transform.rotation = Quaternion.Euler(childObjectsOriginalRotations[i]);
            }
        }
        else
        {
            transform.Rotate(0, 0, rotateSpeed);
            numRotates++;
        }
    }

}
