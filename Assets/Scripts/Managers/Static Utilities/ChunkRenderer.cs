using UnityEditor;
using UnityEngine;
using static UnityEngine.Mesh;

/// This class is in charge of combining a group of voxels into one mesh that will be rendered optimally (only the outer faces of the cubes will be created, not the ones that are overlapping).
public static class ChunkRenderer
{
    /// check if there is a voxel against that face. If so, we do not need to draw it 
    /// has face = return true
    private static bool CheckVoxelNeighbors(Vector3Int relativePos, int faceIndex)
    {
        Vector3Int posToCheck = relativePos + EnvironmentConstants.faceChecks[faceIndex];
        // if (chunk.ValidCoordinates(posToCheck))
        //{
        //    return;
        //}
        return false;
    }

    private static void GenerateVoxelMeshData(VoxelType voxel, Vector3Int relativePos, MeshData meshData)
    {
        // for each face, add the needed vertices
        for (int face = 0; face < EnvironmentConstants.facesCount; face++)
        {
            if (CheckVoxelNeighbors(relativePos, face))
                continue;
            for (int faceVertex = 0; faceVertex < EnvironmentConstants.vertexNoDupCount; faceVertex++)
            {
                int vertexInFaceIndex = EnvironmentConstants.voxelFaces[face, faceVertex];
                Vector3 vertexInFace = EnvironmentConstants.voxelVertices[vertexInFaceIndex];
                meshData.AddVertices(vertexInFace + relativePos);
            }
            // need to add 6 triangle points, but we added only 4 vertices for each face(bc 2 out of the 6 are duplicates)
            // so add the triangles outside the loop 
            // add the triangles s.t all the verticies we added in the loop are there in the adding order, including the duplicates that are not in the constants list 
            meshData.AddTriangles();
            int textureIndex = ServiceLocator.Instance.Get<EnvironmentController>().voxelsData.data[(int)voxel].TexturePosition;
            meshData.AddUV(TextureUtility.GetUvs(face, textureIndex));
        }
    }
    private static MeshData GenerateChunkMeshData(Chunk chunk)
    {
        MeshData meshData = new CollisionMesh(); 
        foreach (var kvp in chunk.Voxels)
        {
            if(kvp.Value != VoxelType.Empty)
                GenerateVoxelMeshData(kvp.Value, kvp.Key, meshData);
        }
        return meshData;
    }

    public static void Render(Chunk chunk)
    {
        chunk.UploadMesh(GenerateChunkMeshData(chunk));
    }
}

