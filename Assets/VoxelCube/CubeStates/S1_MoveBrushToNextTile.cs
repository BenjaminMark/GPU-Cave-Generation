using UnityEngine;
using System.Collections;

public class S1_MoveBrushToNextTile : VoxelCubeStateBase {

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
			
			//base.
			cs.SetBuffer(0, "particleBuffer", stateManager.particleBuffer);

			//cs.SetFloats("sprayOrbRadius", stateManager.sprayOrbRadius);//0.6f);
			//cs.SetInt("targetChanged", 1);

		}
	}

	/// <summary>
	/// 
	/// </summary>
	public override void doUpdate() {
		//base.doUpdate();
		//cs.SetFloat("deltaTime", Time.deltaTime);
		//base.handleMouseOrb();

		Vector3 prevPt = new Vector3(0, 0, 0);
		if(stateManager.currentTile>0){
			prevPt = TileFactory.tileList[stateManager.currentTile-1].tilePos;
		}
		Vector3 newPos = TileFactory.tileList[stateManager.currentTile].tilePos - prevPt;
		//newPos*= stateManager.lSystemResolution;//TODO
		//Debug.Log("><><><><>< newPos: "+newPos.x+"; "+newPos.y+"; "+newPos.z);
		float[] p = {newPos.x, newPos.y, newPos.z};
		cs.SetFloats("offsetVal", p);

		cs.SetFloat("noiseVal", -20);
		cs.SetFloat("flagsVal", -20);

		float[] zero = {0, 0, 0};
		cs.SetFloats("velocVal", zero);

		if(once){
			StartCoroutine(stopStateAfterFrames(0));
			once = false;
		}

		base.csDispatch();
	}

	private IEnumerator stopStateAfterFrames(int frames){
		
		while (frames > 0){
			//Debug.Log("<><><><><><><><><<><><><>frames left for MoveBrush to run: "+frames);
			frames--;
			yield return 0;//new WaitForSeconds(0);
		}

		//stateManager.postProcessBuffer.GetData(stateManager.postProcessArray);

		stateManager.cueNextComputeStep();
	}

	/*
	private float mod = 0;
	private void setMod(){
		mod = (stateManager.cubeSize/2)* stateManager.cubeSpacing;
	}
	*/

	public override void enterState(){
		//cs.SetBuffer(0, "postProcessBuffer", stateManager.postProcessBuffer);
		once = true;
	}

}
