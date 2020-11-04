using UnityEngine;
using System.Collections;

public class ResizeMeshBounds_CullFix : MonoBehaviour {
	
	//public GameObject announcerQuad;
	
	// Use this for initialization
	public Camera cam;
	void Start () {
		
		/*
		 * http://forum.unity3d.com/threads/71051-Disable-Frustum-Culling
		 * http://docs.unity3d.com/Documentation/ScriptReference/Mesh-bounds.html
	 	*/
		/*
	 	Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        Bounds bounds = mesh.bounds;
        int i = 0;
        while (i < uvs.Length) {
            uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.x);
            i++;
        }
        mesh.uv = uvs;	
        */
        
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		//mesh.RecalculateBounds();
		//Debug.Log(mesh.bounds.center+"; "+mesh.bounds.size);

		//camTransform = cam.transform;// Camera.main.transform;
		///distToCenter = (Camera.main.farClipPlane - Camera.main.nearClipPlane) / 2.0f;
		//distToCenter = (cam.farClipPlane - cam.nearClipPlane) / 2.0f;
		//extremeBound = -1;// 500.0f; 
		//meshFilter = transform.renderer.GetComponent<MeshFilter>();



		//meshFilter.sharedMesh.bounds = new Bounds(transform.position, Vector3.one * extremeBound);
		//meshFilter.sharedMesh.bounds = new Bounds(transform.position, Vector3.forward * extremeBound);
		//Vector3 boundsPos = transform.position + Vector3.forward * extremeBound;

		//meshFilter.sharedMesh.bounds = new Bounds(boundsPos, meshFilter.sharedMesh.bounds.size);
		//meshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(1,1,0));
		mesh.bounds = new Bounds(new Vector3(0,0,10), new Vector3(1,1,0));

		//originalMeshBounds = meshFilter.sharedMesh.bounds;
		//expandMesh = false;
	}


	
	//Bounds originalMeshBounds;
	
	//Transform camTransform;
	//float distToCenter;
	//float extremeBound;
	//MeshFilter meshFilter;
	/*
	private bool expandMesh;
	public void expandMeshBounds(bool expand){
		if(expand){
			expandMesh = true;
			transform.renderer.enabled = true;
			//Debug.Log("Expanded. "+transform.parent.name);
		}
		else {
			expandMesh = false;
			//meshFilter.sharedMesh.bounds = originalMeshBounds;
			//meshFilter.mesh.RecalculateBounds();
			transform.renderer.enabled = false;
			//Debug.Log("Contracted. "+transform.parent.name);
		}
	}*/
	
	// Update is called once per frame
	//[ExecuteInEditMode]
	void Update () {
		/*
		//http://answers.unity3d.com/questions/36446/disable-frustum-culling.html
		
		//TODO: only activate this when monster is near the guy. Restore to original otherwise.
		if( expandMesh){
			
			Vector3 center = camTransform.position + camTransform.forward * distToCenter;
			meshFilter.sharedMesh.bounds = new Bounds(center, Vector3.one * extremeBound);
			
			//meshFilter.sharedMesh.bounds = new Bounds(transform.position, Vector3.one * extremeBound);
			//Debug.Log ("supposedly recalculating bounds to centre of camera: "+ center);
		}
		*/
	}
}
