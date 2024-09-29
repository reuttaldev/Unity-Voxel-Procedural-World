using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for applying textures (i.e., images) to the mesh 
/// created for each vertex, so the environment appears textured rather than just white cubes.
/// </summary>
public static class TextureController 
{
    /// <summary>
    /// Each texture file (PNG) contains textures for both the top and sides of a cube.
    /// This value represents the size of a single texture within that file.
    /// </summary>
    private const int textureBlockPixelSize = 164;
    /// size of the entire image texture
    private const int texturePixelSize = 512;
    private static readonly float normalizedBlockSize = textureBlockPixelSize / texturePixelSize;

    /// <summary>
    /// Each element in the UV list is a Vector2 representing the texture coordinate 
    /// for the corresponding vertex at the same index in the vertices list.
    /// This defines which part of the texture is mapped to each vertex.
    /// </summary>
    public static readonly Vector2[][] vortexUVs = new Vector2[EnvironmentConstants.facesCount][];
    static TextureController()
    {
        vortexUVs[0] = GetNormalizedCoordinates(2, 1); // Back Face
        vortexUVs[1] = GetNormalizedCoordinates(0,0); // Front Face
        vortexUVs[2] = GetNormalizedCoordinates(1, 1); // Top Face
        vortexUVs[3] = GetNormalizedCoordinates(0, 1); // Bottom Face
        vortexUVs[4] = GetNormalizedCoordinates(2, 0); // Left Face
        vortexUVs[5] = GetNormalizedCoordinates(1, 0); // Right Face
    }

    /// <summary>
    /// Using the same steps as we are building the triangles to build the textures, starting from bottom left and moving clockwise 
    /// (to build a face = square = 4 vertices)
    /// </summary>
    private static readonly Vector2[] BlockVerticesOffset = new Vector2[EnvironmentConstants.vertexNoDupCount]
    {

        // bottom left conrner
        new Vector2 (0, 0),
        new Vector2 (0, normalizedBlockSize),
        new Vector2 (normalizedBlockSize, 0),
        // top right corner
        new Vector2 (normalizedBlockSize,normalizedBlockSize)

    };

    /// <summary>
    /// Retrieves the normalized coordinate of the START of the block based on its position within the texture.
    /// Normalized coordinates represent the block's position relative to the entire texture size, ranging from 0 to 1.
    /// Block index is the count of which block it is from the texture, starting from the left
    /// </summary>
    private static Vector2 GetBlockStartCoordinate(int blockIndexX, int blockIndexY)
    {
        return new Vector2(blockIndexX * normalizedBlockSize, blockIndexY * normalizedBlockSize);
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
