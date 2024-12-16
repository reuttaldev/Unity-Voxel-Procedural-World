
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;
using Debug = UnityEngine.Debug;


public class EndlessEnvController : MonoBehaviour
{
    [SerializeField]
    private Transform player;
    [SerializeField]
    private ChunkContoller chunkController;
    [SerializeField]
    // time passed between checks to see if more chunks need to be generated 
    private float restDuration = 2f,time = 0 ;
    private bool generating = false;

    // the position of the chunk the player was on during our last check
    private ChunkPosition playerCurrentChunk,playerLastChunk; 
    // as the player moves we need to create the chunks at different locations. this offset tells us where 
    private Vector2Int worldOffset = new Vector2Int(0,0);

    // following https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation
    CancellationTokenSource taskTokenSource = new CancellationTokenSource();


    private void Start()
    {
        var initPoses = ChunkUtility.GetChunkPositionsAroundPos(new ChunkPosition(Vector3.zero)).ToArray();
        GenerateWorld(initPoses);
        var middlePos = initPoses[0].ToWorldPosition() + new Vector3(EnvironmentConstants.chunkWidth/2,15, EnvironmentConstants.chunkDepth / 2);
        player.position = middlePos;
        player.gameObject.SetActive(true);  
        playerCurrentChunk = new ChunkPosition(middlePos);
    }
    private async Task GenerateWorld(ChunkPosition[] poses)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        generating = true;
        // Step 1: Instantiate (or use from an existing pool) chunk game object, so we have something to apply the mesh to
        // must be done on main thread since we are changing game object properties
        foreach (var pos in poses)
        {
            chunkController.CreateChunkGO(pos);
        }
        // Step 2: Generate the chunk data, i.e which voxel type need to be at each position for that chunk
        // all chunks must be generated and present in the array before creating the meshes, to allow checks on shared faces (i.e., check if a face needs to be visible or occluded)
        await GenerateWorldData(poses);
        //Step 3: Generate the chunk mesh data: i.e. which voxel faces need to be visible and with what texture.
        // must be done after generating the game objects since the renderer is a component of the game object.
        // need to change this- so it can be done in parallel 
        var meshGeneration= GenerateWorldMeshData(poses);

        /// Step 4: Render the chunk based on the chunk mesh data that was calculated previously.
        /// again,must be done on the main thread. 
        /// It will start automatically in chunk controller Update method when there is something to render
        
        await meshGeneration;
        generating = false;

        stopwatch.Stop();
        Debug.Log($"Generation took: {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Procedurally generate based on the players position
    /// </summary>
    private async void Update()
    {
        if(Keyboard.current.vKey.isPressed)
        {
            ReGenerateWorld();
        }
        // wait for seconds
        time += Time.deltaTime;
        if (time <= restDuration)
            return;
        time = 0;

        playerCurrentChunk = new ChunkPosition(player.position);
        if (!generating && !Equals(playerCurrentChunk, playerLastChunk))
        {
            playerLastChunk = playerCurrentChunk;
            UpdateWorld();
        }
    }
    private async void UpdateWorld()
    {
        // I do .ToArray on the returned Ienumerables  because the player is moving in the background, and that causes changes the dictionaries to change. So the result may change from the time the request was called until it is evaluated.
        ChunkPosition[] surroundingPlayerChunks = ChunkUtility.GetChunkPositionsAroundPos(playerCurrentChunk).ToArray();
        // get the chunks that are surrounding the player, but don't already exist
        ChunkPosition[] chunksToCreate = chunkController.GetNonExistingChunks(surroundingPlayerChunks, player.position).ToArray();
        // get the chunks that used to surround the player, but don't anymore
        ChunkPosition[] chunksToDelete = chunkController.GetExcessChunks(surroundingPlayerChunks).ToArray();

        foreach (ChunkPosition chunkPosition in chunksToDelete)
        {
            chunkController.RemoveChunk(chunkPosition);
        }
        await GenerateWorld(chunksToCreate);
    }
    /// <summary>
    /// Thread 1: dedicated for generating world data.
    /// The methods executed here determine which voxel type (grass, send etc) need to be at each within a chunk. 
    /// The thread creates new chunk data as they are requested based on player movement; once the player enters an area that does not exist yet and needs to be procedurally generated, new chunk data requests will come in. 
    /// </summary>

    private Task GenerateWorldData(IEnumerable<ChunkPosition> chunkPositions)
    {
        return Task.Run(() =>
        {
            foreach (var pos in chunkPositions)
            {
                chunkController.GenerateChunkData(pos);
            }
            // after all chunk data has been filled, start generating the trees. 
            // tree data must be generated after the chunks data, since some elements of a tree can be split between chunks when it is on the wall of a chunk (trunk is in chunk A, some leafs enter chunk B).
            foreach (var pos in chunkPositions)
            {
                chunkController.AddTreesData(pos);
            }
        }
        , taskTokenSource.Token
        );
    }
    /// <summary>
    /// Thread 2: Dedicated to generating mesh data based on the world data. 
    /// Once all chunk data has finished generating on Thread 1, prepere the mesh for renduring by deciding which faces should be visible and assign textures.
    /// </summary>
    private Task GenerateWorldMeshData(IEnumerable<ChunkPosition> chunkPositions)
    {
        return Task.Run(() =>
        {
            foreach (var pos in chunkPositions)
            {
                chunkController.GenerateChunkMeshData(pos);
            }
        }
        , taskTokenSource.Token
        );
    }

    // for testing 
    private async void ReGenerateWorld()
    {
        ChunkPosition[] current = chunkController.CurrentChunkPositions;
        UnityEngine.Debug.Log(current.Length);
        foreach (ChunkPosition chunkPosition in current)
        {
            chunkController.RemoveChunk(chunkPosition);
        }
        await GenerateWorld(current);
    }

    void OnDisable()
    {
        taskTokenSource.Cancel();
        taskTokenSource.Dispose();
    }
}