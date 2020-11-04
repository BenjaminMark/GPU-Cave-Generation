using UnityEngine;
using System.Collections;

public class CameraRenderCube : MonoBehaviour 
{

	void OnRenderObject()
	{	
		//NOTE: don't be scared by the foreach. it's not for every voxel. it's in case you want/have more cubes of voxels.
		if (VoxelCubeManager.list != null)
			foreach(VoxelCubeManager particle in VoxelCubeManager.list)
				particle.Render();
	}


	/*
	void OnPostRender()
	{	
		//NOTE: don't be scared by the foreach. it's not for every voxel. it's in case you want/have more cubes of voxels.
		if (VoxelCubeManager.list != null)
			foreach(VoxelCubeManager particle in VoxelCubeManager.list)
				particle.Render();
	}
	*/
}
