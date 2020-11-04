using UnityEngine;
using System.Collections;

public class AlwaysFaceCamera : MonoBehaviour {

	public Camera targetCamera;

	// Use this for initialization
	void Start () {
	
	}
	

	void FixedUpdate () {

		transform.rotation = targetCamera.transform.rotation;
	}
}
