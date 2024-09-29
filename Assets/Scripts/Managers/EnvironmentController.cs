using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class EnvironmentController : MonoBehaviour, IRegistrableService
{
    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    public VoxelsData voxelsData;
    private Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    private void Awake()
    {
        ServiceLocator.Instance.Register<EnvironmentController>(this);
    }

    private void Start()
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        foreach (var chunk in chunks.Values)
        {
            Destroy(chunk.gameObject);
        }
        chunks.Clear();

        for (var x = 0; x < EnvironmentConstants.worldSizeInChunks; x++)
        {
            for (var z = 0; z < EnvironmentConstants.worldSizeInChunks; z++)
            {
                GenerateChunk(new Vector3Int(x * EnvironmentConstants.chunkWidth, 0, z * EnvironmentConstants.chunkDepth));
            }
        }
    }
    private void GenerateChunk(Vector3Int worldPos)
    {
        var chunkGO = Instantiate(chunkPrefab, worldPos, Quaternion.identity);
        Chunk chunk = chunkGO.GetComponent<Chunk>();
        chunks[worldPos] = chunk;
        ChunksUtility.FillChunkValues(chunk, worldPos);
        ChunkRenderer.Render(chunk);
    }

    public VoxelType GetVoxelTypeByGlobalPosition(Vector3Int voxelGlobalPos)
    {
        var chunkPos = ChunksUtility.GetChunkPositionByVoxelPosition(voxelGlobalPos);
        if (chunks.ContainsKey(chunkPos))
        {
            var voxelLocalPos = ChunksUtility.GlobalVoxelPositionToLocal(chunkPos, voxelGlobalPos);
            return chunks[chunkPos][voxelLocalPos];
        }
        return VoxelType.Empty;
    }
}