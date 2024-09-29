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
    }

    private void CreateFullChunk()
    {
        var chunkGO = Instantiate(chunkPrefab);
        Chunk chunk = chunkGO.GetComponent<Chunk>();
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
        ChunkRenderer.Render(chunk);

    }
}
