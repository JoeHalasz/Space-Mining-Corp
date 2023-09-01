using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class AsteroidGenerator : MonoBehaviour
{

    // texture for the asteroid
    [SerializeField]
    public Item mineralType;
    public bool isBig = false;

    float AsteroidMinSize = 3.2f; // 12
    float AsteroidMaxSize = 3.7f; // 15
    float AsteroidVariability = 1f;

    ConvexHullCalculator ConvexHullCalcGlobal = new ConvexHullCalculator();

    List<Vector3> points = new List<Vector3>();
    List<Vector3> outsidePoints = new List<Vector3>();
    IDictionary<Vector3, int> pointColors = new Dictionary<Vector3, int>();

    IDictionary<Vector3, int> Vertices = new Dictionary<Vector3, int>();

    IDictionary<Vector3, List<Vector3>> pointToCubes = new Dictionary<Vector3, List<Vector3>>();

    Mesh mesh;

    //bool isVisible;

    //bool firstTime = true;
    [SerializeField]
    public float increment;

    IDictionary<Vector3, int> pointsSetPositions = new Dictionary<Vector3, int>();

    List<List<int>> cubesPointIndecies = new List<List<int>>();

    public void Generate()
    {
        //GenerateAsteroid();
        GenerateAsteroid();
    }

    void GenerateAsteroid()
    {
        // time it
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        if (isBig)
        {
            //AsteroidMinSize = 4f; //18
            //AsteroidMaxSize = 4.5f; //20
        }
        // make a vector3 for the dimentions of the asteroid with random values between AsteroidMinSize and AsteroidMaxSize
        float size = Random.Range(AsteroidMinSize, AsteroidMaxSize);
        Vector3 dimentions = new Vector3(size, size, size);

        float inbetweenPointSize = increment/2;

        float maxDistance = Vector3.Distance(new Vector3(0, 0, 0), new Vector3(size, size, size));

        for (float x = -size; x < size; x += increment)
        {
            for (float y = -size; y < size; y += increment)
            {
                for (float z = -size; z < size; z += increment)
                {

                    //calculate the distance from the center of the asteroid
                    float distance = Vector3.Distance(new Vector3(0,0,0), new Vector3(x, y, z));

                    // the further from center, the higher the chances are that the vertex will not be added
                    if (.75f > distance / maxDistance)
                    {
                        Vector3 middle = new Vector3(x, y, z);
                        // generate 8 points around the position of the voxel that make a cube
                        Vector3[] pointsAround = new Vector3[8];
                        pointsAround[0] = new Vector3(x + inbetweenPointSize, y + inbetweenPointSize, z + inbetweenPointSize);
                        pointsAround[1] = new Vector3(x + inbetweenPointSize, y + inbetweenPointSize, z - inbetweenPointSize);
                        pointsAround[2] = new Vector3(x + inbetweenPointSize, y - inbetweenPointSize, z + inbetweenPointSize);
                        pointsAround[3] = new Vector3(x + inbetweenPointSize, y - inbetweenPointSize, z - inbetweenPointSize);
                        pointsAround[4] = new Vector3(x - inbetweenPointSize, y + inbetweenPointSize, z + inbetweenPointSize);
                        pointsAround[5] = new Vector3(x - inbetweenPointSize, y + inbetweenPointSize, z - inbetweenPointSize);
                        pointsAround[6] = new Vector3(x - inbetweenPointSize, y - inbetweenPointSize, z + inbetweenPointSize);
                        pointsAround[7] = new Vector3(x - inbetweenPointSize, y - inbetweenPointSize, z - inbetweenPointSize);

                        List<int> newCubePointIndecies = new List<int>();
                        // foreach point if the point isnt in pointsSet add it to pointsSet
                        foreach (Vector3 point in pointsAround)
                        {
                            // get the distance from the center of the asteroid
                            float distanceFromCenter = Vector3.Distance(new Vector3(0, 0, 0), point);
                            if (.75f > distanceFromCenter / maxDistance)
                            {
                                if (!pointsSetPositions.ContainsKey(point))
                                {
                                    pointsSetPositions.Add(point, points.Count);
                                    points.Add(point);
                                }
                                newCubePointIndecies.Add(pointsSetPositions[point]);
                            }
                        }
                        if (newCubePointIndecies.Count >= 5)
                        {
                            cubesPointIndecies.Add(newCubePointIndecies);
                        }
                    }
                }
            }
        }

        // stop the timer
        stopwatch.Stop();
        //Debug.Log("part1 took: " + stopwatch.Elapsed.ToString());

        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        float addRandomness = inbetweenPointSize;

        // add some randomness to each point and give each point a color
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector3(points[i].x + Random.Range(-addRandomness, addRandomness), points[i].y + Random.Range(-addRandomness, addRandomness), points[i].z + Random.Range(-addRandomness, addRandomness));
        }

        foreach (List<int> cube in cubesPointIndecies)
        {
            int mineralGroup = Random.Range(0, 10) < 8 ? 0:1;

            // put the point into pointToCubes
            foreach (int index in cube)
            {
                // add the pointColor if its not already in there
                if (!pointColors.ContainsKey(points[index]))
                {
                    pointColors.Add(points[index], mineralGroup);
                }

                if (!pointToCubes.ContainsKey(points[index]))
                {
                    pointToCubes.Add(points[index], new List<Vector3>());
                }
                pointToCubes[points[index]].Add(points[index]);
            }

        }
        stopwatch.Stop();
        //Debug.Log("part2 took: " + stopwatch.Elapsed.ToString());

        // spawn a thread to generate the mesh
        GenerateMesh();

    }

    List<Vector3> allVerts;
    List<int> allTris;
    List<Vector3> allNormals;

    void GenerateVertsTrisAndNormalsForList(List<List<int>> cubes)
    {
        ConvexHullCalculator ConvexHullCalc = new ConvexHullCalculator();
        foreach (List<int> cube in cubes){
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            List<Vector3> pointInThisCube = new List<Vector3>();
            foreach (int index in cube)
            {
                pointInThisCube.Add(points[index]);
            }
            ConvexHullCalc.GenerateHull(pointInThisCube, true, ref verts, ref tris, ref normals);
            // add the verts and tris to the allVerts and allTris
            AddToArrays(verts, tris, normals);
        }
    }

    void AddToArrays(List<Vector3> verts, List<int> tris, List<Vector3> normals)
    {
        // lock allVerts so its threadsafe
        lock (allVerts)
        {
            lock (allTris)
            {
                lock (allNormals)
                {
                    allVerts.AddRange(verts);
                    foreach (int tri in tris)
                    {
                        allTris.Add(tri + allVerts.Count - verts.Count);
                    }
                    allNormals.AddRange(normals);
                }
            }
        }
    }

    void GenerateMesh()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        allVerts = new List<Vector3>();
        allTris = new List<int>();
        allNormals = new List<Vector3>();

        GenerateVertsTrisAndNormalsForList(cubesPointIndecies);
        stopwatch.Stop();
        //Debug.Log("part3 without threads took: " + stopwatch.Elapsed.ToString());
        

        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Color[] colors = new Color[allVerts.Count];

        // create the mesh
        mesh = new Mesh();
        mesh.vertices = allVerts.ToArray();
        //mesh.triangles = allTris.ToArray();

        // create a submesh for the ore material
        mesh.subMeshCount = 2;
        // add 10% of the tris to the ore submesh
        
        // make 2 vectors for the tris groups
        List<int> oreTris = new List<int>();
        List<int> otherTris = new List<int>();

        for (int i = 0; i < allTris.Count; i+=3)
        {
            // if the point is in the ore group
            if (pointColors[allVerts[allTris[i]]] == 1)
            {
                // add the tris to the ore group
                oreTris.Add(allTris[i]);
                oreTris.Add(allTris[i+1]);
                oreTris.Add(allTris[i+2]);
            }
            else
            {
                // add the tris to the other group
                otherTris.Add(allTris[i]);
                otherTris.Add(allTris[i+1]);
                otherTris.Add(allTris[i+2]);
            }
        }

        // add the ore tris to the submesh
        mesh.SetTriangles(oreTris.ToArray(), 0);
        // set the rest of the tris to the other submesh
        mesh.SetTriangles(otherTris.ToArray(), 1);

        //int length = (int)((allTris.Count / 3) / 10f);
        //int[] oreTris = new int[length*3];
        //for (int i = 2; i < length; i+=3)
        //{
        //    oreTris[i-2] = allTris[i-2];
        //    oreTris[i-1] = allTris[i-1];
        //    oreTris[i] = allTris[i];
        //}
        //// add the ore tris to the submesh
        //mesh.SetTriangles(oreTris, 0);
        //// set the rest of the tris to the other submesh
        //int[] otherTris = new int[allTris.Count - oreTris.Length];
        //for (int i = 0; i < otherTris.Length; i++)
        //{
        //    otherTris[i] = allTris[i + oreTris.Length];
        //}
        //mesh.SetTriangles(otherTris, 1);

        // add normals
        mesh.normals = allNormals.ToArray();

        // generate uvs
        Vector2[] uvs = new Vector2[allVerts.Count];
        for (int i = 0; i < allVerts.Count; i++)
        {
            uvs[i] = new Vector2(allVerts[i].x, allVerts[i].z);
        }
        mesh.uv = uvs;

        GetComponent<MeshCollider>().sharedMesh = mesh;

        GetComponent<MeshFilter>().mesh = mesh;
        stopwatch.Stop();
        //Debug.Log("part4 took: " + stopwatch.Elapsed.ToString());
    }


    public void MineAsteroid(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine)
    {
        RemoveCubesClosestToRay(hit.point + rayDirection);

        // if the miner or the miners parent has an inventory then add the mineral to the inventory
        if (miner.GetComponent<Inventory>() != null)
        {
            if (miner.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
            {
                Debug.Log("Inventory full");
            }
        }
        else if (miner.transform.parent.GetComponent<Inventory>() != null)
        {
            if (miner.transform.parent.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
            {
                Debug.Log("Inventory full");
            }
        }

        if (cubesPointIndecies.Count > 1)
        {
            GenerateMesh();
        }
        else
        {
            Destroy(this);
        }
    }

    void RemoveCubesClosestToRay(Vector3 hitPoint)
    {
        float closestDistance = Mathf.Infinity;

        // make the closest point 10 away so that they player cant hit really far away asteroids
        Vector3 closestPoint = Vector3.zero;
        Vector3 asteroidCurrentPosition = transform.position;
        // find the closest point to the rays hit
        
        // loop through all cubes
        foreach (List<int> cube in cubesPointIndecies)
        {
            // loop through all points in the cube
            foreach (int index in cube)
            {
                Vector3 point = points[index];
                float distance = Vector3.Distance(hitPoint, point + asteroidCurrentPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }
        }

        List<List<int>> newCubesPointIndecies = new List<List<int>>();

        // loop through cubesPointIndecies and if the closest point is in it then dont add it to the new list
        foreach (List<int> cube in cubesPointIndecies)
        {
            if (!cube.Contains(points.IndexOf(closestPoint)))
            {
                newCubesPointIndecies.Add(cube);
            }
        }

        cubesPointIndecies = newCubesPointIndecies;


        // remove any cubes if they are not attached to any other cube
        IDictionary<Vector3, int> pointCopies = new Dictionary<Vector3, int>();

        // loop through all the cubes
        foreach (List<int> cube in cubesPointIndecies)
        {
            // loop through all the points in the cube
            foreach (int index in cube)
            {
                // if the point is not in pointCopies then add it
                if (!pointCopies.ContainsKey(points[index]))
                {
                    pointCopies.Add(points[index], 1);
                }
                // if the point is in pointCopies then add one to the value
                else
                {
                    pointCopies[points[index]]++;
                }
            }
        }

        newCubesPointIndecies = new List<List<int>>();
        // loop through all the cubes
        foreach (List<int> cube in cubesPointIndecies)
        {
            // if all of the points in the cube are 1 then delete the cube
            bool deleteCube = true;
            foreach (int index in cube)
            {
                if (pointCopies[points[index]] != 1)
                {
                    deleteCube = false;
                }
            }
            if (!deleteCube)
            {
                newCubesPointIndecies.Add(cube);
            }
        }

        cubesPointIndecies = newCubesPointIndecies;


    }




















    bool firstTime = true;




    void GenerateAsteroid2()
    {
        //generater an empty game object with a mesh and a mesh renderer and a mesh collider for the asteroid

        if (isBig)
        {
            AsteroidMinSize = 18;
            AsteroidMaxSize = 20;
        }
        else
        {
            AsteroidMinSize = 12;
            AsteroidMaxSize = 15;
        }

        // make a vector3 for the dimentions of the asteroid with random values between AsteroidMinSize and AsteroidMaxSize
        int size = Random.Range((int)AsteroidMinSize, (int)AsteroidMaxSize);
        Vector3 dimentions = new Vector3(size, size, size);
        // generate the voxels
        byte[,,] Voxels = GenerateNoise2(dimentions);

        GenerateVertices2(dimentions, Voxels);
        // generate the mesh
        GenerateMesh2();

        Vector3 center = new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2);
        // randomly rotate the asteroid from the center
    }


    // do this every time the player breaks a part of the asteroid
    private void GenerateMesh2()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        // get just outer points
        ConvexHullCalcGlobal.GenerateHull(points, true, ref verts, ref tris, ref normals);

        // generate the colors based off of the mineralType
        Color[] colors = new Color[verts.Count];
        // clear outsidePoints
        outsidePoints.Clear();
        foreach (Vector3 outsidePoint in verts)
        {
            if (!outsidePoints.Contains(outsidePoint))
            {
                outsidePoints.Add(outsidePoint);
                if (firstTime)
                {
                    // 90% chance to change to gray color
                    float chance = Random.Range(0f, 1f);
                    if (chance > 0.1f)
                    {
                        // pointColors[outsidePoint] = new Color(0.2f, .2f, .2f);
                    }
                }
            }
        }
        // for every vert add a color
        for (int i = 0; i < verts.Count; i++)
        {
            colors[i] = new Color(1f,0f,0f,1f);
        }

        // create the mesh
        mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.colors = colors;
        // generate uvs
        Vector2[] uvs = new Vector2[verts.Count];
        for (int i = 0; i < verts.Count; i++)
        {
            uvs[i] = new Vector2(verts[i].x, verts[i].z);
        }
        mesh.uv = uvs;

        GetComponent<MeshCollider>().sharedMesh = mesh;

        GetComponent<MeshFilter>().mesh = mesh;

        if (firstTime)
        {
            firstTime = false;
        }
    }

    private byte[,,] GenerateNoise2(Vector3 dimentions)
    {
        byte[,,] Voxels = new byte[(int)dimentions.x, (int)dimentions.y, (int)dimentions.z];
        float randomX = Random.Range(1.2f, 1.5f);
        float randomY = Random.Range(1.2f, 1.5f);
        float randomZ = Random.Range(1.2f, 1.5f);
        Vector3 center = new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2);
        float maxDistance = Vector3.Distance(center, new Vector3(dimentions.x / randomX, dimentions.y / randomY, dimentions.z / randomZ));
        for (int x = 0; x < dimentions.x; x++)
        {
            for (int y = 0; y < dimentions.y; y++)
            {
                for (int z = 0; z < dimentions.z; z++)
                {
                    // calculate the distance from the center of the asteroid
                    float distance = Vector3.Distance(new Vector3(x, y, z), new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2));

                    // the further from center, the higher the chances are that the vertex will not be added
                    if (Random.Range(.6f, 1f) > distance / maxDistance)
                        Voxels[x, y, z] = Random.Range(0f, 1f) < AsteroidVariability ? (byte)1 : (byte)0;
                }
            }
        }
        return Voxels;
    }

    private void GenerateVertices2(Vector3 dimentions, byte[,,] Voxels)
    {
        Vector3[] voxelsVertices = new Vector3[8];

        Vector3 center = new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2);
        float randomX = Random.Range(1.2f, 1.5f);
        float randomY = Random.Range(1.2f, 1.5f);
        float randomZ = Random.Range(1.2f, 1.5f);
        float maxDistance = Vector3.Distance(center, new Vector3(dimentions.x / randomX, dimentions.y / randomY, dimentions.z / randomZ));
        float rand;
        for (int x = 0; x < dimentions.x; x++)
        {

            for (int y = 0; y < dimentions.y; y++)
            {
                rand = Random.Range(0f, 1f);
                for (int z = 0; z < dimentions.z; z++)
                {
                    if (Voxels[x, y, z] == 1)
                    {
                        // add new Vertices to the list
                        voxelsVertices[0] = new Vector3(x - 1, y + 1, z - 1);
                        voxelsVertices[1] = new Vector3(x + 1, y + 1, z - 1);
                        voxelsVertices[2] = new Vector3(x + 1, y + 1, z + 1);
                        voxelsVertices[3] = new Vector3(x - 1, y + 1, z + 1);
                        voxelsVertices[4] = new Vector3(x - 1, y - 1, z - 1);
                        voxelsVertices[5] = new Vector3(x + 1, y - 1, z - 1);
                        voxelsVertices[6] = new Vector3(x + 1, y - 1, z + 1);
                        voxelsVertices[7] = new Vector3(x - 1, y - 1, z + 1);

                        // for all the new Vertices, if they are not in the Vertices dictionary, add them, else add 1 to the value
                        foreach (Vector3 vertex in voxelsVertices)
                        {
                            // calculate the distance from the center of the asteroid
                            float distance = Vector3.Distance(center, vertex);
                            // the further from center, the higher the chances are that the vertex will not be added
                            if (Random.Range(.3f, 1f) > distance / maxDistance)
                            {
                                if (!Vertices.ContainsKey(vertex))
                                {
                                    Vertices.Add(vertex, 1);
                                    Vector3 newPoint = vertex + new Vector3(Random.Range(-.3f, .3f), Random.Range(-.3f, .3f), Random.Range(-.3f, .3f));
                                    points.Add(newPoint);
                                    // 90% chance for a point to be the color of the asteroid
                                    // this will be changed to just the center of the asteroid, the outer layer will only be 10% mineral color
                                    //if (rand < .9f)
                                    //{
                                    //    pointColors[newPoint] = mineralType.getColor();
                                    //}
                                    //else
                                    //{
                                    //    pointColors[newPoint] = new Color(0.2f, .2f, .2f);
                                    //}
                                }
                                else
                                {
                                    Vertices[vertex] += 1;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void MineAsteroid2(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine)
    {
        for (int i = 0; i < amountToMine; i++)
        {
            RemovePointClosestToRay2(hit.point + (rayDirection * i));
        }

        // if the miner or the miners parent has an inventory then add the mineral to the inventory
        if (miner.GetComponent<Inventory>() != null)
        {
            if (miner.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
            {
                Debug.Log("Inventory full");
            }
        }
        else if (miner.transform.parent.GetComponent<Inventory>() != null)
        {
            if (miner.transform.parent.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
            {
                Debug.Log("Inventory full");
            }
        }

        if (points.Count > 5)
        {
            GenerateMesh();
        }
        else
        {
            Destroy(this);
        }
    }

    void RemovePointClosestToRay2(Vector3 hitPoint)
    {
        float closestDistance = Mathf.Infinity;

        // make the closest point 10 away so that they player cant hit really far away asteroids
        Vector3 closestPoint = Vector3.zero;
        Vector3 asteroidCurrentPosition = transform.position;
        // find the closest point to the rays hit
        foreach (Vector3 point in outsidePoints)
        {
            float distance = Vector3.Distance(hitPoint, point + asteroidCurrentPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        // remove the closest point
        points.Remove(closestPoint);
        outsidePoints.Remove(closestPoint);
    }


    //void RemovePointClosestToRay3(Vector3 hitPoint)
    //{
    //    float closestDistance = Mathf.Infinity;

    //    // make the closest point 10 away so that they player cant hit really far away asteroids
    //    Vector3 closestPoint = Vector3.zero;
    //    Vector3 asteroidCurrentPosition = transform.position;
    //    // find the closest point to the rays hit
    //    int spot1 = 0;
    //    int i = 0;
    //    foreach (Vector3 point in outsidePoints)
    //    {
    //        float distance = Vector3.Distance(hitPoint, point + asteroidCurrentPosition);
    //        if (distance < closestDistance)
    //        {
    //            closestDistance = distance;
    //            closestPoint = point;
    //            spot1 = i;
    //        }
    //        i++;
    //    }

    //    // remove the closest point
    //    points.Remove(closestPoint);
    //    outsidePoints.Remove(closestPoint);

    //    if (points.Count <= 5)
    //    {
    //        return;
    //    }

    //    // find the closest point to the point that was removed
    //    closestDistance = Mathf.Infinity;
    //    Vector3 closestPoint2 = Vector3.zero;
    //    i = 0;
    //    int spot2 = 0;
    //    foreach (Vector3 point in points)
    //    {

    //        float distance = Vector3.Distance(closestPoint, point);
    //        if (distance < closestDistance)
    //        {
    //            // if the point isnt in outside points
    //            if (!outsidePoints.Contains(point))
    //            {
    //                closestDistance = distance;
    //                closestPoint2 = point;
    //                spot2 = i;
    //            }
    //        }
    //        i++;
    //    }

    //    // add it to outside points
    //    outsidePoints.Add(closestPoint2);

    //    // replace all spot1s in verts with spot2
    //    for (int j = 0; j < verts.Count; j++)
    //    {
    //        if (verts[j] == closestPoint)
    //        {
    //            verts[j] = closestPoint2;
    //        }
    //    }

    //    // recalculate uvs
    //    Vector2[] uvs = new Vector2[verts.Count];
    //    for (int j = 0; j < verts.Count; j++)
    //    {
    //        uvs[j] = new Vector2(verts[j].x, verts[j].z);
    //    }

    //    // replace the meshes verts and uvs


    //    GetComponent<MeshFilter>().mesh.vertices = verts.ToArray();
    //    GetComponent<MeshFilter>().mesh.uv = uvs;

    //    GetComponent<MeshFilter>().mesh.RecalculateNormals();
    //    GetComponent<MeshFilter>().mesh.RecalculateBounds();


    //    //// replace all spot1s in tris with spot2
    //    //for (int j = 0; j < tris.Length; j++)
    //    //{
    //    //    if (tris[j] == spot1)
    //    //    {
    //    //        tris[j] = spot2;
    //    //    }
    //    //}

    //    //// replace all spot1s in each normals vector3 with spot2
    //    //for (int j = 0; j < normals.Length; j++)
    //    //{
    //    //    for (int k = 0; k < normals[j].Length; k++)
    //    //    {
    //    //        if (normals[j][k] == spot1)
    //    //        {
    //    //            normals[j][k] = spot2;
    //    //        }
    //    //    }
    //    // }





    //}



}

