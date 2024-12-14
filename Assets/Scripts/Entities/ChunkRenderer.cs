using UnityEditor;
using UnityEngine;

/// This class is in charge of combining a group of voxels into one mesh that will be rendered optimally (only the outer faces of the cubes will be created, not the ones that are overlapping).
[RequireComponent(typeof(MeshCollider))]
// mesh filter is the object to be rendered
[RequireComponent(typeof(MeshFilter))]
// the mesh renderer takes the geometry from the mesh filter and renders it at the position defined by the transform 
[RequireComponent(typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    private ChunkData data;
    private CollisionMesh collisionMesh;
    private WaterMesh waterMesh;
    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private ChunkPosition chunkPos; 

#if UNITY_EDITOR
    [SerializeField]
    bool showGizmos = true;
#endif
    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        collisionMesh = new CollisionMesh();
        waterMesh = new WaterMesh();
    }

    private void GenerateVoxelMeshData(Vector3Int relativePos, int textureIndex)
    {
        for (int face = 0; face < EnvironmentConstants.facesCount; face++)
        {
            if (FaceHasNeighbor(relativePos, face))
                continue;
            for (int faceVertex = 0; faceVertex < EnvironmentConstants.vertexNoDupCount; faceVertex++)
            {
                int vertexInFaceIndex = EnvironmentConstants.voxelFaces[face, faceVertex];
                Vector3 vertexInFace = EnvironmentConstants.voxelVertices[vertexInFaceIndex];
                collisionMesh.AddVertices(vertexInFace + relativePos);
            }
            // need 6 triangle points per face, but only 4 vertices are added (2 are duplicates).
            // add triangles after the loop to ensure they reference all vertices in the correct order, including duplicates.
            collisionMesh.AddTriangles();
            collisionMesh.AddUV(TextureUtility.GetUvsForAtlas(face, textureIndex));
        }
    }

    private void GenerateWaterMeshData(Vector3Int relativePos)
    {
        for (int face = 0; face < EnvironmentConstants.facesCount; face++)
        {
            // water only needs to be render in the faces that touch air (not against other cube
            if (GetFaceNeighborType(relativePos, face) != VoxelType.Empty)
                continue;
            for (int faceVertex = 0; faceVertex < EnvironmentConstants.vertexNoDupCount; faceVertex++)
            {
                int vertexInFaceIndex = EnvironmentConstants.voxelFaces[face, faceVertex];
                Vector3 vertexInFace = EnvironmentConstants.voxelVertices[vertexInFaceIndex];
                waterMesh.AddVertices(vertexInFace + relativePos);
            }
            waterMesh.AddTriangles();
            waterMesh.AddUV(TextureUtility.GetUvsForTexture(face));
        }
    }
    /// <summary>
    /// Checks if there is a voxel against the specified face. 
    /// If a voxel is present, there is no need to draw the face, 
    /// </summary>
    /// <returns>True if a voxel is present against the face; otherwise, false.</returns>
    private VoxelType GetFaceNeighborType(Vector3Int relativePos, int faceIndex)
    {
        // offset the position of the voxel we want to check by a value that corresponds to the face parallel to it. 
        Vector3Int posToCheck = relativePos + EnvironmentConstants.faceChecks[faceIndex];
        VoxelType type;
        if (ChunkUtility.ValidLocalVoxelCoordinates(posToCheck))
        {
            // if a voxel exists at this new position, the two voxels are touching.
            type = data[posToCheck];
        }
        // meaning the voxel that requires checking is not in this specific chunk
        // and we need to get info about a chunk that is not related to this render instance. 
        // refer to Chunk Controller to get this info
        else 
        {
            // access the chunk the voxel is in 
            // add the game object transform to make the voxel poisiton global
            type = ChunkContoller.Instance.GetVoxelTypeByGlobalPos(posToCheck + chunkPos.ToWorldPosition());
        }
        return type;
    }
    private bool FaceHasNeighbor(Vector3Int relativePos, int faceIndex)
    {
        VoxelType type = GetFaceNeighborType(relativePos,faceIndex);
        // need to render (ground) voxels under water as well 
        return type != VoxelType.Empty && type != VoxelType.Water;

    }

    public void GenerateChunkMeshData(ChunkData data, ChunkPosition pos)
    {
        // init the class with the given data. These may change at every generation
        this.data = data;
        this.chunkPos = pos;    
        foreach (var kvp in data.Voxels)
        {
            VoxelType type = kvp.Value;
            switch (type)
            {
                case VoxelType.Empty:
                    continue;
                case VoxelType.Water:
                    GenerateWaterMeshData(kvp.Key);
                    break;
                default:
                    int textureIndex = ChunkContoller.Instance.voxelsTextureData.data[(int)type].TexturePosition;
                    GenerateVoxelMeshData(kvp.Key, textureIndex);
                    break;
            }
        }
    }

    public void ClearLastGeneration()
    {
        collisionMesh.Clear();
        waterMesh.Clear();
        meshFilter.mesh = null; 
    }

    /// <summary>
    ///  Upload the calculated mesh to the game object; set the filter mesh to be a mesh generated based on the collected data.
    ///  must be done on the main thread
    /// </summary>
    public void Render()
    {
        if (this.data == null)
        {
            Debug.LogError("Trying to render before initiating the renderer.");
            return;
        }
        if(collisionMesh.Empty)
        {
            Debug.LogError("Trying to render before calculating mesh data.");
            return;
        }

        // add collision 
        meshCollider.sharedMesh = collisionMesh.GetCollisionMesh();
        // create the Unity mesh, and fill it with our calculated mesh data
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        collisionMesh.UploadData(mesh);
        waterMesh.UploadData(mesh);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (showGizmos && Application.isPlaying && Selection.activeObject == gameObject)
        {
            Gizmos.color = new Color(1, 0, 1, 0.4f);
            Vector3 half = new Vector3(EnvironmentConstants.chunkWidth / 2f, EnvironmentConstants.chunkHeight / 2f ,EnvironmentConstants.chunkDepth / 2f);
            Gizmos.DrawCube(transform.position + half, half * 2);
        }
    }
#endif
}

