using UnityEngine;
using System.Collections;

public class S2_LoadAndNoise : VoxelCubeStateBase {

	//this can remain empty if you don't want your state to have a compute shader.
	//public ComputeShader cs; //Defined in this.base
	//private VoxelCubeStateManager stateManager;//Defined in this.base

	public ComputeShader deprecatedCS;
	public ComputeShader correctCS;



	private int listIndex = 0;
	private float mod;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void userTestingOldCS(VoxelCubeManager _stateManager){
		base.cs = deprecatedCS;
		doStart(_stateManager);
	}
	
	/// <summary>
	/// Takes the StateManager's Compute Buffer and assigns it to this state's compute shader.
	/// Also assigns whatever Floats or Ints etc. this state's compute shader needs.
	/// </summary>
	public override void doStart(VoxelCubeManager _stateManager) {

		base.doStart(_stateManager);

		//TODO remove this hack after user testing
		if(_stateManager.useOldCurlNoiseInstead){
			cs = deprecatedCS;
		}
		else{
			cs = correctCS;
		}

		if(base.cs){
			
			//base.
			cs.SetBuffer(0, "particleBuffer", stateManager.particleBuffer);
			cs.SetFloats("sprayOrbRadius", stateManager.settings.sprayOrbRadius);//0.6f);
			//cs.SetInt("targetChanged", 1);
			cs.SetFloats("sprayOrbOuterRadius", stateManager.settings.sprayOrbOuterRadius);//0.6f);
			cs.SetFloats("randomSeed", stateManager.randomSeed);

			cs.SetFloat("noiseOctave", stateManager.noiseOctave);
			cs.SetFloat("noiseDistAtten", stateManager.noiseDistAtten);

			cs.SetFloat("metaballDistortionFade", stateManager.metaballDistortionFade);


		}

		//setMod();
	}
	
	public override void doUpdate() {
		//base.doUpdate();
		//cs.SetFloat("deltaTime", Time.deltaTime);
		//base.handleMouseOrb();


		if(listIndex < stateManager.lPts.Count){
			//stateManager.lPts[listIndex]*= stateManager.lSystemResolution;//resolution
			float[] lpt = { 
			                        stateManager.lPts[listIndex].lPoint.x,  
									stateManager.lPts[listIndex].lPoint.y, 
									stateManager.lPts[listIndex].lPoint.z
							};
			//TODO: to verify if this point is within the current cube, and if it is, remove it from list or smth.
			cs.SetFloats("lPoint", lpt);
			//float[] lNois = { stateManager.lPts[listIndex].noise2.x, stateManager.lPts[listIndex].noise2.y};
			float[] lNois = { stateManager.lPts[listIndex].noise3.x, stateManager.lPts[listIndex].noise3.y, stateManager.lPts[listIndex].noise3.z, stateManager.lPts[listIndex].noise3.w};
			cs.SetFloats("lNoise", lNois);
			//Debug.Log("------------------------------=======+++++++++"+ stateManager.lPts[listIndex].noise3);
			listIndex++;
		}
		else{//TODO: NOTE: this is what switches the state to the cellular automata state.
			stateManager.cueNextComputeStep();
			//stateManager.currentState = VoxelCubeStateManager.CubeStates.p3_simpleAutomata;
		}


		base.csDispatch();
	}

	/*
	private void setMod(){
		mod = (stateManager.cubeSize/2)* stateManager.cubeSpacing;
	}
	*/
	public override void enterState(){
		//setMod();
		listIndex = 0;
	}
}
