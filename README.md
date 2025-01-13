In this project, I present a procedurally generated environment that is accomplished using a custom voxel engine made in Unity.

## Download Link
To access the Windows, Mac, or Linux build please follow this link [https://lrtal.itch.io/procedurally-generated-voxel-world](https://lrtal.itch.io/procedurally-generated-voxel-world). Download, unzip, and run the executable file.
On Mac, there may be a permission issue since I am not a known publisher. Please try running this command  in the .app containing folder: 
chmod -R +x VoxelWorldProject.app (This is a known Mac-specific [problem](https://discussions.unity.com/t/mac-unity-build-from-a-pc-not-opening-on-mac/803627) with Unity builds. If the command does not work, please contact reutgaming@gmail.com for further assistance.)

## Controls

- Move the player using WASD and arrow keys. Hold Shift or Left Shift to run.  
- Move the camera using the mouse. 
- Jump using Space. 
- Exit the game by pressing ESC for 3 seconds.

## Project Specification

A voxel is an object that resembles a cube in appearance and is considered the 3D equivalent of a pixel.  A voxel engine uses these objects by stacking them and only rendering the faces that are visible to the user. Any environment can be created by utilizing differently sized and textured voxels. The most popular example of a game that thrives on this idea is Minecraft.

In this project, I attempt to create my own version of that to understand the mechanics that work behind the scenes, as well as experiment with multi-threading to streamline the steps of creating and rendering the different voxels in a timely manner. Initially, a small environment is initialized. As the player moves through the scene, new parts will be generated procedurally.

This project was developed in C\# using the Unity engine. It was chosen since it is a well-known and reliable game development tool. The Unity editor has numerous versions, the one used in this game is the newest long-term support release - 6000.0.31f1.

The graphical assets used in the game are from free online resources.

## Development Documentation

The most essential scripts, that determine the game’s functionality, are described below. 

### Chunk Data
Each chunk is constructed from voxels arranged in the 3D space, and chunks are stacked together to form the full world. `ChunkData` is an entity class that represents a collection of voxels. They are stored as a dictionary, where the key is the local position of a voxel within the chunk, and the value is an enum called `VoxelType`. The `VoxelType` determines whether a voxel at position (x,y) will show a texture of grass, grass, water, etc. I provide an indexer method to conveniently access a voxel's type using Vector3Int coordinates.

It also holds a reference to a 'ChunkRenderer' instance, that is described below. 
### Chunk Renderer 

In a voxel engine, the world is built using groups of voxels called chunks instead of treating individual voxels as building blocks. This approach is clever because it allows multiple objects to be rendered in the same GPU call,  which offers better optimization compared to drawing each cube individually. Additionally, the mesh is optimized by generating faces only for the parts visible to the player. Faces of adjacent voxels that touch each other are not generated, which further reduces GPU usage significantly.

The 'ChunkRenderer' script is where the mesh generation calculation and rendering occur. Method `GenerateChunkMeshData` will read the `ChunkData` information that is passed as an argument and call `GenerateWaterMeshData` or `GenerateVoxelMeshData` for each element in the collection based on its `VoxelType` (water vs. anything else that is not air). The following operation will take place in these methods: 
1) The texture that is associated with the `VoxelType` is retrieved from a texture atlas map called `TextureData`
2) for each face in the voxel:
 2.a) Check if the voxel adjacent to this face is solid (any type other than air or water). If it is, there is no need to generate mesh for it since it will never be visible to the player. Return to the step above and continue with the next face. Otherwise, continue to the next step.
 2.b) Add vertices that match the current face to `MeshData` instances that are associated with this renderer. It is an abstract class that has two children: `CollisionMesh` and `WaterMesh`. You add the vertices to the appropriate property based on the method that was called. The differentiation is necessary since the water mesh does not need collision vertices and also the water is rendered on a different mesh layer than the rest.  You know which vertices need to be added to create a specific face by retrieving it from a class called `EnvironmentConstants` that contains static constant / readonly general information that is useful for a voxel engine.
 2.c) Similarly to the vertices, we add the triangles (a list of indexes of the vertices array that are grouped togther in the mesh) and uvs (a list of `Vector2`s that determine which pixel in the texture obtained in step 1) is associated with each vertex).

The process of obtaining the voxel adjacent to a face in step 2.a) is executed as follows:
1) Check if the relative position of the voxel being examined has y != 0 and the face is not the bottom one. If it is, that indicates that this face is at the absolute bottom of the entire chunk. In such cases, there is no need to generate a mesh for it since it will never be visible.
2) Offset the position of the voxel being examined by a vector corresponding to the face parallel to it. For example, when checking the upper face, offset the current local position by `Vector3Int(0, 1, 0)` to determine the local position of the voxel directly above the one being examined.
3) Check if the offset local position is still within the chunk (`coordinates.x >= 0 && coordinates.x < EnvironmentConstants.chunkWidth...`). If it is, simply check its type by using the indexer method on the same `ChunkData` instance that was initially given as an argument: ` type = data[posToCheck];`. If it is not, we need to get information about a chunk that is not related to this renderer. We refer to a class `Chunk Controller` that will be described later on to get this information: `ChunkContoller.Instance.GetVoxelTypeByGlobalPos(posToCheck + chunkPos.ToWorldPosition());`.

This concludes the mesh generation step. 
The renderer, of course, also offers a `Render` method. This uploads the `GenerateWaterMeshData` and `GenerateVoxelMeshData` information to the GameObject's components that will make the object visible: `MeshRenderer`, `MeshFilter` and `MeshCollider` 

### Biome Controller
A biome represents a distinct environmental area within the game world. The `BiomeController` manages the generation and behavior of the different environments based on noise and texture settings. 
Its method takes as arguments a chunk and a `BiomeType` e.g. forest, beach. It retrieves assets called `BiomeSettings` that are `ScriptableObject` types. These contain references to other setting assets called `NoiseSettings`. 

The columns of the chunk are filled out in the following manner: 
1) Retrieve settings
2) Generate a noise to decide the visible height of this column: `int groundHeight = NoiseUtility.GetNormalizedNoise(globalColumnPos, settings.noise);`. the noise is normalized to be between 0 and the chunk's height.
3) Generate a secondary noise to determine whether this column should include special voxel types e.g. rocks. If it is higher than a threshold specified in the settings, make all voxels below ground height to be the special type. Everything above that is either air or water.
4) Call `DecideVoxelTypeByY` given a y position and the maximum visible height position of the column. The texture below the max height will be determined based on the `BiomeSettings` texture values. The values above it will either be air or water- based on `BiomeSettings` water threshold.
5) Generate a third noise to determine whether this column should include a tree, and which type. If it is higher than a threshold specified in the settings, call `AddTreeData`. This will add a tree type to a list that will be generated and rendered once the chunks are all visible.  

Each generated noise is generated in a static class called `NoiseUtility`. The type of noise that is used is Octave Perlin- where  multiple layers of Perlin noise are layered to create natural patterns and textures. The settings of each calculated noise are different, to avoid unwanted correlations such as trees only being generated at high spots. The settings include seed to ensure consistent generation, octave values, amplitude multiplier, and smoothness.


Disclaimer: The functionality is fully implemented, but the different environments are not set in the game yet. Only the forest environment is present. 
### Chunk Controller

Script `ChunkContoller` takes care of instantiating, deleting, and controlling individual components of the chunks e.g. the renderer. 

Instantiating a chunk involves creating a GameObject (Unity's most basic type, which can represent anything) and attaching it with a  fresh `ChunkData` instance. The chunk gets added to a dictionary where the key is the `ChunkPosition` and the value is the `ChunkData`. 

Next, each voxel type inside the chunk needs to be determined. For each one of the 15x15x15 inner positions, the `VoxelType` is decided in a method called `GenerateChunkData`. This process will be detailed in the "Biome Controller" section. 

After each voxel is assigned a type, it is time to generate the mesh data to transform the chunk from a collection of data into something visible on the player's screen. The chunk GameObject is also attached with a `ChunkRenderer` instance. This class is in charge of combining a group of voxels into one mesh that will be generated optimally (only the outer faces of the voxels will be created, not the ones that are overlapping). `ChunkContoller`  calls `renderer.GenerateChunkMeshData` for each chunk in its list.

For better performance, we use a pool of chunk objects that are filled with data when needed and cleared when no longer in use before being returned to the pool. This approach avoids the overhead of repeatedly instantiating and destroying GameObjects.


### Endless Environment Controller
This script controls the procedural generation aspect of the game. It connects all scipts we described above to work in tandem and create the expected logic.

In charge of placing chunks in the world and the order of operations in which the methods on them is controlled, as well as multi-threading logic. 
Controls the placement of the chunks. 

There are 3 computationally and resource-expensive tasks: 

1. Setting up the voxel data, i.e. which type of voxel should be and where. 
2. Generating the mesh data for the voxels. This is done by placing vertices and triangles with textures at precise locations for each voxel`s visible face. All chunks must be generated and present before creating the meshes, to allow checks on shared faces (i.e., check if a face needs to be visible or occluded decided by if it has a voxel next to it, and which type.) Therefore, communications between chunks if an important part of the system, and it is done in ChunkController.
 3. rendering the voxel. I use a texture atlas which is all the texture stitched into one image. This is so the renderer does not have to switch textures when rendering different blocks.
The metrics shown below are for generating 100 chunks, each with a width and depth of 15, and height of 60	

The data below showcases the performance of the game when using different threading architectures. 

Using a single-thread implementation (everything is on the main thread)
When generating and rendering a new environment, we experience a significant SPF loss of less than 15 SPF. Obviously, the main thread is getting blocked by my voxel engine code. 
Generation took: ~3225 ms


Sequential multithreading (executing major task on a separate thread, and awaiting each one before starting the next) Generate chunk data ->  generate mesh data -> render 




Generation took: 7385 ms
We can see that the main thread is no longer getting blocked (we are still above 60FPS), but the generation time is very long (since each tasks waits for the last one to finish).


Parallel multithreading and object pooling : 
Mesh generation begins as soon as voxel data is ready for a chunk, rather than waiting for all voxel data to finish across all chunks. Similarly, the renderer gets to work as soon as some mesh data is available. 

### Player Controller
`PlayerController` script enables the player's movement, facilitating the exploration of the environment. It is the most important step in making the game interactable. 
It works by first reading the user's input from `InputActionReference`- a type provided by Unity's new input system to map button presses to values and callbacks.
The WASD keys input values are read in a special method called `FixedUpdate`, which simulates a loop that is called every frame in a fixed time interval. When the x or y input value is greater than 0, movement is created using a physics component called [RigidBody2D](https://docs.unity3d.com/6000.0/Documentation/Manual/2d-physics/rigidbody/introduction-to-rigidbody-2d.html). The forward direction is calculated- ```moveDirection = transform.right * input.x + transform.forward * input.y;``` and applied by adding force to the physics component in the magnitude of ```moveDirection.normalized * moveSpeed * accelerationMultiplier```. 
 Unfortunately, applying force can cause the player move at a speed that exceed the intended one. Therefore, `ControlSpeed` method keeps the linear velocity in check.  
The jumping functionality is implemented by applying a force in the y-direction only during button event callback triggers. Before jumping, I check that the player is not already in the air by creating a sphere at the player's feet and checking if it is colliding with the ground: `Physics.CheckSphere(raycastPosition.position, playerRadius,GroundLayers, QueryTriggerInteraction.Ignore);`.

Additionally, a `drag` is applied to the player's movement to prevent the appearance of gliding across the floor without friction. This was necessary because I had to use a zero-friction material in the collision component to ensure the player wouldn`t get stuck against the walls of the voxels when jumping on the terrain.

The rotation of the player (and therefore the direction in which the player needs to move) is calculated based on the camera position. The script that controls it is described below. 

### Camera Controller
`CameraRotator` script changes the camera view based on the user's mouse input. After reading the `InputActionReference` values in the `Update` loop, if the squared magnitude exceeds some small constant, `RotateHorizontally` and `RotateVertically` are called.

The horizontal input simply rotates the player around its own y-axis. This will cause changes to the camera view since I am utilizing Cinemachine's follow functionality. The vertical input controls an object that signifies the player's eyes only, without changing the placement of the player itself. The input gets added to the current vertical position, clamped to +90 and -90 to ensure the correct view of the world. 

