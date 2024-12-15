using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A biome represents a distinct environmental area within the game world.
/// The BiomeController manages the generation and behavior of the different biomes.
/// </summary>
public class BiomeController : MonoBehaviour
{
    [SerializeField]
    private List<BiomeSettings> biomesSettings;
    [SerializeField]
    private TreeGenerator treeGenerator;

    public BiomeType GetTypeOfChunk(Vector3 chunkWorldPos)
    {
        return BiomeType.Forest;
    }
    public void FillChunkColumn(ChunkData chunk, BiomeType type, Vector3 chunkWorldPos, int columnX, int columnZ)
    {
        var settings = biomesSettings[(byte)type];
        int groundHeight = NoiseUtility.GetNormalizedNoise(chunkWorldPos.x + columnX, chunkWorldPos.z + columnZ, settings.noise);
        for (int y = 0; y < EnvironmentConstants.chunkHeight; y++)
        {
            chunk[columnX, y, columnZ] = DecideVoxelTypeByY(y, groundHeight, settings);
        }

        // now, check if this column should contain a tree
        float treeNoise = NoiseUtility.GetNoise(chunkWorldPos.x + columnX, chunkWorldPos.z + columnZ, settings.treeNoise);
        //Debug.Log(treeNoise);
        VoxelType treeType = DecideTreeTypeByThreshold(treeNoise, settings.treeThreshold);
        if(treeType != VoxelType.Empty)
        {
            var treePos = new Vector3Int(columnX, groundHeight + 1, columnZ);
            Debug.Log(treePos);
            treeGenerator.AddTree(chunk,treePos,treeType,treeNoise);
        }
    }
    /// Determines the appropriate VoxelType based on the Y coordinate (height) of the voxel and the biome it is in.
    private VoxelType DecideVoxelTypeByY(int y, int groundPos, BiomeSettings biomeSettings)
    {
        if (y > groundPos)
        {
            if (y <biomeSettings.waterThreshold)
            {
                if (y == groundPos +1)
                    return biomeSettings.nearWaterVoxel;
                return VoxelType.Water;
            }
            else
                return VoxelType.Empty;
        }
        // voxels that are under ground pos cannot be empty, because then the pieces that are in ground pos think there is nothing underneath them and should be rendered 
        else if (y < groundPos)
        {
            return biomeSettings.underWaterVoxel;
        }
        // equals = ground position
        return biomeSettings.topVoxel;
    }
    private VoxelType DecideTreeTypeByThreshold(float noiseValue, float threshold)
    {
        if (noiseValue > threshold)
            return VoxelType.TreeTrunk;
        return VoxelType.Empty;
    }

}

