using System;
using UnityEngine;

/// <summary>
/// Collection for voxels. 
/// This represents a group of voxels that share the same mesh, which let me render them all at the same time (same GPU call - better optimization than drawing each cube individually)
/// Each Chunk will be on its own thread to execute calculations of the worlds in parallel 
/// The actual rendering (calculation creation of the mesh is done at ChunkRenderer.cs) 
/// </summary>
public class Chunk: MonoBehaviour 
{
    public Voxel[] voxels = new Voxel[EnvironmentConstants.chunkSize];
    Voxel this[int index] 
    { 
        get
        {
            try
            {
                return voxels[index];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Out of bounds of array");
            }
        }
        set
        {
            try
            {
                voxels[index] = value;
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Out of bounds of array");
            }
        }
    }

    /// <summary>
    /// The chunk exists in a 3D space, with each of its elements (voxels) positioned at specific x, y, z coordinates relative to the chunk.
    /// I want to be able to access these elements based on their world position relative to this chunk.
    /// </summary>
    Voxel this[int x, int y, int z]
    {
        get
        {
            try
            {
                return voxels[CoordinatesToIndex(x,y,z)];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Out of bounds of array");
            }
        }
        set
        {
            try
            {
                voxels[CoordinatesToIndex(z,y,z)] = value;
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Out of bounds of array");
            }
        }
    }

    // 1d array index to 3d array index
    Vector3Int IndexToCoordinates(int i)
    {
        return new Vector3Int(i % (EnvironmentConstants.chunkDepth * EnvironmentConstants.chunkHeight), (i / EnvironmentConstants.chunkHeight) % EnvironmentConstants.chunkDepth, i % EnvironmentConstants.chunkHeight);
    }
    // converting back, i.e. 3d to 1d array index
    int CoordinatesToIndex(int x,int y, int z)
    {
        int i = x + EnvironmentConstants.chunkWidth * y + EnvironmentConstants.chunkDepth * EnvironmentConstants.chunkHeight * z;
        if (i >= EnvironmentConstants.chunkSize) 
            return -1;
        return i;
    }
    bool ValidIndices(Vector3Int coordinates)
    {
        if (coordinates.x < 0 || coordinates.x >= EnvironmentConstants.chunkWidth || coordinates.z < 0 || coordinates.z >= EnvironmentConstants.chunkHeight || coordinates.y < 0 || coordinates.y >= EnvironmentConstants.chunkDepth)
            return false;
        return true;
    }

    public void ChangeVoxel(Vector3Int coordinates, Voxel type)
    {

    }
}
