using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeData 
{
    public ChunkData parentChunkData { get; }
    public Vector3Int localTrunkPosition { get; }
    public VoxelType type { get; }
    public int trunkHeight { get; }
    public TreeData(VoxelType trunkType, VoxelType leafType, Vector3Int pos, ChunkData p, int h)
    {
        this.type = type;
        localTrunkPosition = pos;
        parentChunkData = p;
        trunkHeight = h;
    }
}
