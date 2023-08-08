using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AsteroidGenerator : MonoBehaviour
{

    // texture for the asteroid
    [SerializeField]
    public Item mineralType;
    public bool isBig = false;

    int AsteroidMinSize = 6; // 12
    int AsteroidMaxSize = 10; // 15
    float AsteroidVariability = 1f;

    //List<Vector3> points = new List<Vector3>();
    //List<Vector3> outsidePoints = new List<Vector3>();
    IDictionary<Vector3, Color> pointColors = new Dictionary<Vector3, Color>();

    //IDictionary<Vector3, int> Vertices = new Dictionary<Vector3, int>();

    ConvexHullCalculator ConvexHullCalc = new ConvexHullCalculator();

    Mesh mesh;

    //bool isVisible;

    //bool firstTime = true;

    

    IDictionary<Vector3, int> pointsSetPositions = new Dictionary<Vector3, int>();
    List<Vector3> points = new List<Vector3>();

    List<List<int>> cubesPointIndecies = new List<List<int>>();

    public void Generate()
    {
        //GenerateAsteroid();
        GenerateAsteroid2();
    }

    void GenerateAsteroid2()
    {
        if (isBig)
        {
            AsteroidMinSize = 10; //18
            AsteroidMaxSize = 12; //20
        }
        // make a vector3 for the dimentions of the asteroid with random values between AsteroidMinSize and AsteroidMaxSize
        float sizeX = Random.Range(AsteroidMinSize, AsteroidMaxSize);
        float sizeY = Random.Range(AsteroidMinSize, AsteroidMaxSize);
        float sizeZ = Random.Range(AsteroidMinSize, AsteroidMaxSize);


        Vector3 dimentions = new Vector3(sizeX, sizeY, sizeZ);

        Debug.Log(dimentions);

        float incrementX = 1.5f;
        float incrementY = 1.5f;
        float incrementZ = 1.5f;

        float inbetweenPointSize = .5f;

        float maxDistance = Vector3.Distance(new Vector3(0, 0, 0), new Vector3(sizeX, sizeY, sizeZ));

        for (float x = -sizeX; x < sizeX; x++)
        {
            float xDist = Mathf.Abs(x - sizeX);
            if (.6f <= xDist / sizeX)
                continue;
            
            for (float y = -sizeY; y < sizeY; y++)
            {
                float yDist = Mathf.Abs(y - sizeY);
                if (.6f <= yDist / sizeY)
                    continue;
                for (float z = -sizeZ; z < sizeZ; z++)
                {
                    float zDist = Mathf.Abs(z - sizeZ);
                    if (.6f <= zDist / sizeZ)
                        continue;
                    //calculate the distance from the center of the asteroid
                    //float distance = Vector3.Distance(new Vector3(0,0,0), new Vector3(x, y, z));

                    //// the further from center, the higher the chances are that the vertex will not be added
                    //if (.6f > distance / maxDistance)
                    //{
                        Vector3 middle = new Vector3(x, y, z);
                        // generate 6 points around the position of the voxel that make a cube
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
                        //float distanceFromCenter = Vector3.Distance(new Vector3(0, 0, 0), point);
                        //if (.6f > distanceFromCenter / maxDistance)
                        if (.8f > Mathf.Abs(point.x) / sizeX && .8f > Mathf.Abs(point.y) / sizeY && .8f > Mathf.Abs(point.z) / sizeZ)
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
                            cubesPointIndecies.Add(newCubePointIndecies);
                    //}
                }
            }
        }
        
        float addRandomness = inbetweenPointSize;

        // add some randomness to each point and give each point a color
        for (int i = 0; i < points.Count; i++)
        {


            points[i] = new Vector3(points[i].x + Random.Range(-addRandomness, addRandomness), points[i].y + Random.Range(-addRandomness, addRandomness), points[i].z + Random.Range(-addRandomness, addRandomness));
            Color color = Random.Range(0, 10) < 9 ? mineralType.getColor() : new Color(.2f, .2f, .2f);
            pointColors.Add(points[i], color);
        }

        List<Vector3> allVerts = new List<Vector3>();
        List<int> allTris = new List<int>();
        List<Vector3> allNormals = new List<Vector3>();

        foreach (List<int> cube in cubesPointIndecies)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            // get just outer points

            List<Vector3> pointInThisCube = new List<Vector3>();
            foreach (int index in cube)
            {
                pointInThisCube.Add(points[index]);
            }

            ConvexHullCalc.GenerateHull(pointInThisCube, true, ref verts, ref tris, ref normals);

            // add the verts and tris to the allVerts and allTris
            foreach (Vector3 vert in verts)
            {
                allVerts.Add(vert);
            }
            foreach (int tri in tris)
            {
                allTris.Add(tri + allVerts.Count - verts.Count);
            }
            foreach (Vector3 normal in normals)
            {
                allNormals.Add(normal);
            }

        }

        Color[] colors = new Color[allVerts.Count];
        //for every vert add a color
        for (int i = 0; i < allVerts.Count; i++)
        {
            colors[i] = pointColors[allVerts[i]];
        }

        // create the mesh
        mesh = new Mesh();
        mesh.vertices = allVerts.ToArray();
        mesh.triangles = allTris.ToArray();
        mesh.normals = allNormals.ToArray();
        mesh.colors = colors;
        // generate uvs
        Vector2[] uvs = new Vector2[allVerts.Count];
        for (int i = 0; i < allVerts.Count; i++)
        {
            uvs[i] = new Vector2(allVerts[i].x, allVerts[i].z);
        }
        mesh.uv = uvs;

        GetComponent<MeshCollider>().sharedMesh = mesh;

        GetComponent<MeshFilter>().mesh = mesh;

    }


    public void MineAsteroid(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine)
    {
        //RemoveCubeClosestToRay(hit.point + (rayDirection * i));   

        //// if the miner or the miners parent has an inventory then add the mineral to the inventory
        //if (miner.GetComponent<Inventory>() != null)
        //{
        //    if (miner.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
        //    {
        //        Debug.Log("Inventory full");
        //    }
        //}
        //else if (miner.transform.parent.GetComponent<Inventory>() != null)
        //{
        //    if (miner.transform.parent.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
        //    {
        //        Debug.Log("Inventory full");
        //    }
        //}

        //if (points.Count > 5)
        //{
        //    RegenerateMesh();
        //}
        //else
        //{
        //    Destroy(this);
        //}
    }


































    //void GenerateAsteroid()
    //{
    //    //generater an empty game object with a mesh and a mesh renderer and a mesh collider for the asteroid

    //    if (isBig)
    //    {
    //        AsteroidMinSize = 18;
    //        AsteroidMaxSize = 20;
    //    }

    //    // make a vector3 for the dimentions of the asteroid with random values between AsteroidMinSize and AsteroidMaxSize
    //    int size = Random.Range(AsteroidMinSize, AsteroidMaxSize);
    //    Vector3 dimentions = new Vector3(size, size, size);
    //    // generate the voxels
    //    byte[,,] Voxels = GenerateNoise(dimentions);

    //    GenerateVertices(dimentions, Voxels);
    //    // generate the mesh
    //    GenerateMesh();

    //    Vector3 center = new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2);
    //    // randomly rotate the asteroid from the center
    //}


    //// do this every time the player breaks a part of the asteroid
    //private void GenerateMesh()
    //{
    //    List<Vector3> verts = new List<Vector3>();
    //    List<int> tris = new List<int>();
    //    List<Vector3> normals = new List<Vector3>();
    //    // get just outer points
    //    ConvexHullCalc.GenerateHull(points, true, ref verts, ref tris, ref normals);

    //    // generate the colors based off of the mineralType
    //    Color[] colors = new Color[verts.Count];
    //    // clear outsidePoints
    //    outsidePoints.Clear();
    //    foreach (Vector3 outsidePoint in verts)
    //    {
    //        if (!outsidePoints.Contains(outsidePoint))
    //        {
    //            outsidePoints.Add(outsidePoint);
    //            if (firstTime)
    //            {
    //                // 90% chance to change to gray color
    //                float chance = Random.Range(0f, 1f);
    //                if (chance > 0.1f)
    //                {
    //                    pointColors[outsidePoint] = new Color(0.2f, .2f, .2f);
    //                }
    //            }
    //        }
    //    }
    //    // for every vert add a color
    //    for (int i = 0; i < verts.Count; i++)
    //    {
    //        colors[i] = pointColors[verts[i]];
    //    }

    //    // create the mesh
    //    mesh = new Mesh();
    //    mesh.vertices = verts.ToArray();
    //    mesh.triangles = tris.ToArray();
    //    mesh.normals = normals.ToArray();
    //    mesh.colors = colors;
    //    // generate uvs
    //    Vector2[] uvs = new Vector2[verts.Count];
    //    for (int i = 0; i < verts.Count; i++)
    //    {
    //        uvs[i] = new Vector2(verts[i].x, verts[i].z);
    //    }
    //    mesh.uv = uvs;

    //    GetComponent<MeshCollider>().sharedMesh = mesh;

    //    GetComponent<MeshFilter>().mesh = mesh;

    //    if (firstTime)
    //    {
    //        firstTime = false;
    //    }
    //}

    //private byte[,,] GenerateNoise(Vector3 dimentions)
    //{
    //    byte[,,] Voxels = new byte[(int)dimentions.x, (int)dimentions.y, (int)dimentions.z];
    //    float randomX = Random.Range(1.2f, 1.5f);
    //    float randomY = Random.Range(1.2f, 1.5f);
    //    float randomZ = Random.Range(1.2f, 1.5f);
    //    Vector3 center = new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2);
    //    float maxDistance = Vector3.Distance(center, new Vector3(dimentions.x / randomX, dimentions.y / randomY, dimentions.z / randomZ));
    //    for (int x = 0; x < dimentions.x; x++)
    //    {
    //        for (int y = 0; y < dimentions.y; y++)
    //        {
    //            for (int z = 0; z < dimentions.z; z++)
    //            {
    //                // calculate the distance from the center of the asteroid
    //                float distance = Vector3.Distance(new Vector3(x, y, z), new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2));

    //                // the further from center, the higher the chances are that the vertex will not be added
    //                if (Random.Range(.6f, 1f) > distance / maxDistance)
    //                    Voxels[x, y, z] = Random.Range(0f, 1f) < AsteroidVariability ? (byte)1 : (byte)0;
    //            }
    //        }
    //    }
    //    return Voxels;
    //}

    //private void GenerateVertices(Vector3 dimentions, byte[,,] Voxels)
    //{
    //    Vector3[] voxelsVertices = new Vector3[8];

    //    Vector3 center = new Vector3(dimentions.x / 2, dimentions.y / 2, dimentions.z / 2);
    //    float randomX = Random.Range(1.2f, 1.5f);
    //    float randomY = Random.Range(1.2f, 1.5f);
    //    float randomZ = Random.Range(1.2f, 1.5f);
    //    float maxDistance = Vector3.Distance(center, new Vector3(dimentions.x / randomX, dimentions.y / randomY, dimentions.z / randomZ));
    //    float rand;
    //    for (int x = 0; x < dimentions.x; x++)
    //    {

    //        for (int y = 0; y < dimentions.y; y++)
    //        {
    //            rand = Random.Range(0f, 1f);
    //            for (int z = 0; z < dimentions.z; z++)
    //            {
    //                if (Voxels[x, y, z] == 1)
    //                {
    //                    // add new Vertices to the list
    //                    voxelsVertices[0] = new Vector3(x - 1, y + 1, z - 1);
    //                    voxelsVertices[1] = new Vector3(x + 1, y + 1, z - 1);
    //                    voxelsVertices[2] = new Vector3(x + 1, y + 1, z + 1);
    //                    voxelsVertices[3] = new Vector3(x - 1, y + 1, z + 1);
    //                    voxelsVertices[4] = new Vector3(x - 1, y - 1, z - 1);
    //                    voxelsVertices[5] = new Vector3(x + 1, y - 1, z - 1);
    //                    voxelsVertices[6] = new Vector3(x + 1, y - 1, z + 1);
    //                    voxelsVertices[7] = new Vector3(x - 1, y - 1, z + 1);

    //                    // for all the new Vertices, if they are not in the Vertices dictionary, add them, else add 1 to the value
    //                    foreach (Vector3 vertex in voxelsVertices)
    //                    {
    //                        // calculate the distance from the center of the asteroid
    //                        float distance = Vector3.Distance(center, vertex);
    //                        // the further from center, the higher the chances are that the vertex will not be added
    //                        if (Random.Range(.3f, 1f) > distance / maxDistance)
    //                        {
    //                            if (!Vertices.ContainsKey(vertex))
    //                            {
    //                                Vertices.Add(vertex, 1);
    //                                Vector3 newPoint = vertex + new Vector3(Random.Range(-.3f, .3f), Random.Range(-.3f, .3f), Random.Range(-.3f, .3f));
    //                                points.Add(newPoint);
    //                                // 90% chance for a point to be the color of the asteroid
    //                                // this will be changed to just the center of the asteroid, the outer layer will only be 10% mineral color
    //                                if (rand < .9f)
    //                                {
    //                                    pointColors[newPoint] = mineralType.getColor();
    //                                }
    //                                else
    //                                {
    //                                    pointColors[newPoint] = new Color(0.2f, .2f, .2f);
    //                                }
    //                            }
    //                            else
    //                            {
    //                                Vertices[vertex] += 1;
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //public void MineAsteroid(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine)
    //{
    //    for (int i = 0; i < amountToMine; i++)
    //    {
    //        RemovePointClosestToRay(hit.point + (rayDirection * i));
    //    }

    //    // if the miner or the miners parent has an inventory then add the mineral to the inventory
    //    if (miner.GetComponent<Inventory>() != null)
    //    {
    //        if (miner.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
    //        {
    //            Debug.Log("Inventory full");
    //        }
    //    }
    //    else if (miner.transform.parent.GetComponent<Inventory>() != null)
    //    {
    //        if (miner.transform.parent.GetComponent<Inventory>().addItem(mineralType, 1f, -1) != null)
    //        {
    //            Debug.Log("Inventory full");
    //        }
    //    }

    //    if (points.Count > 5)
    //    {
    //        GenerateMesh();
    //    }
    //    else
    //    {
    //        Destroy(this);
    //    }
    //}

    //void RemovePointClosestToRay(Vector3 hitPoint)
    //{
    //    float closestDistance = Mathf.Infinity;

    //    // make the closest point 10 away so that they player cant hit really far away asteroids
    //    Vector3 closestPoint = Vector3.zero;
    //    Vector3 asteroidCurrentPosition = transform.position;
    //    // find the closest point to the rays hit
    //    foreach (Vector3 point in outsidePoints)
    //    {
    //        float distance = Vector3.Distance(hitPoint, point + asteroidCurrentPosition);
    //        if (distance < closestDistance)
    //        {
    //            closestDistance = distance;
    //            closestPoint = point;
    //        }
    //    }

    //    // remove the closest point
    //    points.Remove(closestPoint);
    //    outsidePoints.Remove(closestPoint);
    //}
}

