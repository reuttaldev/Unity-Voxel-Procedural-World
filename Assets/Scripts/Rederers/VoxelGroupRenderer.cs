using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mesh;

[RequireComponent(typeof(MeshCollider))]
// mesh filter is the object to be rendered
[RequireComponent(typeof(MeshFilter))]
// the mesh renderer takes the geometry from the mesh filter and renders it at the position defined by the transform 
[RequireComponent(typeof(MeshRenderer))]
public class VoxelGroupRenderer : MonoBehaviour
{
    VoxelGroup group; 
    MeshCollider meshCllider;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    VoxelGroup voxelGroup;
    [SerializeField]
    bool showGizmos=true;
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void RenderMesh(MeshData meshData, bool colider)
    {
        Mesh mesh = meshFilter.mesh;
        mesh.vertices = meshData.vertices.ToArray();
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.uv = meshData.uvs.ToArray();
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (showGizmos && Application.isPlaying && group != null)
        {
            Gizmos.color = (Selection.activeObject == gameObject) ? new Color(0, 1, 0, 0.4f): new Color(1, 0, 1, 0.4f);
            Vector3 half = new Vector3(group.size / 2f, group.height / 2f, group.size / 2f);
            Gizmos.DrawCube(transform.position + half, half * 2);
        }
    }
#endif
}

