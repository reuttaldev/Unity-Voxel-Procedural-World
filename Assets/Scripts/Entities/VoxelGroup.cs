using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Voxel
{
    Empty,
    Water,
    Sand,
    Dirt,
    Dark_Grass,
    Light_Grass,
    Dark_Gravel,
    Light_Gravel,
    Stone,
    Tree,
    Sky
}

// creating my own collection for voxels. 
// This represents a group of voxels that will be rendered as one object
public class VoxelGroup

{
    Voxel[] voxels;
    // x,y,z
    public int width, depth, height;
    public int Count => width * height * depth;

    public bool IsReadOnly => false;

    void Init()
    {
        voxels = new Voxel[Count];
        // we start when everything is empty
        for (int i = 0; i < Count; i++)
        {
            voxels[i] = Voxel.Empty;
        }
    }
    void Clear()
    {
        Init();
    }
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

    // we have an inner coordiante system (in 3D space) to map each voxel in the gourp to a unique position
    Vector3Int IndexToCoordinates(int i)
    {
        return new Vector3Int(i % (depth * height), (i / height) % depth, i % height);
    }
    int CoordinatesToIndex(int x,int y, int z)
    {
        int i = x + width * y + depth * height * z;
        if (i >= Count) 
            return -1;
        return i;
    }
    bool ValidIndices(Vector3Int coordinates)
    {
        if (coordinates.x < 0 || coordinates.x >= width || coordinates.z < 0 || coordinates.z >= height || coordinates.y < 0 || coordinates.y >= depth)
            return false;
        return true;
    }

    public void ChangeVoxel(Vector3Int coordinates, Voxel type)
    {

    }
}
