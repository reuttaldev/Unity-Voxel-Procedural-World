using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnvironmentController : MonoBehaviour
{

    [SerializeField]
    ChunkController chunkController;
    
    private void Start()
    {
        // generate all the chunks and store them in the array before rendering
        // this allows to perform checks on the shared faces between chunks (i.e., check if a face is visible or occluded).
        // all chunks must be generated and present in the array for us to verify whether a shared face exists
        GenerateWorldData();
        RenderWorld();
    }
    private void GenerateWorldData()
    {
        chunkController.DeleteChunks();

        for (var x = 0; x < EnvironmentConstants.worldSizeInChunks; x++)
        {
            for (var z = 0; z < EnvironmentConstants.worldSizeInChunks; z++)
            {
                chunkController.GenerateChunkData(new ChunkPosition(x, z));
            }
        }
    }
    private void RenderWorld()
    {
        for (int x = 0; x < EnvironmentConstants.worldSizeInChunks; x++)
        {
            for(int z = 0; z < EnvironmentConstants.worldSizeInChunks; z++)
            {
                chunkController.InstantiateAndRenderChunk(new ChunkPosition(x, z));
            }
        }
    }

   
}