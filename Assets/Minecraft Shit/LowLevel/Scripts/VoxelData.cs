using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 5;
    public static readonly int ChunkHeight = 15;
    public static readonly int WorldSize = 30;

    public static int WorldSizeBlocks
    {
        get { return WorldSize * ChunkWidth; }
    }

    //public static readonly int ViewDistanceInChunks = 10;

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {
        new Vector3(0, 0, 0),   //0 front, bottom, front, left
        new Vector3(1, 0, 0),   //1 front, bottom, front, right
        new Vector3(1, 1, 0),   //2 front, top, front, right
        new Vector3(0, 1, 0),   //3 front, top, front, left
        new Vector3(0, 0, 1),   //4 back, bottom, back, left
        new Vector3(1, 0, 1),   //5 back, bottom, back, right
        new Vector3(1, 1, 1),   //6 back, top, back, right
        new Vector3(0, 1, 1),   //7 back, top, back, left
    };

    ///<summary> Array of displacements to use when checking for surrounding blocks: Back, Front, Bottom, Top, Left, Right. </summary>
    public static readonly Vector3[] faceChecks = new Vector3[6] {
        new Vector3(0, 0, -1),  //Back
        new Vector3(0, 0, 1),   //Front
        new Vector3(0, -1, 0),  //Bottom
        new Vector3(0, 1, 0),   //Top
        new Vector3(-1, 0, 0),  //Left
        new Vector3(1, 0, 0),   //Right
    };

    ///<summary> Array of Vertices for each face: Back, Front, Bottom, Top, Left, Right. </summary>
    public static readonly int[,] voxelTris = new int[6, 4] {
        //Back, Front, Bottom, Top, Left, Right
        { 0, 3, 1, 2 },   //Back Face
        { 5, 6, 4, 7 },   //Front Face
        { 1, 5, 0, 4 },   //Bottom Face
        { 3, 7, 2, 6 },   //Top Face
        { 4, 7, 0, 3 },   //Left Face
        { 1, 2, 5, 6 }    //Right Face
    };

    ///<summary> OBSOLETE? </summary>
    public static readonly Vector2[] voxelUvs = new Vector2[4] {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1)
    };
}
