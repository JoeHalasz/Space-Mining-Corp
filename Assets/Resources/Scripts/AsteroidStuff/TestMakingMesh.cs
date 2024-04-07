using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMakingMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // make 8 points for the cube
        Vector3[] vertices = new Vector3[8];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(1, 0, 0);
        vertices[2] = new Vector3(1, 1, 0);
        vertices[3] = new Vector3(0, 1, 0);
        vertices[4] = new Vector3(0, 0, 1);
        vertices[5] = new Vector3(1, 0, 1);
        vertices[6] = new Vector3(1, 1, 1);
        vertices[7] = new Vector3(0, 1, 1);

        // show them all as small spheres
        for (int i = 0; i < 8; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = vertices[i];
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        // make 6 face for the cube
        int[] triangles = new int[36];
        
        // left face
        triangles[12] = 4;
        triangles[13] = 7;
        triangles[14] = 3;
        triangles[15] = 4;
        triangles[16] = 3;
        triangles[17] = 0;
        // right face
        triangles[18] = 1;
        triangles[19] = 2;
        triangles[20] = 6;
        triangles[21] = 1;
        triangles[22] = 6;
        triangles[23] = 5;
        // top face
        triangles[24] = 2;
        triangles[25] = 3;
        triangles[26] = 7;
        triangles[27] = 2;
        triangles[28] = 7;
        triangles[29] = 6;
        // bottom face
        triangles[30] = 0;
        triangles[31] = 1;
        triangles[32] = 5;
        triangles[33] = 0;
        triangles[34] = 5;
        triangles[35] = 4;
        // front face
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 2;
        // back face
        triangles[6] = 4;
        triangles[7] = 5;
        triangles[8] = 6;
        triangles[9] = 4;
        triangles[10] = 6;
        triangles[11] = 7;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
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
