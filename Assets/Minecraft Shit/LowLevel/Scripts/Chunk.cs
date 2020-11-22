using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public ChunkCoord coord;

    private GameObject chunkObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private int vertexIndex = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    private World world;

    private byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    public byte Block (Vector3Int pos) { return voxelMap[pos.x, pos.y, pos.z]; }

    public void SetBlock (Vector3Int pos, byte newVal) {
        voxelMap[pos.x, pos.y, pos.z] = newVal;
        
    }

    public void Construct(ChunkCoord _coord, World _world)
    {
        world = _world;
        chunkObject = gameObject;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(_coord.x * VoxelData.ChunkWidth, 0, _coord.z * VoxelData.ChunkWidth);
        chunkObject.transform.name = "Chunk " + _coord;

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    public void Refresh()
    {
        vertexIndex = 0;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }


    ///<summary> Generating the data of the 'blocks'. </summary>
    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    voxelMap[x, y, z] = world.GetVoxel(position + new Vector3(x, y, z));
                }
            }
        }
    }

    ///<summary> Generate the voxel triangles/uvs for each 'block'. </summary>
    private void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; ++y)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; ++x)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; ++z)
                {
                    if (world.blockTypes[voxelMap[x, y, z]].isSolid)
                        AddVoxelDataToChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }

    /// <summary> Check if there is a block at this position (or out of chunk). </summary>
    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        
        if (!VoxelIsInChunk(x, y, z))
            return world.blockTypes[world.GetVoxel(pos + position)].isSolid;

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    /// <summary> Generate the voxel vertices, triangles and uvs for the specific position (block). </summary>
    private void AddVoxelDataToChunk(Vector3Int pos)
    {
        for (int f = 0; f < 6; ++f)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[f]))
            {
                for (int i = 0; i < 4; ++i)
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, i]]);

                AddTexture(world.blockTypes[Block(pos)].GetTextureID(f));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    /// <summary> Create and assign the mesh for the CHUNK using the generated vertices, triangles and uvs. </summary>
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary> Get the texture for the specified ID and add the sides to the uv map. </summary>
    private void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        //Shorten the variable to make code nicer. HERE! REMOVE TO OPTIMIZE!
        float d = VoxelData.NormalizedBlockTextureSize;

        x *= d;
        y *= d;

        //Convert from bottom-to-top to top-to-bottom, purely for usablility.
        y = 1f - y - d;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + d));
        uvs.Add(new Vector2(x + d, y));
        uvs.Add(new Vector2(x + d, y + d));
    }

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    private bool VoxelIsInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        return true;
    }
}


/// <summary> OBSOLETE? </summary>
[System.Serializable]
public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }


    //Add Operator here
    public bool SameAs(ChunkCoord other)
    {
        if (other == null)
            return false;
        return (x == other.x && z == other.z);
    }

    public override string ToString()
    {
        return "( " + x + ", " + z + ")";
    }
 }
