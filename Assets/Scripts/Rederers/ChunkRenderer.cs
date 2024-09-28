using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Mesh;

[RequireComponent(typeof(MeshCollider))]
// mesh filter is the object to be rendered
[RequireComponent(typeof(MeshFilter))]
// the mesh renderer takes the geometry from the mesh filter and renders it at the position defined by the transform 
[RequireComponent(typeof(MeshRenderer))]
/// This class is in charge of combining a group of voxels into one mesh that will be rendered optimally (only the outer faces of the cubes will be created, not the ones that are overlapping).
public class ChunkRenderer : MonoBehaviour
{
    // the chunk from which we will generate the mesh data 
    private ChunkData chunk; 
    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshData meshData;
    int index = 0;
#if UNITY_EDITOR
    [SerializeField]
    bool showGizmos=true;
#endif

    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshData = new CollisionMesh();
    }

     /// check if there is a voxel against that face. If so, we do not need to draw it 
    /// has face = return true
    private bool CheckVoxelNeighbors(Vector3Int relativePos, int faceIndex)
    {
        Vector3Int posToCheck = relativePos + EnvironmentConstants.faceChecks[faceIndex];
        // if (chunk.ValidCoordinates(posToCheck))
        //{
        //    return;
        //}
        return false;
    }

    private void GenerateVoxelMeshData(VoxelType voxel, Vector3Int relativePos)
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
                meshData.AddUV(EnvironmentConstants.voxelUvs[faceVertex]);
            }
            // need to add 6 triangle points, but we added only 4 vertices for each face(bc 2 out of the 6 are duplicates)
            // so add the triangles outside the loop 
            int addedVertInLoop = EnvironmentConstants.vertexNoDupCount;
            // add the triangles s.t all the verticies we added in the loop are there in the adding order, including the duplicates that are not in the constants list 
            meshData.AddTrianglePoint(meshData.VerticesCount - addedVertInLoop);
            meshData.AddTrianglePoint(meshData.VerticesCount - addedVertInLoop + 1);
            meshData.AddTrianglePoint(meshData.VerticesCount - addedVertInLoop + 2);
            meshData.AddTrianglePoint(meshData.VerticesCount - addedVertInLoop + 2);
            meshData.AddTrianglePoint(meshData.VerticesCount - addedVertInLoop + 1);
            meshData.AddTrianglePoint(meshData.VerticesCount - addedVertInLoop + 3);
        }
    }
    private void GenerateChunkMeshData()
    {
        meshData.Clear();
        foreach (var kvp in chunk.Voxels)
        {
            GenerateVoxelMeshData(kvp.Value,kvp.Key);
        }
    }

    /// <summary>
    ///  set the filter mesh to be the one we generated, so it is applied on the object
    /// </summary>
    private void UploadMesh()
    {
        meshFilter.mesh = meshData.GenerateMeshFromData();
    }

    public void Render(ChunkData chunkData)
    {
        chunk = chunkData;
        GenerateChunkMeshData();
        UploadMesh();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (showGizmos && Application.isPlaying && chunk != null)
        {
            
            Gizmos.color = (Selection.activeObject == gameObject) ? new Color(0, 1, 0, 0.4f): new Color(1, 0, 1, 0.4f);
            Vector3 half = new Vector3(EnvironmentConstants.chunkWidth / 2f, EnvironmentConstants.chunkDepth / 2f, EnvironmentConstants.chunkHeight / 2f);
            Gizmos.DrawCube(transform.position + half, half * 2);
        }
    }
#endif
}

