using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    [SerializeField]
    private BiomeController biomeController;
    private Dictionary<ChunkPosition, TreeData> trees = new Dictionary<ChunkPosition, TreeData>();

    public void GenerateChunkTreeData(ChunkPosition pos)
    {
        TreeData treeData = new TreeData();
        FillChunkValues(treeData, pos.ToWorldPosition());
        trees[pos] = treeData;
    }

    private void FillChunkValues(TreeData data, Vector3 chunkWorldPos)
    {
        var chunkType = biomeController.GetTypeOfChunk(chunkWorldPos);
        for (int x = 0; x < EnvironmentConstants.chunkWidth; x++)
        {
            for (int z = 0; z < EnvironmentConstants.chunkDepth; z++)
            {
                biomeController.PlaceTrees(data, chunkType, chunkWorldPos, x, z);
            }

        }
    }
}
