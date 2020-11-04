using UnityEngine;
using System.Collections;

public class S0_DoNothing : VoxelCubeStateBase {

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
		base.doStart(_stateManager);



	}
	
	public override void doUpdate() {

	}
	
	
	public override void enterState(){

		stateManager.DoRender = VoxelCubeManager.DoNothing;
	}

	public override void exitState(){

		if(!stateManager.renderLock)
			stateManager.DoRender = stateManager.actualRender;
	}


}
