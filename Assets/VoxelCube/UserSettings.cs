using UnityEngine;
using System.Collections;

public class UserSettings : MonoBehaviour {

	public float sprayOrbRadius = 3;//0.5f;
	public float sprayOrbOuterRadius = 1;

	[Range(-0.95f, -0.874f)]
	public float stalactitePopulation = 0.93f;
	[Range(0.01f, 0.45f)]
	public float attachmentPointDensity = 0.5f;
	//public float stalactiteGrouping = -0.75;
	[Range(0.025f, 3.0f)]
	public float stalactiteSharpness = 2.0f;
	[Range(0.5f, 1.75f)]
	public float airThickness = 1.0f;

	[Range(0.15f, 1.5f)]
	public float maxStHeight = 1;
	[Range(0, 1)]
	public int hoodoos = 0;
	[Range(0.0f, 0.8f)]
	public float wobbliness = 0;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
