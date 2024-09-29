using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public static class ChunksUtility
{
    public static void FillChunkValues(Chunk chunk, Vector3Int worldPos)
    {
        for (int x = 0; x < EnvironmentConstants.chunkWidth; x++)
        {
            for (int z = 0; z < EnvironmentConstants.chunkDepth; z++)
            {

                float noise = Mathf.PerlinNoise((worldPos.x + x) * EnvironmentConstants.noiseScale, (worldPos.z + z) * EnvironmentConstants.noiseScale);
                int groundHeight = Mathf.FloorToInt(noise * EnvironmentConstants.chunkHeight);

                for (int y = 0; y < EnvironmentConstants.chunkHeight; y++)
                {
                    chunk[x, y, z] = DecideVoxelTypeByY(y, groundHeight);
                }
            }
        }
    }
    /// Determines the appropriate VoxelType based on the Y coordinate (height) of the voxel.
    public static VoxelType DecideVoxelTypeByY(int y, int groundPos)
    {
        if (y > groundPos)
        {
            //if (y < EnvironmentConstants.waterThreshold)
                //return VoxelType.Water;
            //else
                return VoxelType.Empty;
        }
        else if (y < EnvironmentConstants.chunkHeight)
        {
            return VoxelType.Light_Sand;
        }
        else
        {
            return VoxelType.Grass;

        }
    }
    // return the position of the chunk that contains the voxel at the given (global) position
    public static Vector3Int GetChunkPositionByVoxelPosition(Vector3Int voxelGlobalPos)
    {
        return new Vector3Int
        {
            x = Mathf.FloorToInt(voxelGlobalPos.x / (float)EnvironmentConstants.chunkSize) * EnvironmentConstants.chunkWidth,
            y = Mathf.FloorToInt(voxelGlobalPos.y / (float)EnvironmentConstants.chunkHeight) * EnvironmentConstants.chunkHeight,
            z = Mathf.FloorToInt(voxelGlobalPos.z / (float)EnvironmentConstants.chunkDepth) * EnvironmentConstants.chunkDepth,
        };
    }

    public static Vector3Int GlobalVoxelPositionToLocal(Vector3Int chunkPos, Vector3Int globalVoxelPos)
    {
        return new Vector3Int
        {
            x = globalVoxelPos.x - chunkPos.x,
            y = globalVoxelPos.y - chunkPos.y,
            z = globalVoxelPos.z - chunkPos.z
        };
    }

}
