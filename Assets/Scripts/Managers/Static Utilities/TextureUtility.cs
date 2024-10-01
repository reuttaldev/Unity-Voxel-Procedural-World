using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for the calculations needed before applying textures (i.e., images) to the mesh 
/// created for each vertex, so the environment appears textured rather than just white cubes.
/// </summary>
public static class TextureUtility 
{
    /// <summary>
    /// Each texture contains textures for both the top and sides of a cube.
    /// This value represents the size of a single texture within that file.
    /// </summary>
    private const int textureBlockPixelSize = 164;
    /// The image file (atlas) contains all the textures for the various voxel types. This is the size of a texture for one voxel
    private const int texturePixelSizeX = 492;
    private const int texturePixelSizeY = 328;
    /// size of the entire image (atlas)
    private const int imagePixelSizeX = 492;
    private const int imagePixelSizeY = 2623;

    private static readonly float normalizedTextureSizeY = (float)texturePixelSizeY / imagePixelSizeY;
    private static readonly float normalizedBlockSizeX = (float)textureBlockPixelSize / imagePixelSizeX;
    private static readonly float normalizedBlockSizeY = (float)textureBlockPixelSize / imagePixelSizeY;

    /// <summary>
    /// Each element in the UV list is a Vector2 representing the texture coordinate 
    /// for the corresponding vertex at the same index in the vertices list.
    /// Each face uses a set of 4 coordinates corresponding to the vertices in the face.
    /// This defines which part of the texture is mapped to each vertex.
    /// </summary>
    private static Vector2[][] voxelTextureUvs;
    static TextureUtility()
    {
        // each element in this array represents the texture coordinates for a single face of the voxel. 
        // this works only for an image with a single texture in it. 
        voxelTextureUvs = new Vector2[EnvironmentConstants.facesCount][];
        voxelTextureUvs[0] = GetNormalizedCoordinates(2, 1); // Back Face
        voxelTextureUvs[1] = GetNormalizedCoordinates(0,0); // Front Face
        voxelTextureUvs[2] = GetNormalizedCoordinates(1, 1); // Top Face
        voxelTextureUvs[3] = GetNormalizedCoordinates(0, 1); // Bottom Face
        voxelTextureUvs[4] = GetNormalizedCoordinates(2, 0); // Left Face
        voxelTextureUvs[5] = GetNormalizedCoordinates(1, 0); // Right Face
    }

    public static Vector2[] GetUvsForTexture(int faceIndex)
    {
        return voxelTextureUvs[faceIndex];
    }

    /// Handle a texture atlas, where multiple textures are stacked vertically in a single image.
    public static Vector2[] GetUvsForAtlas(int faceIndex, int textureIndex)
    {
        Vector2[] modifiedArray = new Vector2[4];

        for (int i = 0; i < 4; i++)
        {
            Vector2 v = voxelTextureUvs[faceIndex][i];
            /// The UV coordinates are adjusted by adding a vertical offset to account for the position of each texture within the image.
            modifiedArray[i] = new Vector2(v.x, v.y + (textureIndex)* normalizedTextureSizeY );
        }
        return modifiedArray;
    }

    /// <summary>
    /// Using the same steps as we are building the triangles to build the textures, starting from bottom left and moving clockwise 
    /// (to build a face = square = 4 vertices)
    /// </summary>
    private static readonly Vector2[] BlockVerticesOffset = new Vector2[EnvironmentConstants.vertexNoDupCount]
    {
        // bottom left conrner
        new Vector2 (0, 0),
        new Vector2 (0, normalizedBlockSizeY),
        new Vector2 (normalizedBlockSizeX, 0),
        // top right corner
        new Vector2 (normalizedBlockSizeX,normalizedBlockSizeY)

    };
    
    /// <summary>
    /// Retrieves the normalized coordinate of the START of the block based on its position within the texture.
    /// Normalized coordinates represent the block's position relative to the entire texture size, ranging from 0 to 1.
    /// Block index is the count of which block it is from the texture, starting from the left
    /// </summary>
    private static Vector2 GetBlockStartCoordinate(int blockIndexX, int blockIndexY)
    {
        return new Vector2(blockIndexX * normalizedBlockSizeX, blockIndexY * normalizedBlockSizeY);
    }

    /// Get the normalized coordinates for the entire face (all 4 vertices)
    private static Vector2[] GetNormalizedCoordinates(int blockIndexX, int blockIndexY)
    {
        // find the starting point of our texture block, whithin the texture file and normalize it so it won't be in pixels, but relative to the image size
        var startCoordinates = GetBlockStartCoordinate(blockIndexX, blockIndexY);
        Vector2[] coordinates = new Vector2[EnvironmentConstants.vertexNoDupCount];
        // add offset for each of the corners
        for (int i = 0; i < EnvironmentConstants.vertexNoDupCount; i++)
        {
            coordinates[i] = startCoordinates + BlockVerticesOffset[i];
        }
        return coordinates;
    }

}
