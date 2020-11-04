using UnityEngine;
using System.Collections;

public class S4_LandMarks : VoxelCubeStateBase {
	
	//this can remain empty if you don't want your state to have a compute shader.
	//public ComputeShader cs; //Defined in this.base
	//private VoxelCubeStateManager stateManager;//Defined in this.base
	
	private bool once = true;
	
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
			//cs.SetFloats("sprayOrbRadius", stateManager.sprayOrbRadius);//0.6f);
			//cs.SetInt("targetChanged", 1);
			cs.SetInt("cubeSize", stateManager.cubeSize);//for automatas
			cs.SetFloats("spacing", stateManager.cubeSpacing);//0.6f);//for automatas
			//cs.SetInt ("seedMod", Random.Range(0, int.MaxValue));
			cs.SetFloats("randomSeed", stateManager.randomSeed);

			cs.SetFloat("rangeModulator", stateManager.settings.stalactitePopulation);
			cs.SetFloat("attachmentPointDensity", stateManager.settings.attachmentPointDensity);
			//cs.SetInt ("frameTotal", stateManager.nrOfLandMarkFramesToRun);
			cs.SetFloat("stalactiteStraightness", stateManager.settings.stalactiteSharpness);
			cs.SetFloat("fineTuneThickness", stateManager.settings.airThickness);
			cs.SetFloat("maxStHeight", stateManager.settings.maxStHeight);
			cs.SetInt("stthick", stateManager.settings.hoodoos);
			cs.SetFloat("hooWobbliness", stateManager.settings.wobbliness);

		}
	}


	public override void doUpdate() {
		//base.doUpdate();
		//cs.SetFloat("deltaTime", Time.deltaTime);
		//base.handleMouseOrb();
		
		if(once){
			StartCoroutine(stopStateAfterFrames(stateManager.nrOfLandMarkFramesToRun));
			once = false;
		}
		
		base.csDispatch();
	}
	
	private IEnumerator stopStateAfterFrames(int frames){
		
		
		while (frames > 0){
			cs.SetInt ("frameNumber", stateManager.nrOfLandMarkFramesToRun - frames);
			//Debug.Log("frames left for automata to run: "+frames);
			frames--;
			if(frames == 0){
				cs.SetInt ("frameNumber", -10); //This is to tell the shader it is on the last frame.
			}
			yield return 0;//new WaitForSeconds(0);
		}


		
		stateManager.cueNextComputeStep();
		stateManager.OnComputingEnded();
	}
	
	public override void enterState(){
		float[] tileP = {
			TileFactory.tileList[stateManager.currentTile].tilePos.x,
			TileFactory.tileList[stateManager.currentTile].tilePos.y,
			TileFactory.tileList[stateManager.currentTile].tilePos.z
			//TileFactory.tileList[stateManager.currentTile].tileID.x+1,
			//TileFactory.tileList[stateManager.currentTile].tileID.y+1,
			//TileFactory.tileList[stateManager.currentTile].tileID.z+1
		};
		cs.SetFloats("tilePos", tileP);
		cs.SetInt ("frameNumber", 0);
		
		once = true;
	}
	
}