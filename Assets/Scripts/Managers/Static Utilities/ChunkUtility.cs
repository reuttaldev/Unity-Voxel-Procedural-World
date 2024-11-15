using System.Collections.Generic;
using System.IO;
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
    /// algorithm inspired from this thread: https://stackoverflow.com/questions/33684970/print-2-d-array-in-clockwise-expanding-spiral-from-center
    /// </summary>
    public static IEnumerable<ChunkPosition> GetChunkPositionsAroundPos(ChunkPosition pos)
    {
        // generate the positions around the given pos, starting from the middle and going outwards
        (int, int)[] directions = new (int, int)[4] { (0, 1), (1, 0), (0, -1), (-1, 0) }; // up, right,down,left
        int n = EnvironmentConstants.worldSizeInChunks;
        int center = n % 2 == 0 ? n / 2 - 1 : n / 2; // account for even and odd n
        int x= center, z = center, d = 0, stepLength = 1, stepCounter = 0;
        while (x>=0 && z>=0 && x<n && z<n)
        {
            for (int i = 0; i < stepLength; i++)
            {
                yield return new ChunkPosition(x+pos.x-center,z+pos.z-center);
                x += directions[d].Item1;
                z += directions[d].Item2;
            }
            d++;
            d %= 4;
            // we want to increase the length of the step after every other step. at the end of each step, we change direction
            stepCounter += 1;
            stepCounter %= 2;
            stepLength = stepCounter == 0 ? ++stepLength : stepLength;
        }
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
