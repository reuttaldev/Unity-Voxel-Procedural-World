using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public static class ChunkUtility
{

    // world pos is not Unity's coordinates in the world but rather counting of the chunks
    public static void FillChunkValues(ChunkData chunk)
    {
        for (int x = 0; x < EnvironmentConstants.chunkWidth; x++)
        {
            for (int z = 0; z < EnvironmentConstants.chunkDepth; z++)
            {

                float noise = Mathf.PerlinNoise(x * EnvironmentConstants.noiseScale, z * EnvironmentConstants.noiseScale);
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
    public static Vector3Int GlobalVoxelPositionToLocal(ChunkPosition containingChunkPos, Vector3 globalVoxelPos)
    {
        var chunkWorldPos = containingChunkPos.ToWorldPosition();
        return new Vector3Int
        {
            x = Mathf.FloorToInt(globalVoxelPos.x - chunkWorldPos.x),
            y = Mathf.FloorToInt(globalVoxelPos.y),
            z = Mathf.FloorToInt(globalVoxelPos.z - chunkWorldPos.z)
        };
    }
    public static bool ValidLocalVoxelCoordinates(Vector3 coordinates)
    {
        if (coordinates.x < 0 || coordinates.x >= EnvironmentConstants.chunkWidth || coordinates.z < 0 || coordinates.z >= EnvironmentConstants.chunkWidth || coordinates.y < 0 || coordinates.y >= EnvironmentConstants.chunkHeight)
            return false;
        return true;
    }

    public static bool ValidGlobalVoxelCoordinates(Vector3Int coordinates)
    {
        return coordinates.y >= 0; 
    }

}
