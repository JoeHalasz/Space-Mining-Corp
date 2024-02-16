using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AsteroidGenerator : MonoBehaviour
{
    // texture for the asteroid
    [SerializeField]
    public Item mineralType;
    public Item stone;
    public bool isBig = false;
    float AsteroidMinSize = 3.2f; // 12
    float AsteroidMaxSize = 3.7f; // 15
    ConvexHullCalculator ConvexHullCalcGlobal = new ConvexHullCalculator();
    public List<Vector3> points = new List<Vector3>();
    public List<List<int>> oreCubes = new List<List<int>>();
    public Mesh mesh;
    public Mesh originalMesh;
    IDictionary<Vector3, int> pointsSetPositions = new Dictionary<Vector3, int>();
    public List<List<int>> outsideCubePointIndecies = new List<List<int>>();
    public List<List<int>> cubesPointIndecies = new List<List<int>>();
    public List<List<int>> originalCubesPointIndecies = new List<List<int>>();
    public List<Vector3> allVerts;
    public List<int> allTris;
    public List<Vector3> allNormals;
    public AsteroidSpawnManager asteroidSpawnManager;

    Vector3 originalPosition;

    bool edited = false;
    bool newMeshCreated = false;

    void Start()
    {
        // set the layer to 8 (asteroid)
        gameObject.layer = 8;
        originalPosition = transform.localPosition;
    }

    List<List<T>> copyListOfLists<T>(List<List<T>> lst)
    {
        return lst.Select(innerList => new List<T>(innerList)).ToList();
    }

    // function takes in all the above variables and sets them in this script
    public bool copyAll(ref AsteroidGenerator other, AsteroidSpawnManager _asteroidSpawnManager)
    {
        mineralType =               other.mineralType;
        stone =                     other.stone;
        points =                    other.points;
        oreCubes =                  other.oreCubes;
        originalMesh =              other.originalMesh;
        outsideCubePointIndecies =  other.outsideCubePointIndecies;
        cubesPointIndecies =        copyListOfLists(other.originalCubesPointIndecies);
        originalCubesPointIndecies= other.originalCubesPointIndecies;
        allVerts =                  new List<Vector3>(other.allVerts); // need to copy these so that we dont have a ref problem
        allTris =                   new List<int>(other.allTris);
        allNormals =                new List<Vector3>(other.allNormals);
        asteroidSpawnManager =      _asteroidSpawnManager;
        if (originalMesh == null){
            #if UNITY_EDITOR
                Debug.Log("Failed to generate asteroid");
            #endif
            return false;
        }
        return true;
    }

    public void ApplyMesh()
    {
        if (mesh != null)
        {
            GetComponent<MeshCollider>().sharedMesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            mesh.UploadMeshData(false);
        }
        else
        {
            GetComponent<MeshCollider>().sharedMesh = originalMesh;
            GetComponent<MeshFilter>().sharedMesh = originalMesh;
            originalMesh.UploadMeshData(false);
        }
    }

    public void setOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
    }

    public void Generate()
    {
        GenerateAsteroid();
    }

    public float size;

    void GenerateAsteroid()
    {
        if (isBig)
        {
            AsteroidMinSize = 4;
            AsteroidMaxSize = 5;
        }
        
        // make a vector3 for the dimentions of the asteroid with random values between AsteroidMinSize and AsteroidMaxSize
        float originalSize = (AsteroidMinSize + AsteroidMaxSize)/2f;
        size = Random.Range(AsteroidMinSize, AsteroidMaxSize);
        float inbetweenPointSize = (size/10f)*1.5f;
        float increment = size/originalSize;

        float maxDistance = Vector3.Distance(new Vector3(0, 0, 0), new Vector3(size, size, size));

        Dictionary<List<Vector3>, int> cubeDistances = new Dictionary<List<Vector3>, int>();
        
        float c;
        for (c = -size; c <= size; c += increment){}
        Vector3 asteroidCenter = new Vector3(-size + c, -size + c, -size + c);

        int count = 0;
        for (float x = -size-increment; x <= size;)
        {
            x += increment;
            for (float y = -size-increment; y <= size;)
            {
                y += increment;
                for (float z = -size-increment; z <= size;)
                {
                    z += increment;
                    //calculate the distance from the center of the asteroid
                    float distance = Vector3.Distance(asteroidCenter, new Vector3(x, y, z));

                    // the further from center, the higher the chances are that the vertex will not be added
                    if (.75f >= distance / maxDistance)
                    {
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
                            float distanceFromCenter = Vector3.Distance(asteroidCenter, point);
                            if (.75f >= distanceFromCenter / maxDistance)
                            {
                                // make a point using the int of all the points
                                Vector3 pointInt = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z));
                                if (!pointsSetPositions.ContainsKey(pointInt))
                                {
                                    pointsSetPositions.Add(pointInt, points.Count);
                                    points.Add(point);
                                    count += 1;
                                }
                                newCubePointIndecies.Add(pointsSetPositions[pointInt]);
                            }
                        }
                        if (newCubePointIndecies.Count >= 5)
                        {
                            cubesPointIndecies.Add(newCubePointIndecies);
                            originalCubesPointIndecies.Add(newCubePointIndecies);
                        }
                    }
                }
            }
        }
        Dictionary<int, List<List<int>>> cubePointIndeciesDistances = new Dictionary<int, List<List<int>>>();

        // calculate cubePointIndeciesDistances
        foreach (List<int> cube in cubesPointIndecies)
        {
            float minDist = Mathf.Infinity;
            foreach (int index in cube)
            {
                if (Vector3.Distance(asteroidCenter, points[index]) < minDist)
                {
                    minDist = Vector3.Distance(asteroidCenter, points[index]);
                }
            }
            int distance = (int)minDist;
            if (!cubePointIndeciesDistances.ContainsKey(distance))
            {
                cubePointIndeciesDistances.Add(distance, new List<List<int>>());
            }
            cubePointIndeciesDistances[distance].Add(cube);
        }
        
        // find the 2 furthest layers
        int furthest = 0;
        int furthest2 = 0;
        foreach (KeyValuePair<int, List<List<int>>> kvp in cubePointIndeciesDistances)
        {
            if (cubePointIndeciesDistances[kvp.Key].Count <= 32)
            {
                continue;
            }
            if (kvp.Key > furthest)
            {
                furthest2 = furthest;
                furthest = kvp.Key;
            }
            else if (kvp.Key > furthest2)
            {
                furthest2 = kvp.Key;
            }
        }
        // add the points from the furthest 2 layers to outsideCubePointIndecies and any layers outside of furshest2
        foreach (KeyValuePair<int, List<List<int>>> kvp in cubePointIndeciesDistances)
        {
            if (kvp.Key == furthest || kvp.Key >= furthest2)
            {
                foreach (List<int> cube in kvp.Value)
                {
                    outsideCubePointIndecies.Add(cube);
                }
            }
        }

        float addRandomness = increment/3f;
        // add some randomness to each point and give each point a color
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector3(points[i].x + Random.Range(-addRandomness, addRandomness), points[i].y + Random.Range(-addRandomness, addRandomness), points[i].z + Random.Range(-addRandomness, addRandomness));
        }

        foreach (List<int> cube in cubesPointIndecies)
        {
            int mineralGroup = Random.Range(0, 10) < 8 ? 0:1;
            if (mineralGroup == 1)
            {
                oreCubes.Add(cube);
            }
        }
        GenerateMesh();
    }

    // pass oreTris and otherTris by ref
    void GenerateVertsTrisAndNormalsForList(List<List<int>> cubes, ref List<int> oreTris, ref List<int> otherTris)
    {
        foreach (List<int> cube in cubes){
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector3> normals = new List<Vector3>();

            List<Vector3> pointInThisCube = new List<Vector3>();
            foreach (int index in cube)
            {
                pointInThisCube.Add(points[index]);
            }
            ConvexHullCalcGlobal.GenerateHull(pointInThisCube, true, ref verts, ref tris, ref normals);
            // add the verts and tris to the allVerts and allTris
            AddToArrays(verts, tris, normals);
            
            // check if the cube is in oreCubes
            
            bool isOre = false;
            // loop through oreCubes and if all the values in cube are the same as one of the cubes in oreCubes then set isOre to true
            foreach (List<int> oreCube in oreCubes)
            {
                if (cube.All(oreCube.Contains) && oreCube.All(cube.Contains))
                {
                    isOre = true;
                    break;
                }
            }
            if (isOre)
            {
                foreach (int tri in tris)
                {
                    oreTris.Add(tri + allVerts.Count - verts.Count);
                }
            }
            else
            {
                foreach (int tri in tris)
                {
                    otherTris.Add(tri + allVerts.Count - verts.Count);
                }
            }
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

    public void unloadAsteroid()
    {
        if (newMeshCreated)
        {
            #if UNITY_EDITOR
                Debug.Log("Freeing mesh memory");
            #endif
            Destroy(mesh);
        }
    }

    void GenerateMesh()
    {
        if (!newMeshCreated)
        {
            mesh = new Mesh();
            newMeshCreated = true;
        }
        else
        {
            Destroy(mesh);
            mesh = new Mesh();
        }
        allVerts = new List<Vector3>();
        allTris = new List<int>();
        allNormals = new List<Vector3>();

        // make 2 vectors for the tris groups
        List<int> oreTris = new List<int>();
        List<int> otherTris = new List<int>();

        if (!edited)
        {
            GenerateVertsTrisAndNormalsForList(outsideCubePointIndecies,  ref oreTris, ref otherTris);
        }
        else
        {
            GenerateVertsTrisAndNormalsForList(cubesPointIndecies, ref oreTris, ref otherTris);
        }

        mesh.vertices = allVerts.ToArray();

        // create a submesh for the ore material
        mesh.subMeshCount = 2;
        
        // add the ore tris to the submesh
        mesh.SetTriangles(oreTris.ToArray(), 0);
        // set the rest of the tris to the other submesh
        mesh.SetTriangles(otherTris.ToArray(), 1);
        // add normals
        mesh.normals = allNormals.ToArray();

        // call calulate UVs using allVerts as the first parameter
        mesh.uv = CalculateUVs(allVerts.ToArray(), 1);
        
        // destroy the old mesh
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshFilter>().sharedMesh = mesh;
        mesh.UploadMeshData(false);
        if (originalMesh == null)
        {
            originalMesh = Instantiate(mesh);
        }
    }

    private enum Facing { Up, Forward, Right };

    public Vector2[] CalculateUVs(Vector3[] v/*vertices*/, float scale)
    {
        var uvs = new Vector2[v.Length];

        for (int i = 0; i < uvs.Length; i += 3)
        {
            int i0 = i;
            int i1 = i + 1;
            int i2 = i + 2;

            Vector3 v0 = v[i0];
            Vector3 v1 = v[i1];
            Vector3 v2 = v[i2];

            Vector3 side1 = v1 - v0;
            Vector3 side2 = v2 - v0;
            var direction = Vector3.Cross(side1, side2);
            var facing = FacingDirection(direction);
            switch (facing)
            {
                case Facing.Forward:
                    uvs[i0] = ScaledUV(v0.x, v0.y, scale);
                    uvs[i1] = ScaledUV(v1.x, v1.y, scale);
                    uvs[i2] = ScaledUV(v2.x, v2.y, scale);
                    break;
                case Facing.Up:
                    uvs[i0] = ScaledUV(v0.x, v0.z, scale);
                    uvs[i1] = ScaledUV(v1.x, v1.z, scale);
                    uvs[i2] = ScaledUV(v2.x, v2.z, scale);
                    break;
                case Facing.Right:
                    uvs[i0] = ScaledUV(v0.y, v0.z, scale);
                    uvs[i1] = ScaledUV(v1.y, v1.z, scale);
                    uvs[i2] = ScaledUV(v2.y, v2.z, scale);
                    break;
            }
        }
        return uvs;
    }

    private bool FacesThisWay(Vector3 v, Vector3 dir, Facing p, ref float maxDot, ref Facing ret)
    {
        float t = Vector3.Dot(v, dir);
        if (t > maxDot)
        {
            ret = p;
            maxDot = t;
            return true;
        }
        return false;
    }

    private Facing FacingDirection(Vector3 v)
    {
        var ret = Facing.Up;
        float maxDot = Mathf.NegativeInfinity;

        if (!FacesThisWay(v, Vector3.right, Facing.Right, ref maxDot, ref ret))
            FacesThisWay(v, Vector3.left, Facing.Right, ref maxDot, ref ret);

        if (!FacesThisWay(v, Vector3.forward, Facing.Forward, ref maxDot, ref ret))
            FacesThisWay(v, Vector3.back, Facing.Forward, ref maxDot, ref ret);

        if (!FacesThisWay(v, Vector3.up, Facing.Up, ref maxDot, ref ret))
            FacesThisWay(v, Vector3.down, Facing.Up, ref maxDot, ref ret);

        return ret;
    }

    private Vector2 ScaledUV(float uv1, float uv2, float scale)
    {
        return new Vector2(uv1 / scale, uv2 / scale);
    }


    public void MineAsteroid(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine)
    {
        if (!edited)
        {
            cubesPointIndecies = copyListOfLists(cubesPointIndecies);
        }
        edited = true;
        List<int> removedCube = RemoveCubesClosestToRay(ray, hit);
        // check if removedCube is in oreCubes
        Item itemMined = stone;
        bool isOre = false;
        foreach (List<int> cube in oreCubes)
        {
            if (cube.All(removedCube.Contains) && removedCube.All(cube.Contains))
            {
                isOre = true;
                break;
            }
        }
        if (isOre)
        {
            itemMined = mineralType;
        }
        // if the miner or the miners parent has an inventory then add the mineral to the inventory
        if (miner.GetComponent<Inventory>() != null)
        {
            if (miner.GetComponent<Inventory>().addItem(itemMined, 1f, -1) != null)
            {
                #if UNITY_EDITOR
                    Debug.Log("Inventory full");
                #endif
            }
        }
        else if (miner.transform.parent.GetComponent<Inventory>() != null)
        {
            if (miner.transform.parent.GetComponent<Inventory>().addItem(itemMined, 1f, -1) != null)
            {
                #if UNITY_EDITOR
                    Debug.Log("Inventory full");
                #endif
            }
        }

        if (cubesPointIndecies.Count > 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }
    
    // check dist every .2f on the ray
    List<int> RemoveCubesClosestToRay(Ray ray, RaycastHit hit)
    {
        float closestDistance = Mathf.Infinity;
        List<int> closestCube = new List<int>();
        Vector3 asteroidCurrentPosition = transform.position;
        // loop through all cubes
        foreach (List<int> cube in cubesPointIndecies)
        {
            // find the midpoint of the cube
            Vector3 midPoint = Vector3.zero;
            foreach (int index in cube)
            {
                midPoint += points[index];
            }
            midPoint /= cube.Count;
            // get the closest one to the hitpoint
            float distance = Vector3.Distance(hit.point, midPoint + asteroidCurrentPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCube = cube;
            }
        }
        // this should be a list of all the cubes that could be the best cube to remove
        List<List<int>> closestCubes = new List<List<int>>();
        List<Vector3> closestCubesMidpoints = new List<Vector3>();
        // get all the cubes that contain one of the points in the closest cube
        for (int i = 0; i < cubesPointIndecies.Count; i++)
        {
            List<int> cube = cubesPointIndecies[i];
            foreach (int index in cube)
            {
                if (closestCube.Contains(index))
                {
                    closestCubes.Add(cube);
                    closestCubesMidpoints.Add(new Vector3());
                    foreach (int index2 in cube)
                    {
                        closestCubesMidpoints[closestCubes.Count-1] += points[index2];
                    }
                    closestCubesMidpoints[closestCubes.Count-1] /= cube.Count;
                    closestCubesMidpoints[closestCubes.Count-1] += asteroidCurrentPosition;
                    break;
                }
            }
        }

        float closestDist = Mathf.Infinity;
        // get the distances and pick the least. thats the closest cube
        for (int i = 0; i < closestCubes.Count; i++)
        {
            float dist = Vector3.Distance(hit.point, closestCubesMidpoints[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestCube = closestCubes[i];
            }
        }
        List<int> closestCubeCopy = new List<int>(closestCube);
        // remove the last closest cube from cubePointIndecies
        cubesPointIndecies.Remove(closestCube);
        asteroidSpawnManager.setIndeciesForAsteroid(transform.localPosition, cubesPointIndecies);
        return closestCube;
    }

    // this should only happen when loading the game
    public void setIndecies(List<List<int>> AllIndecies)
    {
        edited = true;
        cubesPointIndecies = new List<List<int>>();
        foreach (List<int> indecies in AllIndecies)
        {
            cubesPointIndecies.Add(new List<int>(indecies));
        }
            
        if (cubesPointIndecies.Count > 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }

    public void regenerateAsteroid()
    {
        // copy originalCubesPointIndecies into cubesPointIndecies
        cubesPointIndecies = new List<List<int>>();
        foreach (List<int> indecies in originalCubesPointIndecies)
        {
            cubesPointIndecies.Add(new List<int>(indecies));
        }
        if (cubesPointIndecies.Count > 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }

}

