Sandbox_1_CaveGrid
=====================

DX11 Compute shaders for 3d voxel grid.
---


- I set up a point buffer with a custom data structure, which goes to a compute shader (which edits each point), and afterwards goes to a regular shader (which displays cubes)
- it runs your l-system, which makes a list; I read that list of points and crawl through it in the compute shader.
- I can also swap compute shaders (proof of concept: space + click to distort (distortion is on another compute shader)); so I can use them as filters, one after another
- I also made compute shader which creates noise in the voxels around each point that came from the l-system.

- UPDATE: I have rewritten everything.
A cleaner project structure with a compute shader state machine, events and delegates. 
This version draws cubes of cubes of cubes, can make "infinite" caves.

To create a new computeState, make a new prefab, put it as a child of the CubeStateContainer gameobject in the scene, and also add its exact name as a ComputeStates enum in VoxelCubeStateManager.cs. 
To invoke that state, just set the currentState variable; ex: "currentState = ComputeStates.p2_CellularAutomata;" or call "cueNextComputeStep()".

Controls: 
	Zoom: W and S, or Scroll Wheel
	Pan: middleMouse, or Right Click
	Rotate: leftMouse					
					
Open scene from here: Assets\VoxelCube.unity
					

Web Player Build:
... etc. just increment the number if I forget to update this
http://itu.dk/people/tdbe/boxdrop/Thesis/build_9/
...
http://itu.dk/people/tdbe/boxdrop/Thesis/build_5/
http://itu.dk/people/tdbe/boxdrop/Thesis/build_4/
http://itu.dk/people/tdbe/boxdrop/Thesis/build_3/
http://itu.dk/people/tdbe/boxdrop/Thesis/build_2/
http://itu.dk/people/tdbe/boxdrop/Thesis/build_1/
					
					