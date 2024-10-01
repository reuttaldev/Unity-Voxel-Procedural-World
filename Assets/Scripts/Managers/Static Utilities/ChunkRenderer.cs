using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Mesh;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// This class is in charge of combining a group of voxels into one mesh that will be rendered optimally (only the outer faces of the cubes will be created, not the ones that are overlapping).
[RequireComponent(typeof(MeshCollider))]
// mesh filter is the object to be rendered
[RequireComponent(typeof(MeshFilter))]
// the mesh renderer takes the geometry from the mesh filter and renders it at the position defined by the transform 
[RequireComponent(typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private EnvironmentController envController;
    private ChunkData chunk;
    private MeshData meshData;
    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

#if UNITY_EDITOR
    [SerializeField]
    bool showGizmos = true;
#endif

    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    private MeshData GenerateChunkMeshData()
    {
        foreach (var kvp in chunk.Voxels)
        {
            if (kvp.Value != VoxelType.Empty)
                GenerateVoxelMeshData(kvp.Value, kvp.Key);
        }
        return meshData;
    }
    private void GenerateVoxelMeshData(VoxelType voxel, Vector3Int relativePos)
    {
        // for each face, add the needed vertices
        for (int face = 0; face < EnvironmentConstants.facesCount; face++)
        {
            if (FaceHasNeighbor(relativePos, face))
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
            int textureIndex = envController.voxelsData.data[(int)voxel].TexturePosition;
            meshData.AddUV(TextureUtility.GetUvs(face, textureIndex));
        }
    }

    /// <summary>
    /// Checks if there is a voxel against the specified face. 
    /// If a voxel is present, there is no need to draw the face, 
    /// </summary>
    /// <returns>True if a voxel is present against the face; otherwise, false.</returns>
    private bool FaceHasNeighbor(Vector3Int relativePos, int faceIndex)
    {
        // offset the position of the voxel we want to check by a value that corresponds to the face parallel to it. 
        Vector3Int posToCheck = relativePos + EnvironmentConstants.faceChecks[faceIndex];
        VoxelType type;
        if (ChunkUtility.ValidLocalVoxelCoordinates(posToCheck))
        {
            // if a voxel exists at this new position, the two voxels are touching.
            type = chunk[posToCheck];
        }
        // meaning the voxel that requires checking is not in this specific chunk
        else 
        {
            // access the chunk the voxel is in 
            // add the game object transform to make the voxel poisiton global
            type = envController.GetVoxelTypeByGlobalPosition(posToCheck + gameObject.transform.position);
        }
        return type != VoxelType.Empty;
    }

    /// <summary>
    ///  set the filter mesh to be the one we generated, so it is applied on the object
    /// </summary>
    public void UploadMesh()
    {
        if (meshData == null)
        {
            Debug.LogError("MeshData given to chunk is null");
            return;
        }
        meshFilter.mesh = meshData.GenerateMeshFromData();
    }

    public void Render(ChunkData data, EnvironmentController control)
    {
        meshData = new CollisionMesh();
        chunk = data;
        envController = control;
        GenerateChunkMeshData();
        UploadMesh();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (showGizmos && Application.isPlaying && Selection.activeObject == gameObject)
        {
            Gizmos.color = new Color(1, 0, 1, 0.4f);
            Vector3 half = new Vector3(EnvironmentConstants.chunkWidth / 2f, EnvironmentConstants.chunkDepth / 2f, EnvironmentConstants.chunkHeight / 2f);
            Gizmos.DrawCube(transform.position + half, half * 2);
        }
    }
#endif
}

