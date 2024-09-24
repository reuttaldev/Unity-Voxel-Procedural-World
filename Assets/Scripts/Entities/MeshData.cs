using System.Collections.Generic;
using UnityEngine;

//https://docs.unity3d.com/ScriptReference/Mesh.html
public class MeshData
{
    public ICollection<Vector3> vertices { get; private set; }
    // control which pixels on the texture correspond to which vertex on the 3D mesh
    public List<Vector2> uvs { get; private set; }
    public List<int> triangles { get; private set; }

    public virtual void AddVertex(Vector3 v)
    {
        vertices.Add(v);
    }

    // quad = two connected triangles, rendered only from one side 
    public void AddQuads()
    {
        List<int> tempTriangle = new List<int>();
        int verticesCount = vertices.Count;
        for (int i = 0; i < 2; i++)
        {
            //starting from bottom left
            triangles.Add(verticesCount - 4);
            //moving in clockwise direction
            for (int j = 1; j < 3; i++)
            {
                triangles.Add(verticesCount - 4+j+i);
            }
        }
        triangles.AddRange(tempTriangle);
    }
}
public class WaterMesh: MeshData
{
    Mesh.MeshData waterMesh;
    public override void AddVertex(Vector3 v)
    {
        vertices.Add(v);
    }
}
