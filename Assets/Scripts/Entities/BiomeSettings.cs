using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Settings", menuName = "Scriptable Objects/ Biome Settings")]
public class BiomeSettings : ScriptableObject
{
    public BiomeType type;

    [Header("Noise")]
    public float zoom = 0.01f;
    public float zoomOffset = 0.01f;
    public float noiseScale = 0.01f;
    public float noiseOffset = -100;
    public int octaves = 5;
    public float amplitudeMultiplier = 0.5f;
    public float redistributionMultiplier = 1.0f;
    public float expo = 5;    

    [Header("Appearance")]
    public float waterScale = 0.3f; // how much % out of the chunk height should be with water
    public VoxelType topVoxel;
    public VoxelType underGroundVoxel;
    public VoxelType underWaterVoxel;

}

public enum BiomeType : byte
{
    Desert,
    Forest
}