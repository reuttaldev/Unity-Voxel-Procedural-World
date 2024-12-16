using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class EnvironmentConstants
{
    public const int facesCount= 6;
    public const int vertexNoDupCount= 4;
    // x, z, y
    public const int chunkWidth = 15, chunkDepth=15, chunkHeight=100;
    public const int worldSizeInChunks = 4;
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
    ///  Each int here corresponds is an index of a vertex from the list above
    ///  Each element here creates two connected triangles = face (rendered only from one side), by creating lines between the corresponding vertices 
    ///  The faces are crated by starting from bottom left vertex, and moving in anti- clockwise direction
    /// I could use a dictionary with keys representing the direction of each face, I avoid it in order to minimize overhead during iteration.
    /// </summary>
    public static readonly int[,] voxelFaces = new int[facesCount, vertexNoDupCount] {

        // denote corners bottom left,bottom right, upper right and upper left as 0,1,2,3 respectively. 
        // the original back face is: 0,3,1,1,3,2, but since there is duplicate vertices there is no point to include those. 
        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7 }, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face

	};

    // Used for accessing neighboring (adjacent) faces
    public static readonly Vector3Int[] faceChecks = new Vector3Int[facesCount] 
    {
        // backwards
        new Vector3Int(0, 0, -1),
        // forward 
        new Vector3Int(0, 0, 1),
        // up
        new Vector3Int(0, 1, 0),
        // down
        new Vector3Int(0, -1, 0),
        // left 
        new Vector3Int(-1, 0, 0),
        // right
        new Vector3Int(1, 0, 0),

    };
    // Remember vertices are stored and processed in the following order:   bottom left,upper left, bottom right, upper right      
    public static readonly Vector2Int[] verticesOrder = new Vector2Int[vertexNoDupCount]
{
        // bottom left corner
        new Vector2Int (0, 0),
        new Vector2Int (0, 1),
        new Vector2Int (1, 0),
        // top right corner
        new Vector2Int (1,1)

};

}
