using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
public class TreeGenerator : MonoBehaviour
{
    private ConcurrentQueue<TreeData> treesToBeGenerated = new ConcurrentQueue<TreeData>();
    [SerializeField]
    private int maxTrunkHeight=7, minTrunkHeight = 2; // in voxel units

    // Location of trees is decided in BiomeController.FillChunkColumn when generating the world. 
    // It will call this method to let us know the location and tree type. 
    // Tree data must be generated after the chunks data is generated, since some elements of a tree can be split between chunks when it is on the wall of a chunk (trunk is in chunk A, some leafs enter chunk B).
    // After chunks data has been filled, we can go ahead and construct the tree. Fill in data to set the shape of it, then render 
    public void AddTree(ChunkData chunk, Vector3Int localPos, VoxelType type,float noise)
    {
        // get the height of the trunk randonly. Using noise and not random because a random (even with a seed) might not give us the same number, since we don't know if we will get to this part of code at exactly the same point in different machines- some are faster than others.
        // but noise will always give us the same value for the same position
        int height = (int)(noise * maxTrunkHeight); 
        if(height > minTrunkHeight)
            height = minTrunkHeight;
        treesToBeGenerated.Enqueue(new TreeData(type,type,localPos,chunk,height));
    }

    public void GenerateTreesData()
    {
        while (treesToBeGenerated.TryDequeue(out TreeData data))
        {
            FillChunkValuesForTree(data);
        }
    }

    private void FillChunkValuesForTree(TreeData data)
    {
        var chunkData = data.parentChunkData;
        var pos = data.localTrunkPosition;
        // create the trunk
        for (int i = 0; i < data.trunkHeight; i++)
        {
            //chunkData[pos.x,pos.y+i,pos.z] = dat;
        }
        // create the leaves. Here, some of the positions may be outside of the tree parent chunk.

        //
    }
}
