using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A biome represents a distinct environmental area within the game world.
/// The BiomeController manages the generation and behavior of the different biomes.
/// </summary>
public class BiomeController : MonoBehaviour
{
    [SerializeField]
    private List<BiomeSettings> biomesNoiseSettings; 

    public void FillChunkColumn(ChunkData chunk, Vector3 chunkWorldPos, int columnX, int columnZ)
    {
        int groundHeight = GetGroundHeight(BiomeType.Forest,chunkWorldPos.x + columnX, chunkWorldPos.z + columnZ);
        //Debug.Log("Ground Height is " + groundHeight);
        for (int y = 0; y < EnvironmentConstants.chunkHeight; y++)
        {
            chunk[columnX, y, columnZ] = DecideVoxelTypeByY(y, groundHeight);
        }
    }

    private int GetGroundHeight(BiomeType type, float x, float z)
    {
        float noise = NoiseUtility.OctavePerlin(x, z, biomesNoiseSettings[(byte)type]);
        noise = NoiseUtility.Redistribution(noise, biomesNoiseSettings[(byte)type]);
        return (int)NoiseUtility.NormalizeToChunkHeight(noise);
    }

    /// Determines the appropriate VoxelType based on the Y coordinate (height) of the voxel and the biome it is in.
    private VoxelType DecideVoxelTypeByY(int y, int groundPos)
    {
        if (y > groundPos)
        {
            if (y < EnvironmentConstants.waterThreshold)
            {
                if (y == groundPos +1)
                    return VoxelType.Dark_Sand;
                return VoxelType.Water;
            }
            else
                return VoxelType.Empty;
        }
        else if (y < groundPos)
        {
            return VoxelType.Light_Sand;
        }
        // equals
        return VoxelType.Grass;
    }
}

