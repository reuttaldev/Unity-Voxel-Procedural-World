using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ChunkContoller : SimpleSingleton<ChunkContoller>
{
    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    private BiomeController biomeController;
    [SerializeField]
    private Transform chunksParent;
    [SerializeField]
    public TextureData voxelsTextureData;
    System.Random random = new System.Random();

    public delegate void OnRenderFinishedAction();
    public event OnRenderFinishedAction OnRenderFinished;

    private Dictionary<ChunkPosition, ChunkData> chunks = new Dictionary<ChunkPosition, ChunkData>();
    public ChunkPosition[] CurrentChunkPositions => chunks.Keys.ToArray();
    // have a pool for the chunk gameobject, since instantiating and destroying game objects is expensive 
    private Stack<ChunkRenderer> pool = new Stack<ChunkRenderer>();

    // multithread safe 
    private static ConcurrentQueue<ChunkPosition> positionsToBeRendered = new ConcurrentQueue<ChunkPosition>();
    bool rendering = false;

    /// <summary>
    /// Instantiate (or use from an existing pool) chunk game object
    /// this must be done on the main thread 
    /// </summary>
    private ChunkRenderer InstantiateChunk(ChunkPosition pos)
    {
        ChunkData chunk = new ChunkData();
        chunks[pos] = chunk;
        var chunkGO = Instantiate(chunkPrefab, pos.ToWorldPosition(), Quaternion.identity);
        chunkGO.transform.SetParent(chunksParent);
        chunkGO.name = pos.ToString();
        return chunkGO.GetComponent<ChunkRenderer>();
    }

    // instead of instantiating and deleting the chunks repeatedly, have a pool of them
    public void CreateChunkGO(ChunkPosition newPos)
    {
        ChunkRenderer renderer;
        if (pool.Count == 0)
        {
            renderer = InstantiateChunk(newPos);
        }
        else
        {
            renderer = pool.Pop();
        }
        // in the data, set the game object we created for easy access 
        chunks[newPos] = new ChunkData();
        chunks[newPos].renderer = renderer;
        var chunkGO = renderer.gameObject;
        chunkGO.name = newPos.ToString();
        chunkGO.transform.position = newPos.ToWorldPosition();
    }
    // completely delete the  gameobject
    private void DeleteChunk(ChunkPosition pos)
    {
        var rendrer = chunks[pos].renderer;
        Destroy(rendrer);
        chunks.Remove(pos);
    }

    // add the chunk to the pool of free chunks, since it is no longer used at the moment
    public void RemoveChunk(ChunkPosition pos)
    {
        var rendrer = chunks[pos].renderer;
        rendrer.ClearLastGeneration();
        pool.Push(rendrer);
        chunks.Remove(pos);
    }

    /// <summary>
    /// Generate the chunk data, i.e which voxel type need to be at each position for that chunk
    /// </summary>
    /// <param name="pos"></param>
    public void GenerateChunkData(ChunkPosition pos)
    {
        ChunkData chunk = chunks[pos];
        Vector3Int chunkWorldPos = pos.ToWorldPosition();
        var chunkType = biomeController.GetTypeOfChunk(chunkWorldPos);
        for (int x = 0; x < EnvironmentConstants.chunkWidth; x++)
        {
            for (int z = 0; z < EnvironmentConstants.chunkDepth; z++)
            {
                biomeController.FillChunkColumn(chunk, chunkType, chunkWorldPos, x, z);
            }
        }
    }
    /// <summary>
    /// Generate the chunk mesh data: i.e. which voxel faces need to be visible and with what texture.
    /// Should be done on a separate thread, as calculations can be long and we do not want to block the main thread.
    /// </summary>
    public void GenerateChunkMeshData(ChunkPosition pos)
    {
        if (!chunks.ContainsKey(pos))
        {
            Debug.LogError("Trying to render mesh data without initializing chunk data first.");
            return;
        }
        var renderer = chunks[pos].renderer;
        if (renderer == null)
        {
            Debug.LogError("Trying to generate mesh data without initializing chunk Game Object first.");
            return;
        }
        renderer.GenerateChunkMeshData(chunks[pos], pos);
        positionsToBeRendered.Enqueue(pos);
    }

    private void Update()
    {
        //Render the chunk based on the chunk mesh data that was calculated, if any is ready 
        // Must be done on the main thread as we are setting the game object's properties 
        // So I am doing it spread out over time, to avoid blocking the main thread and to allow other operations to happen while rendering
        if (!rendering && positionsToBeRendered.TryDequeue(out var pos))
        {
            rendering = true;
            //StartCoroutine(RenderChunksSequentially());
            chunks[pos].renderer.Render();
            rendering = false;
        }
    }
    #region TREES
    public void AddTreesData(ChunkPosition pos)
    {
        ChunkData chunk = chunks[pos];
        foreach (TreeData treeData in chunk.TreesData)
        {
            AddTreeTrunk(chunk, treeData);
            AddTreeLeafs(chunk, pos,treeData);
        }
    }
    private void AddTreeTrunk(ChunkData chunk,TreeData data)
    {
        for (int i = 0; i < data.trunkHeight; i++)
        {
            chunk[data.localTrunkPosition.x,data.localTrunkPosition.y+i, data.localTrunkPosition.z] = data.trunkType;
        }
    }
    private void AddTreeLeafs(ChunkData chunk, ChunkPosition chunkPos, TreeData data)
    {
        var leafs = GenerateLeafPositions(data.localTrunkPosition + new Vector3Int(0, data.trunkHeight+data.leafRadius, 0), data.leafRadius);
        foreach (Vector3Int localPos in leafs)
        {
            if (ChunkUtility.ValidLocalVoxelCoordinates(localPos))
            {
                chunk[localPos] = data.leafType;
            }
            else // the voxel that requires checking is not in this specific chunk
            {
                // access the chunk the voxel is in 
                SetVoxelTypeByGlobalPos(localPos + chunkPos.ToWorldPosition(),data.leafType);
            }
        }
    }
    private List<Vector3Int> GenerateLeafPositions(Vector3Int position, int radius)
    {
        Vector3Int offset = new Vector3Int();
        List<Vector3Int> leafPositions= new List<Vector3Int>();
        for (int x = -radius; x <= radius; x++)
        {
            offset.x = x;
            for (int y = -radius; y <= radius; y++)
            {
                offset.y = y ;
                for (int z = -radius; z <= radius; z++)
                {
                    offset.z = z;
                    var currentPos = position + offset;
                    // apply randomness based on distance from trunk
                    float distance = offset.magnitude;
                    float probability = Mathf.Clamp01(0.8f - (0.05f * distance));
                    if (random.NextDouble() < probability)
                    {
                        leafPositions.Add(currentPos);
                    }
                }
            }
        }
        return leafPositions;
    }

    #endregion
    #region GET METHODS
    public VoxelType GetVoxelTypeByGlobalPos(Vector3 voxelGlobalPos)
    {
        // get the position of the chunk that contains this voxel
        var chunkPos = new ChunkPosition(voxelGlobalPos);
        // check if this chunk is in our environment 
        if (chunks.ContainsKey(chunkPos))
        {
            var voxelLocalPos = ChunkUtility.GlobalVoxelPositionToLocal(chunkPos, voxelGlobalPos);
            return chunks[chunkPos][voxelLocalPos];
        }
        // check if the position is at the end of the currently existing world.
        // if it is not, then there is not reason to render it since a new chunk will be generated there sometime.
        // this avoids rendering the chunk's (that are procedurally generated) walls 
        return VoxelType.DontRenderNeighbors;
    }
    /// <summary>
    /// return the elements from l that are not found in our chunk dictionary, order by distance (so we render the chunks that are closest to the player first). 
    /// to array because we want to take a snapshot of the chunks collection as it is, since it might change between iteration as we generate new data
    /// </summary>
    public IEnumerable<ChunkPosition> GetNonExistingChunks( IEnumerable<ChunkPosition> l, Vector3 orderByPos)
    {
        return l.Where(pos => !chunks.ContainsKey(pos)).OrderBy(pos => Vector3.Distance(orderByPos, pos.ToWorldPosition()));
    }
    /// <summary>
    /// return the that are found in our chunk dictionary but not in l. The order does not matter
    /// to array because we will change the Collection while iterating (by deleting) and we cannot do that on IEnumerable
    /// </summary>
    public IEnumerable<ChunkPosition> GetExcessChunks( IEnumerable<ChunkPosition> l)
    {
        return chunks.Keys.Where(pos => !l.Contains(pos));
    }
    #endregion

    #region SET METHODS
    public bool SetVoxelTypeByGlobalPos(Vector3 voxelGlobalPos, VoxelType type)
    {
        // get the position of the chunk that contains this voxel
        var chunkPos = new ChunkPosition(voxelGlobalPos);
        // check if this chunk is in our environment 
        if (chunks.ContainsKey(chunkPos))
        {
            var voxelLocalPos = ChunkUtility.GlobalVoxelPositionToLocal(chunkPos, voxelGlobalPos);
            chunks[chunkPos][voxelLocalPos] = type;
            return true;
        }
        return false;
    }
    #endregion
}
