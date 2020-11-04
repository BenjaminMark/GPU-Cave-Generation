using UnityEngine;
using System.Collections;

public class DebugNormals : MonoBehaviour {
	Mesh debugMesh;
	Vector3[] vertices;
	Vector3[] normals;

	Color[] colors = {Color.cyan, Color.magenta, Color.yellow, Color.red, Color.green, Color.blue, Color.white, Color.gray, Color.black};

	VoxelCubeManager stateMan;

	// Use this for initialization
	void Start () {
		debugMesh = GetComponent<MeshFilter>().mesh;
		vertices  = debugMesh.vertices;
		normals  = debugMesh.normals;

		for (int i = 0 ; i < vertices.Length; i++){
			
			vertices[i] =  transform.TransformPoint(vertices[i] );

		}

		stateMan = GameObject.FindGameObjectWithTag("particle").GetComponent<VoxelCubeManager>();
	}


	int ci = 0;
	int ci2 = 0;
	void Update () {
		if(stateMan.drawNormalLines){
			for (int i = 0 ; i < vertices.Length; i++){
					
				//vertices[i] =  transform.TransformPoint(vertices[i] );
					
				//Debug.DrawLine (vertices[i], vertices[i] + (normals[i] * 1f), new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f)));
				Debug.DrawLine (vertices[i], vertices[i] + normals[i], colors[ci2]);
				ci++;
				if(ci%3==0){
					Debug.DrawLine (vertices[i]+ normals[i], vertices[i-1]+ normals[i-1], colors[ci2]);
					Debug.DrawLine (vertices[i-1]+ normals[i-1], vertices[i-2]+ normals[i-2], colors[ci2]);
					Debug.DrawLine (vertices[i]+ normals[i], vertices[i-2]+ normals[i-2], colors[ci2]);
					ci2++;
				}

				if(ci>=colors.Length)
					ci = 0;
				if(ci2>=colors.Length)
					ci2=0;
			}
			ci=0;
			ci2=0;
		}
	}
}
