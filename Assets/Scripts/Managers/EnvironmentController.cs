using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private ChunkController chunkController;

    // time passed between checks to see if more chunks need to be generated 
    private float checkTime = 1;
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

   private void PlacePlayer()
    {
        float midX = (EnvironmentConstants.worldSizeInChunks * EnvironmentConstants.chunkWidth)/2;
        float midZ = (EnvironmentConstants.worldSizeInChunks * EnvironmentConstants.chunkDepth)/2;
        Debug.Log(midX);
        Vector3 worldMidPoint = new Vector3(midX, EnvironmentConstants.chunkHeight+10 , midX);
        // need to find the y position, to place the player at 
        Debug.DrawRay(worldMidPoint, Vector3.down * EnvironmentConstants.chunkHeight * 2, Color.red, 2000.0f); // Duration is 2 seconds
        RaycastHit hit;
        if (Physics.Raycast(worldMidPoint, Vector3.down, out hit, EnvironmentConstants.chunkHeight*2))
        {
            player.transform.position = hit.point;
        }
        else
            Debug.LogError("Could not find position to player the player at");
   }

    private void LoadMoreChunks()
    {
        Debug.Log("Loading more chunks");
    }
}