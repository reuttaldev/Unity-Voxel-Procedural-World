using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class ChunkGenerator : SimpleSingleton<ChunkGenerator>   
{
    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    private BiomeController biomeController;
    [SerializeField]
    private Transform chunksParent;
    [SerializeField]
    public VoxelsTextureData voxelsTextureData;

    public delegate void OnRenderFinishedAction();
    public event OnRenderFinishedAction OnRenderFinished;

    private Dictionary<ChunkPosition,ChunkData> chunks = new Dictionary<ChunkPosition, ChunkData>();
    private Dictionary<ChunkPosition,ChunkRenderer> chunksRenderers = new Dictionary<ChunkPosition, ChunkRenderer>();
    private Stack<ChunkRenderer> freeChunks= new Stack<ChunkRenderer>();

    /// <summary>
    /// Step 1: Generate the chunk data, i.e which voxel type need to be at each position for that chunk
    /// </summary>
    /// <param name="pos"></param>
    public void GenerateChunkData(ChunkPosition pos)
    {
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
    /// <summary>
    /// Step 2: Instantiate (or use from an existing pool) chunk game object
    /// this must be done on the main thread 
    /// </summary>
    public void InstantiateChunkGO(ChunkPosition pos)
    {
        ChunkData chunkData = chunks[pos];
        var chunkGO = Instantiate(chunkPrefab, pos.ToWorldPosition(), Quaternion.identity);
        chunkGO.transform.SetParent(chunksParent);
        chunkGO.name = pos.ToString();
        // in the data, set the game object we created for easy access 
        ChunkRenderer renderer = chunkGO.GetComponent<ChunkRenderer>();
        chunksRenderers[pos] = renderer;
    }

    // instead of instantiating and deleting the chunks repeatedly, have a pool of them
    public void CreateChunkGO(ChunkPosition newPos)
    {
        if(freeChunks.Count == 0)
        {
            InstantiateChunkGO(newPos);
            return;
        }
        var rendrer = freeChunks.Pop();
        chunksRenderers[newPos] = rendrer;

        var chunkGO = rendrer.gameObject;
        chunkGO.name = newPos.ToString();
        chunkGO.transform.position = newPos.ToWorldPosition();
    }
    // completely delete the  gameobject
    public void DeleteChunk(ChunkPosition pos)
    {
        chunks.Remove(pos);
        var rendrer = chunksRenderers[pos];
        chunksRenderers.Remove(pos);
        Destroy(rendrer);
    }

    // add the chunk to the pool of free chunks, since it is no longer used at the moment
    public void RemoveChunk(ChunkPosition pos)
    {
        var rendrer = chunksRenderers[pos];
        rendrer.ClearLastGeneration();
        freeChunks.Push(rendrer);
        chunksRenderers.Remove(pos);
        chunks.Remove(pos);
    }

    /// <summary>
    /// Step 3: Generate the chunk mesh data: i.e. which voxel faces need to be visible and with what texture.
    /// Should be done on a separate thread, as calculations can be long and we do not want to block the main thread.
    /// </summary>
    public void GenerateChunkMeshData(ChunkPosition pos)
    {
        if (!chunks.ContainsKey(pos))
        {
            Debug.LogError("Trying to render mesh data without initializing chunk data first.");
            return;
        }
        if (!chunksRenderers.ContainsKey(pos))
        {
            Debug.LogError("Trying to render mesh data without initializing chunk Game Object first.");
            return;
        }
        var renderer = chunksRenderers[pos];
        renderer.GenerateChunkMeshData(chunks[pos],pos);

    }

    /// <summary>
    /// Step 4: Render the chunk based on the chunk mesh data that was calculated previously.
    /// Must be done on the main thread as we are setting the game object's properties 
    /// </summary>
    public void RenderChunks(ChunkPosition[] poses)
    {
        for (int i = 0; i < poses.Length; i++)
        {
            chunksRenderers[poses[i]].Render();
        }
        OnRenderFinished?.Invoke();
    }
    public IEnumerator RenderChunksSequentially(ChunkPosition[] poses)
    {
        for(int i = 0; i< poses.Length; i++)
        {
            chunksRenderers[poses[i]].Render();   
            yield return new WaitForEndOfFrame();
        }
        OnRenderFinished?.Invoke(); 
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
}
