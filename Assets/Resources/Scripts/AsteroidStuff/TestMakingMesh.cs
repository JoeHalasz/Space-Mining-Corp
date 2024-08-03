using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMakingMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // map called uniqueVertices
        Dictionary<Vector3, int> uniqueVertices = new Dictionary<Vector3, int>();
        List<int> indices = new List<int>();
        int x = 0;
        int y = 0;
        int z = 0;
        int spaceBetween = 1;

        Vector3[] newUniqueVertices = new Vector3[8];
        newUniqueVertices[0] = new Vector3( x - spaceBetween/3.0f,y + spaceBetween/3.0f,z + spaceBetween/3.0f ); // top face back left
        newUniqueVertices[1] = new Vector3( x + spaceBetween,y + spaceBetween,z + spaceBetween ); // top face back right
        newUniqueVertices[2] = new Vector3( x - spaceBetween,y + spaceBetween,z - spaceBetween ); // top face front left    
        newUniqueVertices[3] = new Vector3( x + spaceBetween,y + spaceBetween,z - spaceBetween ); // top face front right
        newUniqueVertices[4] = new Vector3( x + spaceBetween,y - spaceBetween,z + spaceBetween ); // bottom face back right
        newUniqueVertices[5] = new Vector3( x - spaceBetween,y - spaceBetween,z + spaceBetween ); // bottom face back left
        newUniqueVertices[6] = new Vector3( x + spaceBetween,y - spaceBetween,z - spaceBetween ); // bottom face front right
        newUniqueVertices[7] = new Vector3( x - spaceBetween,y - spaceBetween,z - spaceBetween ); // bottom face front left

        for (int i = 0; i < 8; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = newUniqueVertices[i];
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        int[] poses = new int[8];
        for (long j = 0; j < 8; j++)
        {
            // if newUniqueVertices[j] isnt in uniqueVertices then add it
            if (!uniqueVertices.ContainsKey(newUniqueVertices[j]))
            {
                uniqueVertices[newUniqueVertices[j]] = uniqueVertices.Count;
            }
            poses[j] = uniqueVertices[newUniqueVertices[j]];
        }

        int[] localIndices = new int[36] {
            0, 1, 2, 2, 1, 3, // top face
            4, 5, 6, 6, 5, 7, // bottom face
            0, 2, 5, 5, 2, 7, // left face
            3, 1, 6, 6, 1, 4, // right face
            2, 3, 7, 7, 3, 6, // front face
            1, 0, 4, 4, 0, 5  // back face
        };

        // if removing 0 - 1 2 5 4 are effected
        // if removing 1 - 0 2 3 6 4 are effected 

        // 0 - 4
        // 1 - 5
        // 2 - 5
        // 3 - 4
        // 4 - 4
        // 5 - 5
        // 6 - 5
        // 7 - 4

        for (long j = 0; j < 36; j++)
        {
            indices.Add(poses[localIndices[j]]);
        }

        // add some randomenss to all the verts
        for (int i = 0; i < newUniqueVertices.Length; i++)
        {
            //newUniqueVertices[i] += new Vector3(Random.Range(-spaceBetween/4.0f, spaceBetween/4.0f), Random.Range(-spaceBetween/4.0f, spaceBetween/4.0f), Random.Range(-spaceBetween/4.0f, spaceBetween/4.0f));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = newUniqueVertices;
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
        GameObject cube = new GameObject();
        MeshFilter meshFilter = cube.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = cube.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
