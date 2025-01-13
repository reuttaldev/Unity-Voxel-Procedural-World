using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A biome represents a distinct environmental area within the game world.
/// The BiomeController manages the generation and behavior of the different biomes.
/// </summary>
public class BiomeController : MonoBehaviour
{
    [SerializeField]
    private List<BiomeSettings> biomesSettings;

    public BiomeType GetTypeOfChunk(Vector3 chunkWorldPos)
    {
        return BiomeType.Forest;
    }
    public void FillChunkColumn(ChunkData chunk, BiomeType biomeType, Vector3 chunkWorldPos, int columnX, int columnZ)
    {
        var settings = biomesSettings[(byte)biomeType];
        var globalColumnPos = new Vector2(chunkWorldPos.x + columnX, chunkWorldPos.z + columnZ);
        // the noise is normalized to be between 0 and chunk height
        int groundHeight = NoiseUtility.GetNormalizedNoise(globalColumnPos, settings.noise);

        // have a secondary noise to determine whether this column should include e.g. rocks
        // using noise and not a random number to determine probability because a random(even with a seed) might not give us the same number, since we don't know if we will get to this part of code at exactly the same point in different machines- some are faster than others.
        // but noise will always give us the same value for the same position
        float secondaryNoise = NoiseUtility.GetNoise(globalColumnPos, settings.secondaryNoise);
        // equivalent to setting a prob p for a column being store (threshold), generating random num (noise), if it is greater than make it stone 
        bool isStone = secondaryNoise > settings.stoneThreshold;

        bool columnContainsWater = false;
        for (int y = 0; y < EnvironmentConstants.chunkHeight; y++)
        {
            //if noise is above the threshold, then this whole column is stone (below groundHeight - everything above that is either air or water) 
            if (isStone && y <= groundHeight)
                chunk[columnX, y, columnZ] = settings.stoneVoxel;
            else
            {
                var type = DecideVoxelTypeByY(y, groundHeight, settings);
                if(type == VoxelType.Water || type == VoxelType.Dark_Water)
                    columnContainsWater = true;
                chunk[columnX, y, columnZ] = type;
            }
        }

        if (!columnContainsWater)
        {
            // now, check if this column should contain a tree
            float treeNoise = NoiseUtility.GetNoise(globalColumnPos, settings.treeNoise);
            if (treeNoise > settings.treeThreshold)
            {
                chunk.AddTreeData(DecideTreeType(treeNoise, new Vector3Int(columnX, groundHeight + 1, columnZ), settings));
            }
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
                return biomeSettings.waterVoxel;
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

    private TreeData DecideTreeType(float noiseVal, Vector3Int localTrunkPos, BiomeSettings biomeSettings)
    {
        // get the height of the trunk randonly (by noise- so it is alwasy the same value for the same position.
        // since the noise is not necessarily between 0 and 1, take the decimal part 
        int height = (int)(noiseVal * biomeSettings.maxTrunkHeight);
        if (height < biomeSettings.minTrunkHeight)
            height = biomeSettings.minTrunkHeight;
        int radius = localTrunkPos.x % 2 ==0 ? 1 : 2;
        return new TreeData(VoxelType.Dark_Trunk, VoxelType.Pink_Leafs_B, localTrunkPos, height, radius);
    }
}

