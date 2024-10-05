using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    private BiomeController biomeController;
    [SerializeField]
    private Transform chunksParent;
    [SerializeField]
    public VoxelsTextureData voxelsTextureData;

    private Dictionary<ChunkPosition,ChunkData> chunks = new Dictionary<ChunkPosition, ChunkData>();

    public void GenerateChunkData(ChunkPosition pos)
    {
        Debug.Log("Generating chunk at position " + pos);
        ChunkData chunk = new ChunkData();
        FillChunkValues(chunk, pos.ToWorldPosition());
        chunks[pos] = chunk;
    }

    private void FillChunkValues(ChunkData chunk, Vector3 chunkWorldPos)
    {
        for (int x = 0; x < EnvironmentConstants.chunkWidth; x++)
        {
            for (int z = 0; z < EnvironmentConstants.chunkDepth; z++)
            {
                biomeController.FillChunkColumn(chunk, chunkWorldPos, x, z);
            }
        }
    }
    public void InstantiateAndRenderChunk(ChunkPosition pos)
    {
        Debug.Log("Rendering chunk at position " + pos);
        ChunkData chunk = chunks[pos];
        var chunkGO = Instantiate(chunkPrefab, pos.ToWorldPosition(), Quaternion.identity);
        chunkGO.transform.SetParent(chunksParent);
        chunkGO.name = pos.ToString();
        chunk.gameObject = chunkGO;
        ChunkRenderer renderer = chunkGO.GetComponent<ChunkRenderer>();
        renderer.Render(chunk, this);
    }

    public void DeleteChunk(ChunkPosition pos)
    {
        if (chunks.TryGetValue(pos, out var chunk))
        {
            Destroy(chunk.gameObject);
            chunk.gameObject = null;
            chunks.Remove(pos);
        }
        else
            Debug.LogError("Trying to delete chunk in position that does not exist");
    }
    public VoxelType GetVoxelTypeByGlobalPos(Vector3 voxelGlobalPos)
    {
        if (voxelGlobalPos.y < 0)
            return VoxelType.Empty;
        // get the position of the chunk that contains this voxel
        var chunkPos = new ChunkPosition(voxelGlobalPos);
        if (chunkPos.IsValid())
        {
            var voxelLocalPos = ChunkUtility.GlobalVoxelPositionToLocal(chunkPos, voxelGlobalPos);
            return chunks[chunkPos][voxelLocalPos];
        }
        return VoxelType.Empty;
    }

    /// <summary>
    /// return the elements from l that are not found in our chunk dictionary, order by distance (so we render the chunks that are closest to the player first). 
    /// to array because we want to take a snapshot of the chunks collection as it is, since it might change between iteration as we generate new data
    /// </summary>
    public ChunkPosition[] GetNonExistingChunks(IEnumerable<ChunkPosition> l, Vector3 orderByPos)
    {
        return l.Where(pos => !chunks.ContainsKey(pos)).OrderBy(pos=> Vector3.Distance(orderByPos, pos.ToWorldPosition())).ToArray();
    }
    /// <summary>
    /// return the that are found in our chunk dictionary but not in l. The order does not matter
    /// to array because we will change the Collection while iterating (by deleting) and we cannot do that on IEnumerable
    /// </summary>
    public ChunkPosition[] GetExcessChunks(IEnumerable<ChunkPosition> l)
    {
        return chunks.Keys.Where(pos => !l.Contains(pos)).ToArray();
    }
}
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
