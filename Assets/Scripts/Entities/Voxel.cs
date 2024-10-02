using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
/// since the voxel array is very big, I use byte for better memory usage
public enum VoxelType: byte
{
    Grass,
    Grass_Mix,
    Light_Sand,
    Dark_Sand,
    Sand_Mix,
    Water,
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
    /// The texture file contains the textures for all voxel types, stacked vertically. This marks how many other textures are stacked underneath it. 
    private int[] texturePosition;
    /// choose a random texture  
    public int TexturePosition => texturePosition[UnityEngine.Random.Range(0, texturePosition.Length)];
}
[CreateAssetMenu(fileName = "Voxels Texture Data", menuName = "Scriptable Objects/Voxels Texture Data")]
public class VoxelsTextureData : ScriptableObject
{
    [SerializeField]
    /// ordered by the VoxelType:: byte 
    public List<Voxel> data; 

}