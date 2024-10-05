using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public static class ChunkUtility
{
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

    /// <summary>
    /// Returns a list of chunk positions that are surrounding the given pos
    /// elements are being generated on the fly, can return Ienumerable safley
    /// </summary>
    public static IEnumerable<ChunkPosition> GetChunkPositionsAroundPos(ChunkPosition pos)
    {
        int size = EnvironmentConstants.worldSizeInChunks / 2;
        for (int x = pos.x -size; x < pos.x + size; x++)
        {
            for (int z = pos.z - size; z < pos.z + size; z++)
            {
                yield return (new ChunkPosition(x, z));  
            }
        }
    }

}
