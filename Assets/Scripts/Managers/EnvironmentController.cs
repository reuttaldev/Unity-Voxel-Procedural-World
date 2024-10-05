using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private ChunkController chunkController;

    // time passed between checks to see if more chunks need to be generated 
    //private float updateTime = 3;
    // the position of the chunk the player was on during our last check
    private ChunkPosition playerLastChunk; 
    // as the player moves we need to create the chunks at different locations. this offset tells us where 
    private Vector2Int worldOffset = new Vector2Int(0,0);

    private void Start()
    {
        // generate all the chunks and store them in the array before rendering
        // this allows to perform checks on the shared faces between chunks (i.e., check if a face is visible or occluded).
        // all chunks must be generated and present in the array for us to verify whether a shared face exists
        GenerateWorldData();
        RenderWorld();
        PlacePlayer();
    }
    private void GenerateWorldData()
    {
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

   private void PlacePlayer()
    {
        float midX = (EnvironmentConstants.worldSizeInChunks * EnvironmentConstants.chunkWidth)/2;
        float midZ = (EnvironmentConstants.worldSizeInChunks * EnvironmentConstants.chunkDepth)/2;
        Vector3 worldMidPoint = new Vector3(midX, EnvironmentConstants.chunkHeight+10 , midZ);
        // need to find the y position, to place the player at 
        RaycastHit hit;
        if (Physics.Raycast(worldMidPoint, Vector3.down, out hit, EnvironmentConstants.chunkHeight))
        {
            player.position = hit.point;
            playerLastChunk = new ChunkPosition(hit.point);
        }
        else
            Debug.LogError("Could not find position to player the player at");
   }

    private void UpdateWorld(ChunkPosition playerCurrentChunk)
    {
        Debug.Log("Loading more chunks");
        playerLastChunk = playerCurrentChunk;
        IEnumerable<ChunkPosition> surroundingPlayerChunks = ChunkUtility.GetChunkPositionsAroundPos(playerCurrentChunk);
        // get the chunks that are surrounding the player, but don't already exist
        ChunkPosition[] chunksToCreate = chunkController.GetNonExistingChunks(surroundingPlayerChunks, player.position);
        // get the chunks that used to surround the player, but don't anymore
        ChunkPosition[] chunksToDelete = chunkController.GetExcessChunks(surroundingPlayerChunks).ToArray();

        foreach (ChunkPosition chunkPosition in chunksToCreate)
        {
            chunkController.GenerateChunkData(chunkPosition);
        }
        foreach (ChunkPosition chunkPosition in chunksToCreate)
        {
            chunkController.InstantiateAndRenderChunk(chunkPosition);
        }
        foreach(ChunkPosition chunkPosition in chunksToDelete)
        {
            chunkController.DeleteChunk(chunkPosition);
        }
    }

    /// <summary>
    /// Procedurally generate based on the players position
    /// </summary>
    private void Update()
    {
        // wait for seconds 
        ChunkPosition playerCurrentChunk = new ChunkPosition(player.position);
        // we remain on the same chunk, no need to generate more 
        if (Equals(playerCurrentChunk, playerLastChunk))
        {
            return; 
        }
        UpdateWorld(playerCurrentChunk);    
    }
}