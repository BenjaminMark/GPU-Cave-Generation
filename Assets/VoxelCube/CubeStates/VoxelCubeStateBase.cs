using UnityEngine;
using System.Collections;

public class VoxelCubeStateBase : MonoBehaviour {

	//this can remain empty if you don't want your state to have a compute shader.
	public ComputeShader cs;

	internal VoxelCubeManager stateManager;

	internal float[] target = {0f, 0f, 0f };
	internal float[] prevTarget = {0f, 1f, 0f};

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	/// <summary>
	/// Takes the StateManager's Compute Buffer and assigns it to this state's compute shader.
	/// Also assigns whatever Floats or Ints etc. the state's compute shader needs.
	/// </summary>
	public virtual void doStart(VoxelCubeManager _stateManager) {
		stateManager = _stateManager;

		if(cs){
		

		}
	}

	public virtual void doUpdate() {
		// Get the mouse position in the 3D space (a flat box collider catches the ray) .
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit info = new RaycastHit();
		if (Physics.Raycast (ray, out info)) 
		{
			target[0] = info.point.x;
			target[1] = info.point.y;
			target[2] = info.point.z;
		}
	}

	/// <summary>
	/// Start the compute shader (move every particle for this frame) if one is available.
	/// </summary>
	public void csDispatch(){
		if(cs){
			cs.Dispatch(0, stateManager.warp_Count, 1, 1);
		}
	}

	public void handleMouseOrb(){
		// Set the compute shader variables for this frame.

		cs.SetFloat("targetStrengh", stateManager.mouseStrengh);//*msSign);
		
		if(UtilityScript.arrayApproximately(prevTarget, target)){
			cs.SetInt("targetChanged", 0);

		}
		else{
			cs.SetInt("targetChanged", 1);
			cs.SetFloats("target", target);
		}
		prevTarget[0] = target[0];//(float[])target.Clone();
		prevTarget[1] = target[1];
		prevTarget[2] = target[2];
	}

	public virtual void enterState() {

	}

	public virtual void exitState() {
		
	}

	public void saySomething(){
		Debug.Log("Something! [VoxelCubeStateBase]");
	}

}
