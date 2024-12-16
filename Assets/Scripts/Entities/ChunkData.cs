using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mesh;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// Collection for voxels. 
/// This represents a group of voxels that share the same mesh, which let me render them all at the same time (same GPU call - better optimization than drawing each cube individually)
/// Each Chunk will be on its own thread to execute calculations of the worlds in parallel 
/// The actual rendering (calculation creation of the mesh is done at ChunkRenderer.cs) 
/// This is a data container script and does not represent the chunk gameobject. It does contain a reference to the related game object thought, through the renderer variable
/// </summary>
public class ChunkData 

{    /// <summary>
     /// The chunk exists in a 3D space, with each of its elements (voxels) positioned at specific x, y, z coordinates relative to the chunk.
     /// I want to be able to access these elements based on their world position relative to this chunk.
     /// using a dictionary with the position as the key and the type as values to store the voxels in the block
     /// Instead of using an array the size of the voxel space, since many values will be empty (air voxels),
     /// this would cause significant memory overhead.
     /// </summary>
    private Dictionary<Vector3Int, VoxelType> voxels = new Dictionary<Vector3Int, VoxelType>();
    public IEnumerable<KeyValuePair<Vector3Int, VoxelType>> Voxels { get { return voxels; } }
    public VoxelType this[Vector3Int index]
    {
        // remember, not allowed to change the dictionary while iterating it 
        get
        {
            if (voxels.ContainsKey(index))
            {
                return voxels[index];
            }
            else
            {
                return VoxelType.Empty;
            }
        }
        set
        {
            voxels[index] = value;
        }
    }
    private Vector3Int indicesVector = new Vector3Int(); 
    public VoxelType this[int x, int y, int z]
    {
        get
        {
            indicesVector.y = y; indicesVector.z = z; indicesVector.x = x;
            return this[indicesVector];
        }
        set
        {
            indicesVector.y = y; indicesVector.z = z; indicesVector.x = x;
            this[indicesVector] = value;
        }
    }
    // the renderer is attached to the chunk gameobject 
    public ChunkRenderer renderer = null;

    // Location of trees is decided in BiomeController.FillChunkColumn when generating the world. 
    private List<TreeData> treesDatas = new List<TreeData>();
    public void AddTreeData(TreeData data)
    {
        treesDatas.Add(data);
    }
    public IReadOnlyCollection<TreeData> TreesData => treesDatas;
}
