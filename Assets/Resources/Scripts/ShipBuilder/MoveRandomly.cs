using UnityEngine;

public class MoveRandomly : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // move the object this is attached to around randomly
        transform.position += new Vector3(Random.Range(-.05f, .05f), Random.Range(-.05f, .05f), Random.Range(-.05f, .05f));
        Debug.Log("here");
    }
}
