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
    float AsteroidMinSize = 3.2f; // 4.2 for isBig
    float AsteroidMaxSize = 3.7f; // 4.7 for isBig
    public float size;
    public List<Vector3> points;
    public List<CubeData> allCubeData;
    public List<CubeData> allOutsideCubeData;
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
        allOutsideCubeData = other.allOutsideCubeData;
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

    void GenerateAsteroid()
    {
        List<List<List<CubeData>>> cubeDataGrid = new List<List<List<CubeData>>>();
        IDictionary<Vector3, int> pointsSetPositions = new Dictionary<Vector3, int>();
        List<int> newCubePointIndecies = new List<int>();
        IDictionary<Vector3, Vector3> posToRandomizedPos = new Dictionary<Vector3, Vector3>();
        allCubeData = new List<CubeData>();
        allOutsideCubeData = new List<CubeData>();
        points = new List<Vector3>();
        if (isBig)
        {
            size = 5f; // make sure this is an even number
        }
        else
        {
            size = 5f; // make sure this is an even number 
        }

        float increment = 1;

        float maxDistFromCenter = Vector3.Distance(new Vector3(0, 0, 0), new Vector3(size, size, size));

        float c;
        // calculate c
        for (c = -size; c <= size; c += increment) { }
        Vector3 asteroidCenter = new Vector3(-size + c, -size + c, -size + c);
        
        ConvexHullCalculator ConvexHullCalcGlobal = new ConvexHullCalculator();

        // inizialize cubeDataGrid
        for (int i = 0; i < size * 4; i++)
        {
            cubeDataGrid.Add(new List<List<CubeData>>());
            for (int j = 0; j < size * 4; j++)
            {
                cubeDataGrid[i].Add(new List<CubeData>());
                for (int k = 0; k < size * 4; k++)
                {
                    cubeDataGrid[i][j].Add(null);
                }
            }
        }

        createAllCubes((int)size, asteroidCenter, asteroidCenter, maxDistFromCenter, true, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
        
        GenerateMesh();
    }

    CubeData createAllCubes(int maxDistBand, Vector3 pos, Vector3 asteroidCenter, float maxDistFromCenter, bool lastWasOre, 
                            ref List<List<List<CubeData>>> cubeDataGrid, ref IDictionary<Vector3, int> pointsSetPositions, 
                            ref IDictionary<Vector3, Vector3> posToRandomizedPos, ref List<Vector3> points, ref ConvexHullCalculator ConvexHullCalcGlobal)
    {
        int distBand = (int)(Mathf.Abs(asteroidCenter.x - pos.x) + Mathf.Abs(asteroidCenter.y - pos.y) + Mathf.Abs(asteroidCenter.z - pos.z));
        if (distBand > maxDistBand)
        {
            return null;
        }
        if (cubeDataGrid[(int)pos.x + maxDistBand][(int)pos.y+ maxDistBand][(int)pos.z+ maxDistBand] != null)
        {
            return cubeDataGrid[(int)pos.x+ maxDistBand][(int)pos.y+ maxDistBand][(int)pos.z+ maxDistBand];
        }

        // if we are on the last layer then make sure we arent too far from the center
        if (distBand == maxDistBand)
        {
            if (.75f >= Vector3.Distance(pos, asteroidCenter) / maxDistFromCenter)
            {
                return null;
            }
        }

        // generate 8 points around the position of the voxel that make a cube
        Vector3[] pointsAround = new Vector3[8];
        pointsAround[0] = new Vector3(pos.x + .5f, pos.y + .5f, pos.z + .5f);
        pointsAround[1] = new Vector3(pos.x + .5f, pos.y + .5f, pos.z - .5f);
        pointsAround[2] = new Vector3(pos.x + .5f, pos.y - .5f, pos.z + .5f);
        pointsAround[3] = new Vector3(pos.x + .5f, pos.y - .5f, pos.z - .5f);
        pointsAround[4] = new Vector3(pos.x - .5f, pos.y + .5f, pos.z + .5f);
        pointsAround[5] = new Vector3(pos.x - .5f, pos.y + .5f, pos.z - .5f);
        pointsAround[6] = new Vector3(pos.x - .5f, pos.y - .5f, pos.z + .5f);
        pointsAround[7] = new Vector3(pos.x - .5f, pos.y - .5f, pos.z - .5f);

        List<int> newCubePointIndecies = new List<int>();
        foreach (Vector3 point in pointsAround)
        {
            // if we are on the last layer then make sure each point isnt too far from the center
            if (distBand < maxDistBand || .75f >= (Vector3.Distance(asteroidCenter, point)) / maxDistFromCenter)
            {
                // make a point using the int of all the points
                if (!pointsSetPositions.ContainsKey(point))
                {
                    pointsSetPositions.Add(point, points.Count);
                    points.Add(point);
                    // randomize the position of the point
                    Vector3 randomizedPos = new Vector3(point.x + Random.Range(-.33f, .33f), point.y + Random.Range(-.33f, .33f), point.z + Random.Range(-.33f, .33f));
                    posToRandomizedPos.Add(point, randomizedPos);
                }
                newCubePointIndecies.Add(pointsSetPositions[point]);
            }
        }
        if (newCubePointIndecies.Count >= 5)
        {
            // make a new CubeData and add it to the list
            CubeData cubeData = new CubeData();
            cubeData.distanceBandFromCenter = distBand;
            cubeData.indecies = new List<int>(newCubePointIndecies);
            if (distBand == maxDistBand)
            {
                if (lastWasOre)
                {
                    cubeData.isOre = Random.Range(0, 100) <= 70; // TODO mess with this number if the ores dont look good
                }
                else
                {
                    cubeData.isOre = Random.Range(0, 100) <= 10;
                }
            }
            else
            {
                cubeData.isOre = Random.Range(0, 100) <= 90;
            }
            // calculate midpoint using the randomized points in newCubePointIndecies
            Vector3 midpoint = new Vector3(0, 0, 0);
            foreach (int index in newCubePointIndecies)
            {
                midpoint += posToRandomizedPos[points[index]];
            }
            midpoint /= newCubePointIndecies.Count;
            cubeData.midpoint = midpoint;
            

            
            List<Vector3> pointInThisCube = new List<Vector3>();
            foreach (int index in cubeData.indecies)
            {
                pointInThisCube.Add(posToRandomizedPos[points[index]]);
            }
            ConvexHullCalcGlobal.GenerateHull(pointInThisCube, true, ref cubeData.verts, ref cubeData.tris, ref cubeData.normals);
            // calculate the uvs using the verts x,y or x,z or y,z depending on which way the triangle is stretched
            cubeData.uvs = new List<Vector2>();
            for (int j = 0; j < cubeData.tris.Count; j += 3)
            {
                Vector3 v1 = cubeData.verts[cubeData.tris[j]];
                Vector3 v2 = cubeData.verts[cubeData.tris[j + 1]];
                Vector3 v3 = cubeData.verts[cubeData.tris[j + 2]];
                Vector3 side1 = v2 - v1;
                Vector3 side2 = v3 - v1;
                Vector3 normal = Vector3.Cross(side1, side2);
                normal.Normalize();

                float ratioX = Vector3.Distance(v1, v2) / Vector3.Distance(side1, side2);
                float ratioY = Vector3.Distance(v1, v3) / Vector3.Distance(side1, side2);

                cubeData.uvs.Add(new Vector2(ratioX, ratioY));
                cubeData.uvs.Add(new Vector2(0, ratioY));
                cubeData.uvs.Add(new Vector2(ratioX, 0));
            }

            // if any of them are null then this is an outside cube
            if (cubeData.leftCube == null || cubeData.rightCube == null || cubeData.topCube == null || cubeData.bottomCube == null || cubeData.frontCube == null || cubeData.backCube == null)
            {
                cubeData.isOutside = true;
            }

            cubeDataGrid[(int)pos.x+ maxDistBand][(int)pos.y+ maxDistBand][(int)pos.z+ maxDistBand] = cubeData;
            
            cubeData.leftCube =   createAllCubes(maxDistBand, new Vector3(pos.x - 1, pos.y, pos.z), asteroidCenter, maxDistFromCenter, cubeData.isOre, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
            cubeData.rightCube =  createAllCubes(maxDistBand, new Vector3(pos.x + 1, pos.y, pos.z), asteroidCenter, maxDistFromCenter, cubeData.isOre, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
            cubeData.topCube =    createAllCubes(maxDistBand, new Vector3(pos.x, pos.y + 1, pos.z), asteroidCenter, maxDistFromCenter, cubeData.isOre, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
            cubeData.bottomCube = createAllCubes(maxDistBand, new Vector3(pos.x, pos.y - 1, pos.z), asteroidCenter, maxDistFromCenter, cubeData.isOre, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
            cubeData.frontCube =  createAllCubes(maxDistBand, new Vector3(pos.x, pos.y, pos.z + 1), asteroidCenter, maxDistFromCenter, cubeData.isOre, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
            cubeData.backCube =   createAllCubes(maxDistBand, new Vector3(pos.x, pos.y, pos.z - 1), asteroidCenter, maxDistFromCenter, cubeData.isOre, ref cubeDataGrid, ref pointsSetPositions, ref posToRandomizedPos, ref points, ref ConvexHullCalcGlobal);
            
            allOutsideCubeData.Add(cubeData);
            allCubeData.Add(cubeData);

            // create a small sphere at the midpoint where the color is different depending on the distband
            Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.black, Color.white };
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = midpoint + transform.position + new Vector3(0, 10, 0);
            sphere.transform.localScale = new Vector3(.1f, .1f, .1f);
            if (distBand != cubeData.distanceBandFromCenter)
            {
                Debug.Log("This one was fixed");
            }
            sphere.GetComponent<Renderer>().material.color = colors[cubeData.distanceBandFromCenter % colors.Length];
            
            return cubeData; // Add the cubeData to the list
        }
        else
        {
            return null;
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

        for (int i = 0; i < allOutsideCubeData.Count; i++)
        {
            if (!allOutsideCubeData[i].isOutside)
            {
                continue;
            }
            // if the cube has been mined then continue
            if (!minedCubesIndecies.Contains(i))
            {
                trisTemp.Clear();
                trisTemp.AddRange(allOutsideCubeData[i].tris);
                trisTemp.Select(i => i + allVerts.Count);
                allVerts.AddRange(allOutsideCubeData[i].verts);
                allTris.AddRange(trisTemp);
                allNormals.AddRange(allOutsideCubeData[i].normals);
                allUVs.AddRange(allOutsideCubeData[i].uvs);

                if (allOutsideCubeData[i].isOre)
                {
                    foreach (int tri in allOutsideCubeData[i].tris)
                    {
                        oreTris.Add(tri + allVerts.Count - allOutsideCubeData[i].verts.Count);
                    }
                }
                else
                {
                    foreach (int tri in allOutsideCubeData[i].tris)
                    {
                        otherTris.Add(tri + allVerts.Count - allOutsideCubeData[i].verts.Count);
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

        if (allOutsideCubeData[removedCubeIndex].isOre)
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

        if (minedCubesIndecies.Count < allOutsideCubeData.Count - 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }

    void setConnectedCubesToOutsideCubes(CubeData cube)
    {
        if (cube.leftCube != null && !cube.leftCube.isOutside)
        {
            cube.leftCube.isOutside = true;
            allOutsideCubeData.Add(cube.leftCube);
        }
        if (cube.rightCube != null && !cube.rightCube.isOutside)
        {
            cube.rightCube.isOutside = true;
            allOutsideCubeData.Add(cube.rightCube);
        }
        if (cube.topCube != null && !cube.topCube.isOutside)
        {
            cube.topCube.isOutside = true;
            allOutsideCubeData.Add(cube.topCube);
        }
        if (cube.bottomCube != null && !cube.bottomCube.isOutside)
        {
            cube.bottomCube.isOutside = true;
            allOutsideCubeData.Add(cube.bottomCube);
        }
        if (cube.frontCube != null && !cube.frontCube.isOutside)
        {
            cube.frontCube.isOutside = true;
            allOutsideCubeData.Add(cube.frontCube);
        }
        if (cube.backCube != null && !cube.backCube.isOutside)
        {
            cube.backCube.isOutside = true;
            allOutsideCubeData.Add(cube.backCube);
        }
    }

    int RemoveCubesClosestToRay(Ray ray, RaycastHit hit, WorldManager worldManager)
    {
        float closestDistance = Mathf.Infinity;
        int closestCubeIndex = 0;
        Vector3 asteroidCurrentPosition = transform.position;

        float closestDist = Mathf.Infinity;
        // get the distances and pick the least. thats the closest cube
        for (int i = 0; i < allOutsideCubeData.Count; i++)
        {
            if (!minedCubesIndecies.Contains(i) && allOutsideCubeData[i].isOutside)
            {
                float dist = Vector3.Distance(hit.point, allOutsideCubeData[i].midpoint + asteroidCurrentPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestCubeIndex = i;
                }
            }
        }
        minedCubesIndecies.Add(closestCubeIndex);
        // set the cubes connected to the mined cube to be outside cubes
        setConnectedCubesToOutsideCubes(allOutsideCubeData[closestCubeIndex]);
        
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
            setConnectedCubesToOutsideCubes(allOutsideCubeData[index]);
        }

        if (minedCubesIndecies.Count < allOutsideCubeData.Count - 1)
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
        if (minedCubesIndecies.Count < allOutsideCubeData.Count - 1)
        {
            GenerateMesh();
        }
        else
        {
            asteroidSpawnManager.addRemovedAsteroid(originalPosition); // this will destroy it as well
        }
    }
}

