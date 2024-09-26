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
[RequireComponent(typeof(Chunk))]
/// This class is in charge of combining a group of voxels into one mesh that will be rendered optimally (only the outer faces of the cubes will be created, not the ones that are overlapping).
public class ChunkRenderer : MonoBehaviour
{
    // the chunk from which we will generate the mesh data 
    private Chunk chunk; 
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
        chunk = GetComponent<Chunk>();
        meshData = new CollisionMesh();
    }

    private void Start()
    {
        Render();
    }

    private void GenerateVoxelMeshData(Voxel voxel, Vector3Int relativePos)
    {
        // for each face, add the needed vertices
        for (int face = 0; face < EnvironmentConstants.facesCount; face++)
        {
            for (int faceVertex = 0; faceVertex < EnvironmentConstants.voxelFaces.GetLength(1); faceVertex++)
            {
                int vertexInFaceIndex = EnvironmentConstants.voxelFaces[face, faceVertex];
                Vector3 vertexInFace = EnvironmentConstants.voxelVertices[vertexInFaceIndex];
                meshData.AddVertices(vertexInFace + relativePos);
                meshData.AddUV(EnvironmentConstants.voxelUvs[faceVertex]);
                meshData.AddTriangle();
            }
        }
    }
    private void GenerateChunkMeshData()
    {
        meshData.Clear();

        GenerateVoxelMeshData(chunk.voxels[0], Vector3Int.zero);
        //foreach (var voxel in chunk.voxels)
        //{
        //    GenerateVoxelMeshData(voxel);
        //}
    }

    /// <summary>
    ///  set the filter mesh to be the one we generated, so it is applied on the object
    /// </summary>
    private void UploadMesh()
    {
        meshFilter.mesh = meshData.GenerateMeshFromData();
    }

    private void Render()
    {
        GenerateChunkMeshData();
        UploadMesh();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (showGizmos && Application.isPlaying && chunk != null)
        {
            /*
            Gizmos.color = (Selection.activeObject == gameObject) ? new Color(0, 1, 0, 0.4f): new Color(1, 0, 1, 0.4f);
            Vector3 half = new Vector3(EnvironmentConstants.chunkWidth / 2f, EnvironmentConstants.chunkDepth / 2f, EnvironmentConstants.chunkHeight / 2f);
            Gizmos.DrawCube(transform.position + half, half * 2);*/
        }
    }
#endif
}

