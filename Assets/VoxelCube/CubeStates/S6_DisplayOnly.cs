using UnityEngine;
using System.Collections;

public class S6_DisplayOnly : VoxelCubeStateBase {

	//this can remain empty if you don't want your state to have a compute shader.
	//public ComputeShader cs; //Defined in this.base
	//private VoxelCubeStateManager stateManager;//Defined in this.base


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	/// <summary>
	/// Takes the StateManager's Compute Buffer and assigns it to this state's compute shader.
	/// Also assigns whatever Floats or Ints etc. this state's compute shader needs.
	/// </summary>
	public override void doStart(VoxelCubeManager _stateManager) {
		/*
		base.doStart(_stateManager);

		if(base.cs){
			
			//base.
			cs.SetBuffer(0, "particleBuffer", stateManager.particleBuffer);
			cs.SetFloats("sprayOrbRadius", stateManager.sprayOrbRadius);//0.6f);
			cs.SetInt("targetChanged", 1);

		}
		*/
	}
	
	public override void doUpdate() {
		//base.doUpdate();
		//cs.SetFloat("deltaTime", Time.deltaTime);
		//base.handleMouseOrb();

		//TODO: This CS is disabled. no longer desired in the advanced build. was useful to debug a 1 tile cube. 

		//base.csDispatch();
	}
	
	

}
