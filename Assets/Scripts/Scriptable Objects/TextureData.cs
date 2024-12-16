using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Voxels Texture Data", menuName = "Scriptable Objects/Voxels Texture Data")]
// We have a texture atlas- containing all sprites for the voxel. 
// This data container is used to store which texture is stored where in the atlas, so we can access it easily at run time as well as make changes easily it the editor (not having to change any code in case of making changes to the atlas).
// The atlas corners are (0,0), (1,0), (0,1), (1,1) for the botton left, bottom right, upper left and upper right corners respectivally. 
// The positions of the textures within the atlas are normalized- they are relative to the atlas size.
// meaning row 0 (bottom) starts at y=0, row 1 at y= texturePixelSize / atlasPixelSize.
public class TextureData : ScriptableObject
{
    [SerializeField]
    private List<VoxelData> data;
    private Dictionary<VoxelType, VoxelData> dataDict = new Dictionary<VoxelType, VoxelData>();

    private void OnEnable()
    {
        foreach (var item in data)
        {
            dataDict[item.Type] = item;    
        }
    }

    public VoxelData GetVoxelData(VoxelType type)
    {
        if (dataDict.TryGetValue(type, out var voxelData))
            return voxelData;
        Debug.LogError("No voxel data for type "+type.ToString() +" in texture data scriptable object");
        return dataDict[VoxelType.Light_Grass];
    }

}
