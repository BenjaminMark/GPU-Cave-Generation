using UnityEngine;
using System.Collections;

public class S1_LoadOnly : VoxelCubeStateBase {

	//this can remain empty if you don't want your state to have a compute shader.
	//public ComputeShader cs; //Defined in this.base
	//private VoxelCubeStateManager stateManager;//Defined in this.base

	private int listIndex = 0;

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


		if(listIndex < stateManager.lPts.Count){
			//stateManager.lPts[listIndex].lPoint*= stateManager.lSystemResolution;//resolution
			float[] lpt = { 
							stateManager.lPts[listIndex].lPoint.x * stateManager.lSystemResolution + (stateManager.cubeSize/2)* stateManager.cubeSpacing, 
							stateManager.lPts[listIndex].lPoint.z * stateManager.lSystemResolution +(stateManager.cubeSize/2)* stateManager.cubeSpacing, 
							stateManager.lPts[listIndex].lPoint.y * stateManager.lSystemResolution
						  };

			cs.SetFloats("lPoint", lpt);
			
			listIndex++;
		}
		else{//TODO: NOTE: this is what switches the state to the cellular automata state.
			stateManager.cueNextComputeStep();
			//stateManager.currentState = VoxelCubeStateManager.CubeStates.p3_simpleAutomata;
		}


		base.csDispatch();
	}


	public override void enterState(){
		
		listIndex = 0;
	}

}
