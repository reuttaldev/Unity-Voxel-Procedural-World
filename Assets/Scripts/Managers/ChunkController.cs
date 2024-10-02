using System.Collections;
using System.Collections.Generic;
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

    private ChunkData[,] chunks = new ChunkData[EnvironmentConstants.worldSizeInChunks, EnvironmentConstants.worldSizeInChunks];

    public void GenerateChunkData(ChunkPosition pos)
    {
        Debug.Log("Generating chunk at position " + pos);

        ChunkData chunk = new ChunkData();
        FillChunkValues(chunk, pos.ToWorldPosition());

        chunks[pos.x, pos.z] = chunk;
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
        var chunkGO = Instantiate(chunkPrefab, pos.ToWorldPosition(), Quaternion.identity);
        chunkGO.transform.SetParent(chunksParent);
        chunkGO.name = pos.ToString();
        ChunkRenderer renderer = chunkGO.GetComponent<ChunkRenderer>();
        renderer.Render(chunks[pos.x, pos.z], this);
    }

    public void DeleteChunks()
    {
        foreach (Transform child in chunksParent)
        {
            Destroy(child.gameObject);
        }
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
            return chunks[chunkPos.x, chunkPos.z][voxelLocalPos];
        }
        return VoxelType.Empty;
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
