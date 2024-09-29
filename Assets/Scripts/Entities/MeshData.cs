using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.Mesh;

//https://docs.unity3d.com/ScriptReference/Mesh.html
public abstract class MeshData
{
    public ICollection<Vector3> vertices;
    ///a list of indexes of the vertices array
    private ICollection<int> triangles;
    /// control which pixels on the texture correspond to which vertex on the 3D mesh
    /// The vector 2 tells the position of the pixel on the image, and the index of the vector 2 in this uv array tells which index from the vetor list it will correspond (and be painted on)
    public List<Vector2> uvs;
    public MeshData()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();    
        uvs = new List<Vector2>();  
    }
    public virtual void Clear()
    {
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
    }
    public virtual void AddVertices(Vector3 v)
    {
        vertices.Add(v);
    }

    /// add the triangles s.t all the verticies we added in the loop are there in the adding order, including the duplicates that are not in the constants list 
    public virtual void AddTriangles()
    {
        int verticesCount = vertices.Count;
        triangles.Add(verticesCount - EnvironmentConstants.vertexNoDupCount);
        triangles.Add(verticesCount - EnvironmentConstants.vertexNoDupCount + 1);
        triangles.Add(verticesCount - EnvironmentConstants.vertexNoDupCount + 2);
        triangles.Add(verticesCount - EnvironmentConstants.vertexNoDupCount + 2);
        triangles.Add(verticesCount - EnvironmentConstants.vertexNoDupCount + 1);
        triangles.Add(verticesCount - EnvironmentConstants.vertexNoDupCount + 3);
    }
    public virtual Mesh GenerateMeshFromData()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    internal void AddUV(Vector2[] a)
    {
        uvs.AddRange(a);
    }
}

public class CollisionMesh: MeshData
{

}
public class WaterMesh: MeshData
{
    Mesh.MeshData waterMesh;
}
