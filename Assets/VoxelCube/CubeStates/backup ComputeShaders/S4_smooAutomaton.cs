using UnityEngine;
using System.Collections;

public class S4_smooAutomaton : VoxelCubeStateBase {

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

		if(base.cs){

			cs.SetBuffer(0, "particleBuffer", stateManager.particleBuffer);
			cs.SetFloats("sprayOrbRadius", stateManager.settings.sprayOrbRadius);//0.6f);
			cs.SetInt("targetChanged", 1);
			cs.SetInt("cubeSize", stateManager.cubeSize);//for automatas
			cs.SetFloats("spacing", stateManager.cubeSpacing);//0.6f);//for automatas
		}
	}
	
	public override void doUpdate() {
		base.doUpdate();

		// Set the compute shader variables for this frame.
		cs.SetFloat("deltaTime", Time.deltaTime);
		cs.SetFloat("targetStrengh", stateManager.mouseStrengh);//*msSign);

		if(UtilityScript.arrayApproximately(base.prevTarget, base.target)){
			cs.SetInt("targetChanged", 0);
		}
		else{
			cs.SetInt("targetChanged", 1);
			cs.SetFloats("target", base.target);
		}
		base.prevTarget = base.target;

		base.csDispatch();
	}
	
	

}
