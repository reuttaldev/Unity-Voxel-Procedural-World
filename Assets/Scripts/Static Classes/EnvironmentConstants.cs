using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnvironmentConstants
{
    public const int facesCount= 6;
    public const int vertexNoDupCount= 4;
    // x, y, z 
    public const int chunkWidth = 10, chunkDepth=10, chunkHeight=10;
    public static readonly int chunkSize = chunkWidth * chunkDepth * chunkHeight;

    // voxel = cube in this context. These are the cube vertices
    public static readonly Vector3[] voxelVertices = new Vector3[8]
       {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
       };

    /// <summary>
    ///   Each int here corresponds is an index of a vertex from the list above
    ///   Each element here creates two connected triangles = face (rendered only from one side), by creating lines between the corresponding vertices 
    ///  The faces are crated by starting from bottom left vertex, and moving in anti- clockwise direction
    /// </summary>
    public static readonly int[,] voxelFaces = new int[facesCount, vertexNoDupCount] {

        // the original back face is: 0,3,1,1,3,2, but since there is duplicate vertices there is no point to include those. 
        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7 }, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face

	};
    /// <summary>
    /// Using the same steps as we are building the triangles to build the textures, starting from bottom left and moving clockwise. 
    /// </summary>
    public static readonly Vector2[] voxelUvs = new Vector2[vertexNoDupCount] 
    {

        // bottom left
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        // top right
        new Vector2 (1.0f, 1.0f)

    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[facesCount] 
    {

        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0)

    };


}
