using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform player;
    public Vector3 spawnPoint;

    [Range(1, 100)]
    public int ViewDistanceInChunks = 10;

    public Material material;
    public BlockType[] blockTypes;

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSize, VoxelData.WorldSize];

    public List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    private Vector3 lastPlayerPos = Vector3.zero;
    private ChunkCoord lastChunk = new ChunkCoord(0, 0);

    private byte[,,] allBlocks = new byte[VoxelData.WorldSizeBlocks, VoxelData.ChunkHeight, VoxelData.WorldSizeBlocks];
    public byte Block(Vector3 pos) { return allBlocks[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z)]; }

    void Start()
    {
        spawnPoint = new Vector3(VoxelData.WorldSize * VoxelData.ChunkWidth / 2f, VoxelData.ChunkHeight + 1, VoxelData.WorldSize * VoxelData.ChunkWidth / 2f);
        GenerateWorld();
    }

    void Update()
    {
        CheckViewDistance();
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < VoxelData.WorldSizeBlocks; ++x)
        {
            for (int z = 0; z < VoxelData.WorldSizeBlocks; ++z)
            {
                for (int y = 0; y < VoxelData.ChunkHeight; ++y)
                {
                    allBlocks[x, y, z] = (byte)Random.Range(1, blockTypes.Length);
                }
            }
        }

        for (int x = (VoxelData.WorldSize / 2) - ViewDistanceInChunks; x < (VoxelData.WorldSize / 2) + ViewDistanceInChunks; ++x)
        {
            for (int z = (VoxelData.WorldSize / 2) - ViewDistanceInChunks; z < (VoxelData.WorldSize / 2) + ViewDistanceInChunks; ++z)
            {
                CreateNewChunk(x, z);
            }
        }

        player.position = spawnPoint;
    }

    ChunkCoord ChunkFromPos(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    private void CheckViewDistance()
    {
        ChunkCoord coord = ChunkFromPos(player.position);
        if (!coord.SameAs(lastChunk)) {
            lastChunk = coord;

            List<ChunkCoord> chunksToRemove = new List<ChunkCoord>(activeChunks);

            for (int x = coord.x - ViewDistanceInChunks; x < coord.x + ViewDistanceInChunks; ++x)
            {
                for (int z = coord.z - ViewDistanceInChunks; z < coord.z + ViewDistanceInChunks; ++z)
                {
                    ChunkCoord c = new ChunkCoord(x, z);

                    if (ChunkIsInWorld(c))
                    {
                        if (chunks[x, z] == null)
                        {
                            CreateNewChunk(x, z);
                        }
                        else if (!chunks[x, z].isActive)
                        {
                            chunks[x, z].isActive = true;
                            activeChunks.Add(c);
                        }
                    }
                    
                    for (int i = 0; i < chunksToRemove.Count; ++i)
                    {
                        if (chunksToRemove[i].SameAs(c))
                        {
                            chunksToRemove.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            foreach (ChunkCoord c in chunksToRemove)
            {
                chunks[c.x, c.z].isActive = false;
                activeChunks.Remove(c);
            }
        }
    }

    public byte GetVoxel(Vector3 pos)
    {
        if (!VoxelIsInWorld(pos))
            return 0;


        
        return allBlocks[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z)];
    }

    public void DestroyBlock(Vector3 pos)
    {
        if (VoxelIsInWorld(pos))
        {
            Debug.Log("Exists in World");
            Vector3Int Ipos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            allBlocks[Ipos.x, Ipos.y, Ipos.z] = 0;
            ChunkCoord c = ChunkFromPos(pos);
            chunks[c.x, c.z].SetBlock(new Vector3Int(Ipos.x - c.x * VoxelData.ChunkWidth, Ipos.y, Ipos.z - c.z * VoxelData.ChunkWidth), 0);
            chunks[c.x, c.z].Refresh();
        }
    }
    
    public void CreateBlock(Vector3 pos)
    {
        if (VoxelIsInWorld(pos))
        {
            Debug.Log("Exists in World");
            Vector3Int Ipos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            allBlocks[Ipos.x, Ipos.y, Ipos.z] = 1;
            ChunkCoord c = ChunkFromPos(pos);
            chunks[c.x, c.z].SetBlock(new Vector3Int(Ipos.x - c.x * VoxelData.ChunkWidth, Ipos.y, Ipos.z - c.z * VoxelData.ChunkWidth), 0);
            chunks[c.x, c.z].Refresh();
        }
    }

    private void CreateNewChunk(int x, int z)
    {
        GameObject GO = new GameObject();
        
        chunks[x, z] = GO.AddComponent<Chunk>();
        chunks[x, z].Construct(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

    bool ChunkIsInWorld(ChunkCoord chunkPos)
    {
        if (chunkPos.x > 0 && chunkPos.x < VoxelData.WorldSize - 1 &&
            chunkPos.z > 0 && chunkPos.z < VoxelData.WorldSize - 1)
            return true;
        return false;
    }

    bool VoxelIsInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeBlocks &&
            pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeBlocks)
            return true;
        return false;
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture IDs")]
    public int backFace;
    public int frontFace;
    public int bottomFace;
    public int topFace;
    public int leftFace;
    public int rightFace;

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex) {
            case 0:
                return backFace;
            case 1:
                return frontFace;
            case 2:
                return bottomFace;
            case 3:
                return topFace;
            case 4:
                return leftFace;
            case 5:
                return rightFace;
            default:
                Debug.LogError("Getting ID for face outside of 0-5 range.");
                return 0;
        }
    }
}