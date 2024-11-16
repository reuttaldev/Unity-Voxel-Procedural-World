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
    private List<NoiseSettings> treesNoiseSettings;

    public BiomeType GetTypeOfChunk(Vector3 chunkWorldPos)
    {
        return BiomeType.Forest;
    }
    public void FillChunkColumn(ChunkData chunk, BiomeType type, Vector3 chunkWorldPos, int columnX, int columnZ)
    {
        var settings = biomesSettings[(byte)type];
        int groundHeight = NoiseUtility.GetNoise(chunkWorldPos.x + columnX, chunkWorldPos.z + columnZ, settings.noise);
        for (int y = 0; y < EnvironmentConstants.chunkHeight; y++)
        {
            chunk[columnX, y, columnZ] = DecideVoxelTypeByY(y, groundHeight, settings);
        }
    }
    public void PlaceTrees(TreeData data, BiomeType type, Vector3 chunkWorldPos, int columnX, int columnZ)
    {
        var noiseSettings = treesNoiseSettings[(byte)type];
        var settings = biomesSettings[(byte)type];

        // get a random noise at x,z position
        int treeThreshold = NoiseUtility.GetNoise(chunkWorldPos.x + columnX, chunkWorldPos.z + columnZ, noiseSettings);
        // if the noise is greater than a pre-set value (tree threshold) for this biome, then add a tree there
        if(treeThreshold > settings.treeThreshold)
        {

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
        else if (y < groundPos)
        {
            return biomeSettings.underGroundVoxel;
        }
        // equals = ground position
        return biomeSettings.topVoxel;
    }
}

