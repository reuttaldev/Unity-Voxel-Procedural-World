using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField]
    private GameObject chunkPrefab;

    private void Start()
    {
        CreateFullChunk();
    }

    private void CreateFullChunk()
    {
        ChunkData chunk = new ChunkData();
        for (int x = 0; x < EnvironmentConstants.chunkWidth; x++)
        {
            for (int y = 0; y < EnvironmentConstants.chunkDepth; y++)
            {
                for(int z = 0; z < EnvironmentConstants.chunkHeight; z++)
                {
                    chunk[x,y,z] = VoxelType.Dirt;
                }
            }
        }
        var chunkGO = Instantiate(chunkPrefab);
        var chunkRenderer = chunkGO.GetComponent<ChunkRenderer>();
        chunkRenderer.Render(chunk);

    }
}
