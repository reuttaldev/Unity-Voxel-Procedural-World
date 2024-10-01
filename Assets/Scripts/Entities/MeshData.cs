using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//https://docs.unity3d.com/ScriptReference/Mesh.html
public abstract class MeshData
{
    protected List<Vector3> vertices;
    ///a list of indexes of the vertices array
    protected List<int> triangles;
    /// control which pixels on the texture correspond to which vertex on the 3D mesh
    /// The vector 2 tells the position of the pixel on the image, and the index of the vector 2 in this uv array tells which index from the vetor list it will correspond (and be painted on)
    protected List<Vector2> uvs;
    protected int layerIndex;
    public MeshData()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();    
        uvs = new List<Vector2>();
        layerIndex = 0; 
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
    /// <summary>
    ///  Given a Unity mesh object, add to it all the data we hold in this container.
    /// </summary>
    public abstract void UploadData(Mesh mesh); 

    internal void AddUV(Vector2[] a)
    {
        uvs.AddRange(a);
    }
}
public class CollisionMesh: MeshData
{
    public Mesh GetCollisionMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    public override void UploadData(Mesh mesh)
    {
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles,0);
    }

}
public class WaterMesh: MeshData
{
    public WaterMesh() : base()
    {
        layerIndex = 1;
    }
    public override void UploadData(Mesh mesh)
    {
        // no water to render 
        if (vertices.Count == 0)
            return; 

        // get the existing data, because when we call Set it overrides what is already there
        var v = new List<Vector3>(mesh.vertices);
        int originalVerticesCount = v.Count;
        var u = new List<Vector2>(mesh.uv);

        // add to it the current data
        v.AddRange(vertices);
        u.AddRange(uvs);
        // offset the triangle indices to account for previously added triangles. 
        // this ensures that the indices correspond correctly to the vertex indices being added now.
        var t = triangles.Select(val => val + originalVerticesCount).ToArray();

        mesh.SetVertices(v);
        mesh.SetUVs(0,u);
        mesh.SetTriangles(t, 1);
    }
}
