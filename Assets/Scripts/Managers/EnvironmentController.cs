using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour, IRegistrableService
{
    [SerializeField]
    private GameObject chunkPrefab;
    [SerializeField]
    public VoxelsData voxelsData;
    private void Awake()
    {
        ServiceLocator.Instance.Register<EnvironmentController>(this);
    }

    private void Start()
    {
        CreateFullChunk();
        //CreateOneVoxelChunk();
    }

    private void CreateOneVoxelChunk()
    {
        ChunkData chunk = new ChunkData();
        chunk[0,0,0] = VoxelType.Dark_Sand;
        var chunkGO = Instantiate(chunkPrefab);
        var chunkRenderer = chunkGO.GetComponent<ChunkRenderer>();
        chunkRenderer.Render(chunk);
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
                    chunk[x,y,z] = VoxelType.Grass;
                }
            }
        }
        var chunkGO = Instantiate(chunkPrefab);
        var chunkRenderer = chunkGO.GetComponent<ChunkRenderer>();
        chunkRenderer.Render(chunk);

    }
}
