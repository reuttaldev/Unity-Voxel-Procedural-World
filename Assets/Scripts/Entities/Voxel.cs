using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum VoxelType
{
    Empty,
    Water,
    Sand,
    Dirt,
    Dark_Grass,
    Light_Grass,
    Dark_Gravel,
    Light_Gravel,
    Stone,
    Tree,
    Sky
}

[CreateAssetMenu(fileName = "Voxel", menuName ="Scriptable Objects/Voxel")]
public class Voxel
{
    public VoxelType type;
    public Vector3Int relativePosition; // each voxel has a position relative to the chunk it is in 
    public bool generateCollision = true;
}
