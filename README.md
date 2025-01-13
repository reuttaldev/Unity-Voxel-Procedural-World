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

It also holds a reference to a 'ChunkRenderer' instance, which is described below. 
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

Each generated noise is generated in a static class called `NoiseUtility`. The type of noise that is used is Octave Perlin- where  multiple layers of Perlin noise are layered to create natural patterns and textures. The settings of each calculated noise are different, to avoid unwanted correlations such as trees only being generated at high spots. The settings include seed to ensure consistent generation, octaves amount, amplitude multiplier (contribution of each octave to the final noise), and smoothness (scale of the noise).


Disclaimer: The functionality is fully implemented, but the different environments are not set in the game yet. Only the forest environment is present- others are left for future work. 
### Chunk Controller

Script `ChunkContoller` takes care of instantiating, deleting, and controlling individual components of the chunks. 

Instantiating a chunk involves creating a GameObject (Unity's most basic type, which can represent anything) and attaching it with a fresh `ChunkRenderer` instance. The chunk gets added to a dictionary where the key is the `ChunkPosition` and the value is the `ChunkData`. 
For better performance, we use a pool of chunk objects that are filled with data when needed and cleared when no longer in use before being returned to the pool. This approach avoids the overhead of repeatedly instantiating and destroying GameObjects.

Next, each voxel type inside the chunk needs to be determined. For each one of the 15x15x15 inner positions, the `VoxelType` is decided in a method called `GenerateChunkData`. This process triggers the column-filling process detailed in the "Biome Controller" section. 

 `ChunkContoller`  calls `renderer.GenerateChunkMeshData` for each chunk in its list when it is time to generate the mesh data to transform the chunk from a collection of data into something visible on the player's screen. This step can only be done after the `ChunkData` for all objects has been done. That is because in the renderer when generating mesh data, we perform checks of adjacent voxels to check if they are solid. Sometimes, the adjacent voxels are outside of the local position and exist within a different chunk. This script will be called in order to obtain information about it. If its data is not yet filled out the renderer does not know if to include vertices for this voxel and the process is "blocked".  

 After mesh data is calculated, it can be rendered. In Unity, rendering must be done on the main thread. To avoid blocking performance by rendering all chunks at once—an issue that could hinder operations like moving the player and cause SPF drops —chunk rendering is spread out over fixed time intervals. Positions of chunks that have had their mesh calculated are added to a `ConcurrentQueue<ChunkPosition>` (it will become clear why the queue is concurrent in the next section). In the `Update` loop, elements are dequeued and rendered on the main thread - once per time interval (or frame). 

Additionally, `ChunkController` provides methods for adding trees: `AddTreesData`, `AddTreeTrunk`, `AddTreeLeafs`. These are called if, in a certain column, the "include tree" probability surpassed those provided in `BiomeSetting`. The probability is calculated as a noise rather than a random number because a random (even with a seed) might not give us the same number when we need it to produce a consistent environment. That's because we don't know if we will get to this part of the code at exactly the same point in different machines- some run faster than others. However, the noise will always give us the same value for the same offset settings. The tree's trunk height is decided as follows: `(int)(noiseVal * biomeSettings.maxTrunkHeight);`. The radius of the leaves is  `localTrunkPos.x % 2 ==0 ? 1 : 2;`.
Tree data must be generated after the chunks data, since some elements of a tree can be split between chunks when it is on the wall of a chunk (trunk is in chunk A, some leafs enter chunk B).

### Endless Environment Controller
This script controls the procedural generation aspect of the game. It connects all scripts we described above to work in tandem and create the expected logic. It also defines the multi-threading logic. 

There are 3 computationally and resource-expensive tasks: 

1. Setting up the voxel data, i.e. which type of voxel should be and where. 
2. Generating the mesh data for the voxels. This is done by placing vertices and triangles with textures at precise locations for each voxel`s visible face. All chunks must be generated and present before creating the meshes, to allow checks on shared faces (i.e., check if a face needs to be visible or occluded decided by if it has a voxel next to it, and which type).
 3. rendering the voxel. I use a texture atlas which is all the texture stitched into one image. This is so the renderer does not have to switch textures when rendering different blocks.

The data below showcases the performance of the game when using different threading architectures. The metrics are for generating  100 chunks, each with a width and depth of 15, and height of 60. 
<!-- The images are imported from Unity's profiler. -->

<!-- ![Single Thread](images/main_thread_performance.png)

Using a single-thread implementation (everything is on the main thread):
When generating and rendering a new environment, we experience a significant SPF loss reaching less than 15 SPF. Obviously, the main thread is getting blocked by my voxel engine code. 
Generation took: ~3225 ms


<!-- ![Multi Thread_Sequential](images/Sequential_multithreading_performance.png)-->
Using sequential multithreading (executing major tasks on a separate thread, and awaiting each one before starting the next) Generate chunk data ->  generate mesh data -> render. Generation took: ~6000 ms
We can see that the main thread is no longer getting blocked (we are still above 60FPS), but the generation time is very long (since each task waits for the last one to finish).

<!-- ![Multi Thread_Sequential](images/parallel_multithreading_performance.png)-->
Using (partially) parallel multithreading and object pooling: 
The renderer gets to work as soon as some mesh data is available. Generation took: ~3000 ms. No FPS drops were shown. 

The last approach gives us the best approach. 

To ensure the correct behavior of the game regardless of the number of threads being used, each task is given a `CancellationTokenSource` as an argument. It is canceled and disposed of on the `OnDisable` method of the `EndlessEnvController` GameObject. This is to ensure that when the game is stopped- no threads continue to run in the background.

Regarding the procedurally generated aspect of the game: The initial world is generated with the player positioned at its center. When the player leaves their current chunk, the UpdateWorld function is triggered. This function determines which chunks currently surround the player, using ChunkUtility.GetChunkPositionsAroundPos to calculate these positions, starting from the closest chunk and moving outward. It also identifies chunks that are no longer near the player. Based on this information, chunks are created and destroyed following the process previously explained.
The environment will look different in different positions since the noise values are based on the global position of the chunks. 

To summarize, the sequence of operations for creating the world from start to finish is as follows:
1) Instantiate (or use from an existing pool) chunk GameObbjects. This must be done on the main thread since we are changing GameObbjects properties. 
2) Decide `VoxelTypes` for every voxel in every chunk by filling the values of `ChunkData` for each object in `BiomeController`.
3) Generate mesh data for every chunk i.e. which voxel faces need to be visible and with what texture. This must be done after generating all `ChunkData` due to adjacent voxel checks.
4) Rendering. Can be done as soon as some mesh has finished calculating. Again, this must be done on the main thread.
5) Repeat based on the player's position.

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

### Script Comunication

Most script referencing happens using direct references in the editor. Additionally, I added a a generic class `SimpleSingleton<T>: MonoBehaviour where T: MonoBehaviour` to access instances at runtime without having to set up a references.
