using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public static ChunkPosition[] GetInitChunksPositions()
    {
        int size = EnvironmentConstants.worldSizeInChunks * EnvironmentConstants.worldSizeInChunks;
        ChunkPosition[] a = new ChunkPosition[size];
        for (int x = 0; x < EnvironmentConstants.worldSizeInChunks; x++)
        {
            for (int z = 0; z < EnvironmentConstants.worldSizeInChunks; z++)
            {
                // Map the 2D indices (x, z) to a 1D index
                int index = x * EnvironmentConstants.worldSizeInChunks + z;
                a[index] =  new ChunkPosition(x, z);
            }
        }
        return a;
    }
    /// <summary>
    /// return the elements from l that are not found in our chunk dictionary, order by distance (so we render the chunks that are closest to the player first). 
    /// to array because we want to take a snapshot of the chunks collection as it is, since it might change between iteration as we generate new data
    /// </summary>
    public static ChunkPosition[] GetNonExistingChunks(Dictionary<ChunkPosition,ChunkData> chunks,IEnumerable<ChunkPosition> l, Vector3 orderByPos)
    {
        return l.Where(pos => !chunks.ContainsKey(pos)).OrderBy(pos => Vector3.Distance(orderByPos, pos.ToWorldPosition())).ToArray();
    }
    /// <summary>
    /// return the that are found in our chunk dictionary but not in l. The order does not matter
    /// to array because we will change the Collection while iterating (by deleting) and we cannot do that on IEnumerable
    /// </summary>
    public static ChunkPosition[] GetExcessChunks(Dictionary<ChunkPosition, ChunkData> chunks, IEnumerable<ChunkPosition> l)
    {
        return chunks.Keys.Where(pos => !l.Contains(pos)).ToArray();
    }

}
