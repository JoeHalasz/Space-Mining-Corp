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

    ConvexHullCalculator ConvexHullCalcGlobal = new ConvexHullCalculator();

    public List<Vector3> points = new List<Vector3>();
    public List<Vector3> outsidePoints = new List<Vector3>();
    public IDictionary<Vector3, int> pointColors = new Dictionary<Vector3, int>();

    public IDictionary<Vector3, List<Vector3>> pointToCubes = new Dictionary<Vector3, List<Vector3>>();

    public Mesh mesh;

    [SerializeField]
    public float increment;

    public IDictionary<Vector3, int> pointsSetPositions = new Dictionary<Vector3, int>();

    public List<List<int>> cubesPointIndecies = new List<List<int>>();

    public List<Vector3> allVerts;
    public List<int> allTris;
    public List<Vector3> allNormals;

    IDictionary<T1, T2> copyIDict<T1, T2>(IDictionary<T1, T2> oldDict) { 
        // for all the keys in the old dict, add them to the new dict
        IDictionary<T1, T2> newDict = new Dictionary<T1, T2>();
        foreach (T1 key in oldDict.Keys)
        {
            newDict.Add(key, oldDict[key]);
        }
        return newDict;
    }

    List<List<T>> copyListOfLists<T>(List<List<T>> list)
    {
        List<List<T>> newList = new List<List<T>>();
        foreach (List<T> subList in list)
        {
            newList.Add(new List<T>(subList));
        }
        return newList;
    }

    // function takes in all the above variables and sets them in this script
    public void copyAll(Item _mineralType, bool _isBig, List<Vector3> _points, List<Vector3> _outsidePoints, IDictionary<Vector3, int> _pointColors,
                            IDictionary<Vector3, List<Vector3>> _pointToCubes, Mesh _mesh, float _increment, IDictionary<Vector3, int> _pointsSetPositions, List<List<int>> _cubesPointIndecies,
                            List<Vector3> _allVerts, List<int> _allTris, List<Vector3> _allNormals)
    {
        mineralType =           _mineralType;
        isBig =                 _isBig;
        points =                new List<Vector3>(_points);
        outsidePoints =         new List<Vector3>(_outsidePoints);
        pointColors =           copyIDict(_pointColors);
        pointToCubes =          copyIDict(_pointToCubes);
        mesh =                  (Mesh)Instantiate(_mesh);
        increment =             _increment;
        pointsSetPositions =    copyIDict(_pointsSetPositions);
        cubesPointIndecies =    copyListOfLists(_cubesPointIndecies);
        allVerts =              new List<Vector3>(_allVerts);
        allTris =               new List<int>(_allTris);
        allNormals =            new List<Vector3>(_allNormals);
    }

    public void Generate()
    {
        GenerateAsteroid();
    }

    void GenerateAsteroid()
    {
        // make a vector3 for the dimentions of the asteroid with random values between AsteroidMinSize and AsteroidMaxSize
        float size = Random.Range(AsteroidMinSize, AsteroidMaxSize);

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

        GenerateMesh();

    }

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

    Mesh GetSimplifiedMesh()
    {
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        ConvexHullCalculator ConvexHullCalc = new ConvexHullCalculator();
        
        List<Vector3> simpleVerts = new List<Vector3>();
        List<int> simpleTris = new List<int>();
        List<Vector3> simpleNormals = new List<Vector3>();

        ConvexHullCalc.GenerateHull(points, true, ref simpleVerts, ref simpleTris, ref simpleNormals);
        
        mesh.vertices = simpleVerts.ToArray();

        List<int> oreTris = new List<int>();
        List<int> otherTris = new List<int>();
        for (int i = 0; i < simpleTris.Count; i += 3)
        {
            // if the point is in the ore group
            if (pointColors[simpleVerts[simpleTris[i]]] == 1)
            {
                // add the tris to the ore group
                oreTris.Add(simpleTris[i]);
                oreTris.Add(simpleTris[i + 1]);
                oreTris.Add(simpleTris[i + 2]);
            }
            else
            {
                // add the tris to the other group
                otherTris.Add(simpleTris[i]);
                otherTris.Add(simpleTris[i + 1]);
                otherTris.Add(simpleTris[i + 2]);
            }
        }

        // add the ore tris to the submesh
        mesh.SetTriangles(oreTris.ToArray(), 0);
        // set the rest of the tris to the other submesh
        mesh.SetTriangles(otherTris.ToArray(), 1);

        mesh.normals = simpleNormals.ToArray();
        Vector2[] uvs = new Vector2[simpleVerts.Count];
        for (int i = 0; i < simpleVerts.Count; i++)
        {
            uvs[i] = new Vector2(simpleVerts[i].x, simpleVerts[i].z);
        }
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }


    void GenerateMesh()
    {
        allVerts = new List<Vector3>();
        allTris = new List<int>();
        allNormals = new List<Vector3>();

        GenerateVertsTrisAndNormalsForList(cubesPointIndecies);

        Color[] colors = new Color[allVerts.Count];

        // create the mesh
        mesh = new Mesh();
        mesh.vertices = allVerts.ToArray();

        // create a submesh for the ore material
        mesh.subMeshCount = 2;
        
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
        // add normals
        mesh.normals = allNormals.ToArray();

        // generate uvs
        Vector2[] uvs = new Vector2[allVerts.Count];
        for (int i = 0; i < allVerts.Count; i++)
        {
            uvs[i] = new Vector2(allVerts[i].x, allVerts[i].z);
        }
        mesh.uv = uvs;

        
        // TODO try lods on asteroids
        //// add lod group
        //LODGroup lodGroup = GetComponent<LODGroup>();
        //if (lodGroup == null)
        //{
        //    lodGroup = gameObject.AddComponent<LODGroup>();
        //}

        //LOD[] lods = new LOD[3];
        //lods[0] = new LOD(1f, new Renderer[] { GetComponent<Renderer>() });
        //lods[1] = new LOD(.25f, new Renderer[] { GetComponent<Renderer>() });
        //lods[2] = new LOD(.005f, new Renderer[] { GetComponent<Renderer>() });

        //Mesh simpleMesh = GetSimplifiedMesh();
        
        //lods[0].renderers[0].GetComponent<MeshFilter>().mesh = mesh;
        //lods[1].renderers[0].GetComponent<MeshFilter>().mesh = simpleMesh;

        //lodGroup.SetLODs(lods);
        //lodGroup.RecalculateBounds();

        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.RecalculateBounds();
    }


    public void MineAsteroid(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine)
    {
        RemoveCubesClosestToRay(ray, hit);

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

    // check dist every .2f on the ray
    void RemoveCubesClosestToRay(Ray ray, RaycastHit hit)
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
        // get all the cubes that contain one of the points in the closest cube
        foreach (List<int> cube in cubesPointIndecies)
        {
            foreach (int index in cube)
            {
                if (closestCube.Contains(index))
                {
                    closestCubes.Add(cube);
                    break;
                }
            }
        }

        
        // for each .2 meters on the ray, check to see if we are still getting closer to the closest cube midpoint
        // if we still are then keep going, if not then stop and remove the closest cube
        Vector3 rayDirection = ray.direction;
        Vector3 rayOrigin = ray.origin;
        Vector3 rayPoint = rayOrigin;
        Vector3 closestCubeMidPoint = Vector3.zero;
        
        float lastClosestDist = Mathf.Infinity;
        List<int> lastClosest = new List<int>();
        closestDistance = Mathf.Infinity;
        List<int> currentClosest = new List<int>();

        int i = 0;

        while (i++ < 5000)
        {
            //// make a small sphere at the rayPoint
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = rayPoint;
            //sphere.transform.localScale = new Vector3(.1f, .1f, .1f);
            //sphere.GetComponent<Renderer>().material.color = Color.red;
            // loop through all the cubes and find the closest one
            foreach (List<int> cube in closestCubes)
            {

                // if there is a currentClosest point
                if (currentClosest.Count != 0)
                {
                    // if the current distance to that closest point is greater than the last distance to the closest point
                    // then we have gone too far and should remove the last closest cube
                    float currentDistance = Vector3.Distance(rayPoint, closestCubeMidPoint + asteroidCurrentPosition);
                    if (currentDistance > lastClosestDist)
                    {
                        // remove the last closest cube from cubePointIndecies
                        cubesPointIndecies.Remove(lastClosest);
                        return;
                    }
                }

                // find the midpoint of the cube
                Vector3 midPoint = Vector3.zero;
                foreach (int index in cube)
                {
                    midPoint += points[index];
                }
                midPoint /= cube.Count;
                // get the closest one to the hitpoint
                float distance = Vector3.Distance(rayPoint, midPoint + asteroidCurrentPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentClosest = cube;
                    closestCubeMidPoint = midPoint;
                }
            }

            lastClosest = currentClosest;
            lastClosestDist = closestDistance;
            rayPoint += rayDirection * .2f;
        }

    }

}

