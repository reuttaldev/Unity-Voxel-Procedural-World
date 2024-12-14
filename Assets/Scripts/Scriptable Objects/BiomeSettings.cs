using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Settings", menuName = "Scriptable Objects/ Biome Settings")]
public class BiomeSettings : ScriptableObject
{

    [Header("Noise")]
    public NoiseSettings noise;

    [Header("Appearance")]
    [SerializeField]
    private float waterScale = 0.3f; // how much % out of the chunk height should be with water
    [HideInInspector]
    public float waterThreshold;
    public VoxelType topVoxel;
    public VoxelType underWaterVoxel;
    public VoxelType nearWaterVoxel;

    [Header("Trees")]
    public float treeThreshold = 0.5f;

    private void OnEnable()
    {
        waterThreshold = waterScale * EnvironmentConstants.chunkHeight;
    }
}

public enum BiomeType : byte
{
    Desert,
    Forest,
    Mountain,
    Beach
}