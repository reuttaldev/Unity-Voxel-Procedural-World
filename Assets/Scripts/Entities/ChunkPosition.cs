using UnityEngine;

/// <summary>
/// The world position is not represented in Unity's world coordinates; 
/// instead, it is based on a counting system that tracks the positions of chunks.
/// </summary>
public struct ChunkPosition
{
    public int x { get; }
    public int z { get; }

    public ChunkPosition(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    public ChunkPosition(Vector3 v)
    {
        this.x = Mathf.FloorToInt(v.x / EnvironmentConstants.chunkWidth);
        this.z = Mathf.FloorToInt(v.z / EnvironmentConstants.chunkDepth);
    }
    public override string ToString()
    {
        return $"({x} , {z})";
    }

    /// <summary>
    /// Transform the chunk system coordinates to Unity's regular world coordinates
    /// </summary>
    /// <returns></returns>
    public Vector3Int ToWorldPosition()
    {
        return new Vector3Int(x * EnvironmentConstants.chunkWidth, 0, z * EnvironmentConstants.chunkDepth);
    }
    public bool IsValid()
    {
        return x < EnvironmentConstants.worldSizeInChunks && z < EnvironmentConstants.worldSizeInChunks && x >= 0 && z >= 0;

    }
}