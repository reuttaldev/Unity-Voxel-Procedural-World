using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


public class EndlessEnvController : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private ChunkGenerator chunkController;
    [SerializeField]
    private float restDuration = 2f,time = 0 ;
    private bool generating = false;
    // time passed between checks to see if more chunks need to be generated 
    //private float updateTime = 3;
    // the position of the chunk the player was on during our last check
    private ChunkPosition playerCurrentChunk,playerLastChunk; 
    // as the player moves we need to create the chunks at different locations. this offset tells us where 
    private Vector2Int worldOffset = new Vector2Int(0,0);
    private void Start()
    {
        var initPoses = ChunkUtility.GetInitChunksPositions();
        GenerateWorld(initPoses);
        PlacePlayer();
    }
    private void GenerateWorld(ChunkPosition[] poses)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        generating = true;
        // Step 1: Generate the chunk data, i.e which voxel type need to be at each position for that chunk
        // This is done before rendering to allow checks on the shared faces between chunks (i.e., check if a face is visible or occluded).
        // all chunks must be generated and present in the array for us to verify whether a shared face exists
        GenerateWorldData(poses);
        // Step 2: Instantiate (or use from an existing pool) chunk game object, so we have something to apply the mesh to
        // By unity specifications, instantiation must be done on the main thread 
        foreach (var pos in poses)
        {
            chunkController.InstantiateChunkGO(pos);
        }

        //Step 3: Generate the chunk mesh data: i.e. which voxel faces need to be visible and with what texture.
        // must be done after generating the game objects since the renderer is a component of the game object.
        // need to change this- so it can be done in parallel 
        GenerateWorldMeshData(poses);

        /// Step 4: Render the chunk based on the chunk mesh data that was calculated previously.
        /// again,must be done on the main thread
        //StartCoroutine(chunkController.RenderChunksSequentially(poses));
        chunkController.RenderChunks(poses);
        generating = false;
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Generation took: {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Procedurally generate based on the players position
    /// </summary>
    private void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            var initPoses = ChunkUtility.GetInitChunksPositions();
            GenerateWorld(initPoses);
        }

        // wait for seconds
        time += Time.deltaTime;
        if (time <= restDuration)
            return;
        time = 0;

        playerCurrentChunk = new ChunkPosition(player.position);
        // we remain on the same chunk, no need to generate more 
        if (Equals(playerCurrentChunk, playerLastChunk))
        {
            return;
        }
        if (generating == false)
        {
            playerLastChunk = playerCurrentChunk;
            UpdateWorld();

        }
    }
    private void UpdateWorld()
    {
        // I do .ToArray on the returned Ienumerables  because the player is moving in the background, and that causes changes the dictionaries to change. So the result may change from the time the request was called until it is evaluated.
        ChunkPosition[] surroundingPlayerChunks = ChunkUtility.GetChunkPositionsAroundPos(playerCurrentChunk).ToArray();
        // get the chunks that are surrounding the player, but don't already exist
        ChunkPosition[] chunksToCreate = chunkController.GetNonExistingChunks(surroundingPlayerChunks, player.position).ToArray();
        // get the chunks that used to surround the player, but don't anymore
        ChunkPosition[] chunksToDelete = chunkController.GetExcessChunks(surroundingPlayerChunks).ToArray();

        foreach (ChunkPosition chunkPosition in chunksToDelete)
        {
            chunkController.DeleteChunk(chunkPosition);
        }
        GenerateWorld(chunksToCreate);
    }
    private void GenerateWorldData(IEnumerable<ChunkPosition> chunkPositions)
    {
            foreach (var pos in chunkPositions)
            {
                chunkController.GenerateChunkData(pos);       
            }

    }
    private void GenerateWorldMeshData(IEnumerable<ChunkPosition> chunkPositions)
    {
            foreach (var pos in chunkPositions)
            {
                chunkController.GenerateChunkMeshData(pos);
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
            player.gameObject.SetActive(true);  
            player.position = hit.point;
            playerLastChunk = new ChunkPosition(hit.point);
        }
        else
            UnityEngine.Debug.LogError("Could not find position to player the player at");
   }

}