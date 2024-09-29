using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
/// since the voxel array is very big, I use byte for better memory usage
public enum VoxelType: byte
{
    Grass,
    Mix,
    Dirt,
    Water,
    Sand,
    Dark_Grass,
    Light_Grass,
    Dark_Gravel,
    Light_Gravel,
    Stone,
    Tree,
    Empty
}


/// <summary>
///  Container used for texture information and collision information of the voxels 
/// </summary>
[Serializable]
public struct Voxel
{
    [SerializeField]
    private VoxelType type;
    [SerializeField]
    private Material[] materials;
    /// choose a random material 
    public Material GetMaterial => materials[UnityEngine.Random.Range(0, materials.Length)];
    [SerializeField]
    private bool generateCollision;
    public bool GenerateCollision => generateCollision;


}
[CreateAssetMenu(fileName = "Voxels Data", menuName = "Scriptable Objects/Voxels Data")]
public class VoxelsData : ScriptableObject
{
    [SerializeField]
    /// ordered by the VoxelType:: byte 
    private List<Voxel> voxelData; 

}