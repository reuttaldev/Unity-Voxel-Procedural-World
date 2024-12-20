using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Settings", menuName = "Scriptable Objects/ Biome Settings")]
// store the appearance settings for each biome
public class BiomeSettings : ScriptableObject
{
    // everything has a serialize field marking and a private setter to ensure setting vales 
    // can only be adjusted through the editor, and not through scripts in runtime

    [Header("Noise")]
    [field: SerializeField]
    public NoiseSettings noise { get; private set; }
    [field: SerializeField]
    // have a secondary noise to determine whether this column should include e.g. rocks, trees
    public NoiseSettings secondaryNoise { get; private set; }
    [field: SerializeField]
    public NoiseSettings treeNoise { get; private set; }

    [Header("Appearance")]
    [field: SerializeField]
    private float waterScale = 0.3f; // how much % out of the chunk height should be with water
    [field: SerializeField]
    private float stoneProb = 0.3f; // likelihood of a chunk's column being stone
    [HideInInspector]
    public float waterThreshold { get; private set;}
    [field: SerializeField]
    public float stoneThreshold { get; private set; }
    [field: SerializeField]
    public VoxelType topVoxel { get; private set; }
    [field: SerializeField]
    public VoxelType waterVoxel { get; private set; }
    [field: SerializeField]
    public VoxelType underWaterVoxel { get; private set; }
    [field: SerializeField]
    public VoxelType nearWaterVoxel { get; private set; }
    [field: SerializeField]
    public VoxelType stoneVoxel { get; private set; }

    [Header("Trees")]
    [field: SerializeField]
    public float treeThreshold = 10f;
    [field: SerializeField]
    public int maxTrunkHeight = 7, minTrunkHeight = 4; // in voxel units

    private void OnEnable()
    {
        waterThreshold = waterScale * EnvironmentConstants.chunkHeight;
        stoneThreshold = (1- stoneProb) * EnvironmentConstants.chunkHeight; // 1- the prob bc we want the higher the prob, the lower the threshold
    }
}
public enum BiomeType : byte
{
    Desert,
    Forest,
    Mountain,
    Beach
}