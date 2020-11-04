using UnityEngine;
using System.Collections;
using System;

public class DisplayMeshBounds : MonoBehaviour {

	#if UNITY_EDITOR
	//|| UNITY_STANDALONE 

	// Use this for initialization
	void Start () {
		meshBoundsOut = transform.renderer.bounds.size;

		if(transform.gameObject.layer == 31){
			gizmoColor = Color.grey;
		}

		HandleMeshSave = doActualMeshSave;
	}

	private Color gizmoColor = Color.yellow;

	public bool _bIsSelected = true;
	public Vector3 meshBoundsOut = Vector3.zero;
	public bool SAVE_this_mesh = false;
	
	void OnDrawGizmos()
	{
		if (_bIsSelected)
			OnDrawGizmosSelected();
	}
	
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = gizmoColor;
		//Gizmos.color = Color.yellow;
		//Gizmos.DrawCube(transform.position, Vector3.one);  //center sphere
		if (transform.renderer != null){

			//Gizmos.DrawWireCube(transform.position, transform.renderer.bounds.size);
			Gizmos.DrawWireCube(transform.renderer.bounds.center, transform.renderer.bounds.size);

		}
	}

	private Action HandleMeshSave;// = DoNothing;
	
	private void DoNothing(){
	}

	bool once = true;
	// Update is called once per frame
	void Update () {
		HandleMeshSave();

	}

	
	private void doActualMeshSave(){
		if(once && SAVE_this_mesh){
			UnityEditor.AssetDatabase.CreateAsset(gameObject.GetComponent<MeshFilter>().mesh, "Assets/exportedCaveTiles/"+"manSave_"+gameObject.name+"_"+Time.time+".asset");
			once = false;
			HandleMeshSave = DoNothing;
		}
	}
	#endif

}
