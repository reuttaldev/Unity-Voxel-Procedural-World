using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
/// since the voxel array is very big, I use byte for better memory usage
public enum VoxelType
{
    Light_Grass,
    Light_Sand,
    Dark_Sand,
    Water,
    Light_Trunk,
    Light_leafs,
    Dark_Trunk,
    Dark_Leafs,
    Medium_Trunk,
    Pink_Trunk,
    Pink_Leafs_A,
    Pink_Leafs_B,
    Leafs_With_Flowers,
    Purple_Trunk,
    Light_Rocks,
    Dark_Rocks,
    Dark_Grass,
    Dark_Water,
    Empty
}



/// <summary>
///  Container used for texture information and collision information of the voxels 
/// </summary>
[Serializable]
public struct VoxelData
{
    [SerializeField]
    private VoxelType type;
    [SerializeField]
    /// The texture file contains the textures for all voxel types, stacked vertically. This marks how many other textures are stacked underneath it. 
    private Vector2Int topTexturePosition;
    [SerializeField]
    private Vector2Int sideTexturePosition;
    public VoxelType Type => type;
    /// choose a random texture  
    public Vector2Int TopTexturePosition => topTexturePosition;
    public Vector2Int SideTexturePosition => sideTexturePosition;
}
