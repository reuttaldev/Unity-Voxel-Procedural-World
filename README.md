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

The graphical assets that are used in the game are from free online resources.

## Development Documentation

The most essential scripts, that determine the game’s functionality, are described below. 

### Player Controller
'PlayerController' script enables the player's movement, facilitating the exploration of the environment. It is the most important step in making the game interactable. 
It works by first reading the user's input from 'InputActionReference'- a type provided by Unity's new input system to map button presses to values and callbacks.
The WASD keys input values are read in a special method called 'FixedUpdate', which simulates a loop that is called every frame in a fixed time interval. When the x or y input value is greater than 0, movement is created using a physics component called [RigidBody2D](https://docs.unity3d.com/6000.0/Documentation/Manual/2d-physics/rigidbody/introduction-to-rigidbody-2d.html). The forward direction is calculated- '''moveDirection = transform.right * input.x + transform.forward * input.y;''' and applied by adding force to the physics component in the magnitude of '''moveDirection.normalized * moveSpeed * accelerationMultiplier'''. 
 Unfortunately, applying force can cause the player move at a speed that exceed the intended one. Therefore, 'ControlSpeed' method keeps the linear velocity in check.  
The jumping functionality is implemented by applying a force in the y-direction only during button event callback triggers. Before jumping, I check that the player is not already in air using '''Physics.CheckSphere(raycastPosition.position, playerRadius,GroundLayers, QueryTriggerInteraction.Ignore);'''.

Additionally, a 'drag' is applied to the player's movement to prevent the appearance of gliding across the floor without friction. This was necessary because I had to use a zero-friction material in the collision component to ensure the player wouldn't get stuck against the walls of the voxels when jumping on the terrain.

The rotation of the player (and therefore the direction in which the player needs to move) is calculated based on the camera position. The script that controls it is described below. 

- Camera Controller
- Endless Environment Controller
- Chunk Controller
- Biome Controller

Each chunk is constructed from 16 voxels that are stacked vertically and horizontally. Chunks are also stacked to create the full world. 

There are 3 computationally and resource-expensive tasks: 

1. Setting up the voxel data, i.e. which type of voxel should be and where. 
2. Generating the mesh data for the voxels. This is done by placing vertices and triangles with textures at precise locations for each voxel's visible face. All chunks must be generated and present before creating the meshes, to allow checks on shared faces (i.e., check if a face needs to be visible or occluded decided by if it has a voxel next to it, and which type.) Therefore, communications between chunks if an important part of the system, and it is done in ChunkController.
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
