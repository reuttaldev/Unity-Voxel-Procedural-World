using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for the calculations needed before applying textures (i.e., images) to the mesh 
/// created for each vertex, so the environment appears textured rather than just white cubes.
/// Handle a texture atlas, where multiple textures are stacked  in a single image.
/// </summary>
public static class TextureUtility 
{
    /// The image file (atlas) contains all the textures for the various voxel types. This is the size of a texture for one voxel
    private const int texturePixelSize = 16;
    /// size of the entire image (atlas)
    private const int atlasPixelSizeX = 576;
    private const int atlasPixelSizeY = 544;
    private const double normalizedTextureSizeX = (double)texturePixelSize / atlasPixelSizeX;
    private const double normalizedTextureSizeY = (double)texturePixelSize / atlasPixelSizeY;


    /// <summary>
    /// Each element in the UV list is a Vector2 representing the texture coordinate in the atlas
    /// for the corresponding vertex at the same index in the vertices list.
    /// Each face uses a set of 4 coordinates corresponding to the vertices in the face .
    /// This defines which part of the atlas is mapped to each vertex.
    /// </summary>
    public static Vector2[] GetTexturePositionInAtlas(int face, VoxelData voxelData)
    {

        Vector2[] texturePosition = new Vector2[EnvironmentConstants.vertexNoDupCount];
        //each vortex has a texture for its walls, and a texture for the top and the bottom. 
        //Faces are processed in the following order: 0 = Back Face, 1= Front Face,2 = Top Face
        //3= Bottom Face, 4=  Left Face, 5 = Right Face
        // find the bottom left corner of the requested texture in the atlas
        var textureStartPosition = (face == 2 || face == 3) ? GetBlockStartCoordinate(voxelData.TopTexturePosition) : GetBlockStartCoordinate(voxelData.SideTexturePosition);

        for (int i = 0; i < EnvironmentConstants.vertexNoDupCount; i++)
        {
            /// Add an offset to account for this.
            var offsetMultiplier = EnvironmentConstants.verticesOrder[i];
            float offsetX = (float)(offsetMultiplier.x * normalizedTextureSizeX);
            float offsetY = (float)(offsetMultiplier.y * normalizedTextureSizeY);
            texturePosition[i] = new Vector2(textureStartPosition.x + offsetX, textureStartPosition.y + offsetY);
        }
        return texturePosition;
    }

    /// <summary>
    /// Retrieves the normalized coordinate of the START of the block based on its position within the texture.
    /// Normalized coordinates represent the block's position relative to the entire texture size, ranging from 0 to 1.
    /// Block index is the count of which block it is from the texture, starting from the left
    /// </summary>
    private static Vector2 GetBlockStartCoordinate(Vector2Int textureIndex)
    {
        return new Vector2((float)((textureIndex.x+1) * normalizedTextureSizeX), (float)((textureIndex.y+1) * normalizedTextureSizeY));
    }
}
