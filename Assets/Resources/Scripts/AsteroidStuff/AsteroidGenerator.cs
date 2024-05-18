using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeData
{
    public bool isOre = false;
    public bool isOutside = false;
    public List<int> indecies;
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector3> normals;
    public CubeData leftCube = null;
    public CubeData rightCube = null;
    public CubeData topCube = null;
    public CubeData bottomCube = null;
    public CubeData frontCube = null;
    public CubeData backCube = null;
    public List<Vector2> uvs;
    public Vector3 midpoint;
    public int distanceBandFromCenter;

    public CubeData()
    {
        indecies = new List<int>();
        verts = new List<Vector3>();
        tris = new List<int>();
        normals = new List<Vector3>();
    }

    public void setConnectedCubesToOutsideCubes()
    {
        if (leftCube != null)
        {
            leftCube.isOutside = true;
        }
        if (rightCube != null)
        {
            rightCube.isOutside = true;
        }
        if (topCube != null)
        {
            topCube.isOutside = true;
        }
        if (bottomCube != null)
        {
            bottomCube.isOutside = true;
        }
        if (frontCube != null)
        {
            frontCube.isOutside = true;
        }
        if (backCube != null)
        {
            backCube.isOutside = true;
        }

    }
}

public class AsteroidGenerator : MonoBehaviour
{
    [SerializeField]
    bool CREATE_DEBUG_POINTS = false;
    [SerializeField]
    bool REGENERATE_ASTEROID = false;
    public Item mineralType;
    public Item stone;
    public bool isBig = false;
    public float size;
    public List<Vector3> points;
    public List<CubeData> allCubeData;
    public HashSet<int> minedCubesIndecies;
    public Mesh mesh;
    public Mesh originalMesh;
    List<Vector3> allVerts;
    List<int> allTris;
    List<Vector3> allNormals;
    List<Vector2> allUVs;
    List<int> oreTris;
    List<int> otherTris;
    List<int> trisTemp;
    public AsteroidSpawnManager asteroidSpawnManager;
    Vector3 originalPosition;
    bool edited = false;
    bool newMeshCreated = false;

    void Start()
    {
        // set the layer to 8 (asteroid)
        gameObject.layer = 8;
#if UNITY_EDITOR
        InvokeRepeating("CheckDebugBooleans", 5, 1);
#endif
    }

#if UNITY_EDITOR
    void CheckDebugBooleans()
    {
        if (CREATE_DEBUG_POINTS)
        {
            foreach (Vector3 point in points)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = point + transform.position;
                sphere.transform.localScale = new Vector3(.1f, .1f, .1f);
                sphere.GetComponent<Renderer>().material.color = Color.red;
            }
            CREATE_DEBUG_POINTS = false;
        }
        if (REGENERATE_ASTEROID)
        {
            GenerateAsteroid();
            REGENERATE_ASTEROID = false;
            Debug.Log("Asteroid regenerated");
        }
    }
#endif

    public bool copyAll(ref AsteroidGenerator other, AsteroidSpawnManager _asteroidSpawnManager)
    {
        mineralType = other.mineralType;
        stone = other.stone;
        points = other.points;
        originalMesh = other.originalMesh;
        allCubeData = other.allCubeData;
        asteroidSpawnManager = _asteroidSpawnManager;
        if (originalMesh == null)
        {
#if UNITY_EDITOR
                Debug.Log("Failed to generate asteroid");
#endif
            return false;
        }
        return true;
    }

    public void ApplyMesh()
    {
        if (mesh == null)
        {
            GetComponent<MeshCollider>().sharedMesh = originalMesh;
            LODGroup lodGroup = GetComponent<LODGroup>();
            LOD[] lods = lodGroup.GetLODs();
            if (lods[0].renderers.Length == 0)
            {
                lods[0].renderers = new Renderer[1];
                lods[0].renderers[0] = GetComponent<Renderer>();
            }
            lods[0].renderers[0].GetComponent<MeshFilter>().sharedMesh = originalMesh;
            if (lods[1].renderers.Length == 0)
            {
                lods[1].renderers = new Renderer[1];
                lods[1].renderers[0] = transform.GetChild(0).GetComponent<Renderer>();
            }
            lodGroup.SetLODs(lods);
            originalMesh.UploadMeshData(false);
        }
        else
        {
            GetComponent<MeshCollider>().sharedMesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            mesh.UploadMeshData(false);
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

    void GenerateAsteroid() // this should only happen for the pregenerated asteroids at game load
    {
        IDictionary<Vector3, int> pointsSetPositions = new Dictionary<Vector3, int>();
        Dictionary<CubeData, int> cubeDistances = new Dictionary<CubeData, int>();
        List<List<List<CubeData>>> cubeDataGrid = new List<List<List<CubeData>>>();
        List<int> newCubePointIndecies = new List<int>();
        allCubeData = new List<CubeData>();
        points = new List<Vector3>();
        if (isBig)
        {
            size = 7.5f;
        }
        else
        {
            size = 3.5f;
        }

        float inbetweenPointSize = .5f;
        float increment = 1;

        float maxDistance = Vector3.Distance(new Vector3(0, 0, 0), new Vector3(size, size, size));


        float c;
        // calculate c
        for (c = -size; c <= size; c += increment) { }
        Vector3 asteroidCenter = new Vector3(-size + c, -size + c, -size + c);

        int count = 0;
        int xGridPos = 0;
        float percentUsedForCutoff = .75f;
        if (isBig)
        {
            percentUsedForCutoff = .65f;
        }
        for (float x = -size - increment; x <= size;)
        {
            int yGridPos = 0;
            cubeDataGrid.Add(new List<List<CubeData>>());
            x += increment;
            for (float y = -size - increment; y <= size;)
            {
                int zGridPos = 0;
                cubeDataGrid[cubeDataGrid.Count - 1].Add(new List<CubeData>());
                y += increment;
                for (float z = -size - increment; z <= size;)
                {
                    cubeDataGrid[cubeDataGrid.Count - 1][cubeDataGrid[cubeDataGrid.Count - 1].Count - 1].Add(null);
                    z += increment;
                    //calculate the distance from the center of the asteroid
                    float distance = Vector3.Distance(asteroidCenter, new Vector3(x, y, z));

                    // the further from center, the higher the chances are that the vertex will not be added
                    if (.99f >= distance / maxDistance)
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

                        newCubePointIndecies.Clear();
                        int pointsNotMoved = 0;
                        // foreach point if the point isnt in pointsSet add it to pointsSet
                        foreach (Vector3 point in pointsAround)
                        {
                            // get the distance from the center of the asteroid
                            float distanceFromCenter = Vector3.Distance(asteroidCenter, point);
                            if (percentUsedForCutoff >= distanceFromCenter / maxDistance)
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
                                pointsNotMoved++;
                            }
                        }
                        if (pointsNotMoved >= 5)
                        {
                            // make a new CubeData and add it to the list
                            CubeData cubeData = new CubeData();
                            cubeData.indecies = new List<int>(newCubePointIndecies);
                            allCubeData.Add(cubeData); // Add the cubeData to the list
                            cubeDataGrid[xGridPos][yGridPos][zGridPos] = cubeData;
                        }
                    }
                    zGridPos += 1;
                }
                yGridPos += 1;
            }
            xGridPos += 1;
        }

        for (int x = 0; x < cubeDataGrid.Count; x++)
        {
            for (int y = 0; y < cubeDataGrid[x].Count; y++)
            {
                for (int z = 0; z < cubeDataGrid[x][y].Count; z++)
                {
                    if (cubeDataGrid[x][y][z] != null)
                    {
                        if (x + 1 < cubeDataGrid.Count && cubeDataGrid[x + 1][y][z] != null)
                        {
                            cubeDataGrid[x][y][z].rightCube = cubeDataGrid[x + 1][y][z];
                        }
                        if (x - 1 >= 0 && cubeDataGrid[x - 1][y][z] != null)
                        {
                            cubeDataGrid[x][y][z].leftCube = cubeDataGrid[x - 1][y][z];
                        }
                        if (y + 1 < cubeDataGrid[x].Count && cubeDataGrid[x][y + 1][z] != null)
                        {
                            cubeDataGrid[x][y][z].topCube = cubeDataGrid[x][y + 1][z];
                        }
                        if (y - 1 >= 0 && cubeDataGrid[x][y - 1][z] != null)
                        {
                            cubeDataGrid[x][y][z].bottomCube = cubeDataGrid[x][y - 1][z];
                        }
                        if (z + 1 < cubeDataGrid[x][y].Count && cubeDataGrid[x][y][z + 1] != null)
                        {
                            cubeDataGrid[x][y][z].frontCube = cubeDataGrid[x][y][z + 1];
                        }
                        if (z - 1 >= 0 && cubeDataGrid[x][y][z - 1] != null)
                        {
                            cubeDataGrid[x][y][z].backCube = cubeDataGrid[x][y][z - 1];
                        }
                    }
                }
            }
        }

        Dictionary<int, int> numCubesInDistanceBand = new Dictionary<int, int>();

        // calculate cubePointIndeciesDistances
        foreach (CubeData cube in allCubeData)
        {
            float minDist = Mathf.Infinity;
            foreach (int index in cube.indecies)
            {
                float dist = Vector3.Distance(asteroidCenter, points[index]);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
            int distanceLayer = (int)minDist;
            cubeDistances.Add(cube, distanceLayer);
            if (!numCubesInDistanceBand.ContainsKey(distanceLayer))
            {
                numCubesInDistanceBand.Add(distanceLayer, 0);
            }
            numCubesInDistanceBand[distanceLayer] += 1;
        }

        // find the 2 furthest layers that have at least 32 cubes
        int furthest = 0;
        int furthest2 = 0;
        foreach (KeyValuePair<int, int> kvp in numCubesInDistanceBand)
        {
            if (kvp.Value >= 32)
            {
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
        }
        // if its an inside cube then make it a 90% chance that its an ore
        foreach (KeyValuePair<CubeData, int> kvp in cubeDistances)
        {
            if (kvp.Value != furthest && kvp.Value != furthest2)
            {
                if (Random.Range(0, 10) <= 9)
                {
                    kvp.Key.isOre = true;
                }
            }
        }

        // if a cube has any of its sides missing cubeData then its an outside Cube 
        foreach (CubeData cube in allCubeData)
        {
            if (cube.leftCube == null || cube.rightCube == null || cube.topCube == null || cube.bottomCube == null || cube.frontCube == null || cube.backCube == null)
            {
                cube.isOutside = true;
            }
        }

        float addRandomness = increment / 3f;
        // add some randomness to each point and give each point a color
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector3(points[i].x + Random.Range(-addRandomness, addRandomness), points[i].y + Random.Range(-addRandomness, addRandomness), points[i].z + Random.Range(-addRandomness, addRandomness));
        }

        List<int> oreIndecies = new List<int>();
        // 3% that a point is an ore. If a point is in a cube then make that cube an ore
        for (int i = 0; i < points.Count; i++)
        {
            if (Random.Range(0, 100) <= 3)
            {
                oreIndecies.Add(i);
            }
        }
        foreach (CubeData cube in allCubeData)
        {
            foreach (int index in cube.indecies)
            {
                if (oreIndecies.Contains(index))
                {
                    cube.isOre = true;
                    break;
                }
            }
        }

        GenerateVertsTrisNormalsAndUVs();
        GenerateMidPoints();
        GenerateMesh();
    }


    // this should only happen once whne the asteroid is first generated
    void GenerateVertsTrisNormalsAndUVs()
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> pointInThisCube = new List<Vector3>();

        foreach (CubeData cube in allCubeData)
        {
            verts.Clear();
            tris.Clear();
            normals.Clear();
            pointInThisCube.Clear();
            foreach (int index in cube.indecies)
            {
                pointInThisCube.Add(points[index]);
            }

            ConvexHullCalculator ConvexHullCalcGlobal = new ConvexHullCalculator();
            ConvexHullCalcGlobal.GenerateHull(pointInThisCube, true, ref verts, ref tris, ref normals);

            cube.verts = new List<Vector3>(verts);
            cube.tris = new List<int>(tris);
            cube.normals = new List<Vector3>(normals);
            // calculate the uvs using the verts x,y or x,z or y,z depending on which way the triangle is stretched
            cube.uvs = new List<Vector2>();
            for (int j = 0; j < cube.tris.Count; j += 3)
            {
                Vector3 v1 = cube.verts[cube.tris[j]];
                Vector3 v2 = cube.verts[cube.tris[j + 1]];
                Vector3 v3 = cube.verts[cube.tris[j + 2]];
                Vector3 side1 = v2 - v1;
                Vector3 side2 = v3 - v1;
                Vector3 normal = Vector3.Cross(side1, side2);
                normal.Normalize();

                float ratioX = Vector3.Distance(v1, v2) / Vector3.Distance(side1, side2);
                float ratioY = Vector3.Distance(v1, v3) / Vector3.Distance(side1, side2);

                cube.uvs.Add(new Vector2(ratioX, ratioY));
                cube.uvs.Add(new Vector2(0, ratioY));
                cube.uvs.Add(new Vector2(ratioX, 0));
            }
        }
    }

    void GenerateMidPoints()
    {
        foreach (CubeData cube in allCubeData)
        {
            Vector3 midPoint = Vector3.zero;
            foreach (int index in cube.indecies)
            {
                midPoint += points[index];
            }
            midPoint /= cube.indecies.Count;
            cube.midpoint = midPoint;
        }
    }

    void OnDestroy()
    {
        unloadAsteroid();
    }

    public void unloadAsteroid()
    {
        if (newMeshCreated)
        {
            Destroy(mesh);
        }
    }
    void GenerateMesh()
    {
        if (!newMeshCreated)
        {
            // first time setup
            mesh = new Mesh();
            newMeshCreated = true;
            allVerts = new List<Vector3>();
            allTris = new List<int>();
            allNormals = new List<Vector3>();
            allUVs = new List<Vector2>();
            oreTris = new List<int>();
            otherTris = new List<int>();
            trisTemp = new List<int>();
            if (minedCubesIndecies == null)
            {
                minedCubesIndecies = new HashSet<int>();
            }
        }
        else
        {
            Destroy(mesh);
            mesh = new Mesh();
            allVerts.Clear();
            allTris.Clear();
            allNormals.Clear();
            allUVs.Clear();
            oreTris.Clear();
            otherTris.Clear();
            trisTemp.Clear();
        }

        for (int i = 0; i < allCubeData.Count; i++)
        {
            if (!allCubeData[i].isOutside)
            {
                continue;
            }
            // if the cube has been mined then continue
            if (!minedCubesIndecies.Contains(i))
            {
                trisTemp.Clear();
                trisTemp.AddRange(allCubeData[i].tris);
                trisTemp.Select(i => i + allVerts.Count);
                allVerts.AddRange(allCubeData[i].verts);
                allTris.AddRange(trisTemp);
                allNormals.AddRange(allCubeData[i].normals);
                allUVs.AddRange(allCubeData[i].uvs);

                if (allCubeData[i].isOre)
                {
                    foreach (int tri in allCubeData[i].tris)
                    {
                        oreTris.Add(tri + allVerts.Count - allCubeData[i].verts.Count);
                    }
                }
                else
                {
                    foreach (int tri in allCubeData[i].tris)
                    {
                        otherTris.Add(tri + allVerts.Count - allCubeData[i].verts.Count);
                    }
                }
            }
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
        // add uvs
        mesh.uv = allUVs.ToArray();

        GetComponent<MeshCollider>().sharedMesh = mesh;

        LODGroup lodGroup = GetComponent<LODGroup>();
        LOD[] lods = lodGroup.GetLODs();
        if (lods[0].renderers.Length == 0)
        {
            lods[0].renderers = new Renderer[1];
            lods[0].renderers[0] = GetComponent<Renderer>();
        }
        lods[0].renderers[0].GetComponent<MeshFilter>().sharedMesh = mesh;
        if (lods[1].renderers.Length == 0)
        {
            lods[1].renderers = new Renderer[1];
            lods[1].renderers[0] = transform.GetChild(0).GetComponent<Renderer>();
        }
        lodGroup.SetLODs(lods);
        if (originalMesh == null)
        {
            originalMesh = Instantiate(mesh);
        }
        mesh.UploadMeshData(false);
    }

    public void MineAsteroid(GameObject miner, Ray ray, RaycastHit hit, Vector3 rayDirection, int amountToMine, WorldManager worldManager)
    {
        if (!edited)
        {
            if (minedCubesIndecies == null)
            {
                minedCubesIndecies = new HashSet<int>();
            }
            edited = true;
        }

        int removedCubeIndex = RemoveCubesClosestToRay(ray, hit, worldManager);
        Item itemMined = stone;

        if (allCubeData[removedCubeIndex].isOre)
        {
            itemMined = mineralType;
        }
        Debug.Log(itemMined.getName() + " mined by " + miner.name);

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

        if (minedCubesIndecies.Count < allCubeData.Count - 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }

    int RemoveCubesClosestToRay(Ray ray, RaycastHit hit, WorldManager worldManager)
    {
        float closestDistance = Mathf.Infinity;
        int closestCubeIndex = 0;
        Vector3 asteroidCurrentPosition = transform.position;

        float closestDist = Mathf.Infinity;
        // get the distances and pick the least. thats the closest cube
        for (int i = 0; i < allCubeData.Count; i++)
        {
            if (!minedCubesIndecies.Contains(i) && allCubeData[i].isOutside)
            {
                float dist = Vector3.Distance(hit.point, allCubeData[i].midpoint + asteroidCurrentPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestCubeIndex = i;
                }
            }
        }
        minedCubesIndecies.Add(closestCubeIndex);
        // set the cubes connected to the mined cube to be outside cubes
        allCubeData[closestCubeIndex].setConnectedCubesToOutsideCubes();

        asteroidSpawnManager.setRemovedChunksForAsteroid(worldManager.getObjectTruePosition(transform.position), minedCubesIndecies);
        return closestCubeIndex;
    }

    public void setRemovedCubeIndecies(HashSet<int> minedCubesIndecies)
    {
        edited = true;
        this.minedCubesIndecies = minedCubesIndecies;
        // use setConnectedCubesToOutsideCubes();
        foreach (int index in minedCubesIndecies)
        {
            allCubeData[index].setConnectedCubesToOutsideCubes();
        }

        if (minedCubesIndecies.Count < allCubeData.Count - 1)
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
        if (minedCubesIndecies.Count < allCubeData.Count - 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }
}

