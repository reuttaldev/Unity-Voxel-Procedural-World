using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Voxels Texture Data", menuName = "Scriptable Objects/Voxels Texture Data")]
public class VoxelsTextureData : ScriptableObject
{
    [SerializeField]
    /// ordered by the VoxelType:: byte 
    public List<Voxel> data;

}
