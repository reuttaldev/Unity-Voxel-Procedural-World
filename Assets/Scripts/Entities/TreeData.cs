using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeData 
{
    public Vector3Int localTrunkPosition { get; }
    public VoxelType leafType { get; }

    public VoxelType trunkType { get; }
    public int trunkHeight { get; }
    public int leafRadius { get; }
    public TreeData(VoxelType trunkType, VoxelType leafType, Vector3Int pos,  int h,int r)
    {
        this.trunkType = trunkType;
        this.leafType = leafType;
        this.localTrunkPosition = pos;
        this.trunkHeight = h;
        this.leafRadius = r;

    }
}
