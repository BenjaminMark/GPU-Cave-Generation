using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This is a modified version of Scrawk's Marching Cubes implementation. The source is in the Plugins folder
public class CubeMarcher : MonoBehaviour {

	//The size of voxel array. Be carefull not to make it to large as a mesh in unity can only be made up of 65000 verts
	/*
	public int width = 32;
	public int height = 32;
	public int length = 32;
	*/

	public Material m_material;
	public Material ice_material;
	public Material canyon_material;
	public GameObject m_meshContainer;
	public InputManager inputMan;

	VoxelCubeManager particleCube;
	MarchingCubes marchingCubes;

	private Vector3 currentTileID;

	void Start(){

		particleCube = gameObject.GetComponent<VoxelCubeManager>();
		marchingCubes = GetComponent<MarchingCubes>();


	}


	void Update(){	}

	public Mesh CreateMesh(VoxelCubeManager.VoxelParticle[] voxels, VoxelCubeManager.PPParticle[] ppVoxels)//, Vector3 tileID) 
	{

		//Target is the value that represents the surface of mesh
		//For example the perlin noise has a range of -1 to 1 so the mid point is were we want the surface to cut through
		//The target value does not have to be the mid point it can be any value with in the range
		//MarchingCubes.SetTarget(0.5f);
		MarchingCubes.SetTarget(0.0f);
		
		//Winding order of triangles use 2,1,0 or 0,1,2
		MarchingCubes.SetWindingOrder(2, 1, 0);//inner mesh
		//MarchingCubes.SetWindingOrder(0, 1, 2);//outer mesh
		
		//Set the mode used to create the mesh
		//Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface
		if(particleCube.marchingTetrahedrons){
			marchingCubes.SetModeToTetrahedrons();
		}
		else{
			marchingCubes.SetModeToCubes();
		}



		/*
		float[,,] voxels = new float[width, height, length];
		
		int x;
		int y;
		int z;
		
		//Fill voxels with values. Im using perlin noise but any method to create voxels will work
		for(x = 0; x < width; x++)
		{
			for(y = 0; y < height; y++)
			{
				for(z = 0; z < length; z++)
				{
					voxels[x,y,z] = m_perlin.FractalNoise3D(x, y, z, 3, 40.0, 1.0); 
				}
			}
		}
		*/

		//Debug.Log ("Marching cubes - About to create mesh from list of voxels [this is CubeMarcher].");

		//Mesh mesh = marchingCubes.CreateMeshFromList(voxels);
		//return marchingCubes.CreateMeshFromList(voxels);
		return marchingCubes.CreateMeshFromList(voxels, ppVoxels);

		/*
		if(mesh.vertices.Length >0){
			//The diffuse shader wants uvs so just fill with a empty array, they're not actually used
			mesh.uv = new Vector2[mesh.vertices.Length];
			mesh.RecalculateNormals();
			
			GameObject m_mesh = new GameObject("Mesh");
			m_mesh.transform.parent = m_meshContainer.transform;
			m_mesh.name = "tile["+tileID.x+"]["+tileID.y+"]["+tileID.z+"]";
			m_mesh.AddComponent<MeshFilter>();
			m_mesh.AddComponent<MeshRenderer>();
			m_mesh.renderer.material = m_material;
			m_mesh.GetComponent<MeshFilter>().mesh = mesh;
			//Center mesh
			//m_mesh.transform.localPosition = Vector3(-width/2, -height/2, -length/2);
			//m_mesh.transform.position = new Vector3((particleCube.cubeSize*particleCube.cubeSpacing)/2, (particleCube.cubeSize*particleCube.cubeSpacing)/2, (particleCube.cubeSize*particleCube.cubeSpacing)/2);
			m_mesh.transform.localScale *= particleCube.cubeSpacing;

			//UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/exportedCaveTiles/"+m_mesh.name+".asset");
			m_mesh.AddComponent<MeshCollider>();
			m_mesh.GetComponent<MeshCollider>().sharedMesh = mesh;


			//Debug.Log ("Marching cubes - attached m_mesh components [back inside CubeMarcher].");
		}
		else{
			Debug.Log("DISCARDED A MESH WITH 0 VERTICES_____________________!@#$#%#^$%&%^*&(*)) ");
			//Debug.Log("!@#$#%#^$%&%^*&(*))*&^$^$*&(*(&%@#$!@%$#**()%%^@#$$#_______________");


		}
		*/
	}

	private float minSide = 5;
	private float minSum = 35;
	public void applyPostProcessedMesh(Mesh mesh, Vector3[] outNorms, Vector3 tileID, int id){
	//public void applyPostProcessedMesh(Mesh mesh, Vector3[] outNorms, Vector3[] outVerts, int[] outTris, Vector3 tileID){

		if(tileID.x==0 && tileID.y==0 && tileID.z==-1){
			return;
		}

		//mesh.RecalculateNormals();// this MAKES normals. don' use it if you make your own

		//mesh.vertices = outVerts;
		mesh.normals = outNorms;
		//mesh.triangles = outTris;
		//The diffuse shader wants uvs so just fill with a empty array, they're not actually used
		mesh.uv = new Vector2[mesh.vertices.Length];

		//TODO might be slow. http://docs.unity3d.com/Documentation/ScriptReference/Mesh.Optimize.html \
		//mesh.tangents = CalculateTangents(mesh);
		mesh.Optimize();

		GameObject m_mesh = new GameObject();//new GameObject("Mesh");
		m_mesh.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
	
		m_mesh.transform.position = new Vector3(m_mesh.transform.position.x+7, m_mesh.transform.position.y+7, m_mesh.transform.position.z+7);
		//m_mesh.transform.position = TileFactory.tileList[particleCube.currentTile].tileCentre;


		m_mesh.transform.parent = m_meshContainer.transform;
		m_mesh.name = "tile["+tileID.x+"]["+tileID.y+"]["+tileID.z+"]";
		m_mesh.AddComponent<MeshFilter>();
		m_mesh.AddComponent<MeshRenderer>();
		//m_mesh.renderer.material = m_material;
		m_mesh.renderer.sharedMaterial = m_material;
		m_mesh.GetComponent<MeshFilter>().mesh = mesh;


		//TODO: important, for depth testing, and also important to do it AFTER adding it to the mesh filter.
		//m_mesh.GetComponent<MeshFilter>().mesh.RecalculateBounds();
		//after visualization turns out the bounds were fine.
		//m_mesh.renderer.bounds.center = m_mesh.renderer.bounds.

		/*
		Vector3 pos = m_mesh.transform.position;
		pos.x -= TileFactory.tileList[particleCube.currentTile].tileCentre.x;
		pos.y -= TileFactory.tileList[particleCube.currentTile].tileCentre.y;
		pos.z -= TileFactory.tileList[particleCube.currentTile].tileCentre.z;
		m_mesh.transform.position = pos;
		*/
		//m_mesh.transform.position = TileFactory.tileList[particleCube.currentTile].tilePos;


		//Center mesh
		//m_mesh.transform.localPosition = Vector3(-width/2, -height/2, -length/2);
		//m_mesh.transform.position = new Vector3((particleCube.cubeSize*particleCube.cubeSpacing)/2, (particleCube.cubeSize*particleCube.cubeSpacing)/2, (particleCube.cubeSize*particleCube.cubeSpacing)/2);
		m_mesh.transform.localScale *= particleCube.cubeSpacing;
			
		//save mesh
		//UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/exportedCaveTiles/"+m_mesh.name+".asset");
		m_mesh.AddComponent<MeshCollider>();
		m_mesh.GetComponent<MeshCollider>().sharedMesh = mesh;
		//m_mesh.GetComponent<MeshCollider>().sharedMesh.

		//TODO: remove this
		//m_mesh.AddComponent<DebugNormals>();

		m_mesh.isStatic = true;

		if(inputMan.lsysMan.showMeshBoundingBoxes){
			m_mesh.AddComponent<DisplayMeshBounds>();
		}
		if(inputMan.lsysMan.doDynamicOcclusionCulling){



			Vector3 b = m_mesh.renderer.bounds.size;
			if( b.x < minSide || b.y < minSide || b.z < minSide ||
			   b.x + b.y + b.z < minSum){
				m_mesh.layer = 31;
				m_mesh.renderer.enabled = true;
			}
			else{
				m_mesh.layer = 30;
				m_mesh.renderer.enabled = false;
			}
		}

		//Adds the mesh to the cleanupList so it can be removed when the user switches to a new l-system at runtime.
		GetComponent<CleanUpList>().objects.Add(m_mesh);

		//a primitive offset to fight z-fighting (no longer needed)
		/*
		m_mesh.transform.position = new Vector3(m_mesh.transform.position.x,
		                                        m_mesh.transform.position.y + (id%9)*0.045f,//(id%9)*0.05f,
		                                        m_mesh.transform.position.z);
		*/
		//m_mesh.renderer.sharedMaterial.SetVector("Noise Seed", new Vector4(particleCube.randomSeed[0], particleCube.randomSeed[1], particleCube.randomSeed[2], 0));

		//Debug.Log ("Marching cubes - attached m_mesh components [back inside CubeMarcher].");

	}

	private Vector4[] CalculateTangents(Mesh mesh)
		
	{
		
		int triangleCount = mesh.triangles.Length;
		
		int vertexCount = mesh.vertices.Length;
		
		
		
		var tan1 = new Vector3[vertexCount];
		
		var tan2 = new Vector3[vertexCount];
		
		
		
		var tangents = new Vector4[vertexCount];
		
		
		
		for (int a = 0; a < triangleCount; a += 3)
			
		{
			
			int i1 = mesh.triangles[a + 0];
			
			int i2 = mesh.triangles[a + 1];
			
			int i3 = mesh.triangles[a + 2];
			
			
			
			var v1 = mesh.vertices[i1];
			
			var v2 = mesh.vertices[i2];
			
			var v3 = mesh.vertices[i3];
			
			
			
			var w1 = Vector2.zero;//_uvs[i1];
			
			var w2 = Vector2.zero;//_uvs[i2];
			
			var w3 = Vector2.zero;//_uvs[i3];
			
			
			
			float x1 = v2.x - v1.x;
			
			float x2 = v3.x - v1.x;
			
			float y1 = v2.y - v1.y;
			
			float y2 = v3.y - v1.y;
			
			float z1 = v2.z - v1.z;
			
			float z2 = v3.z - v1.z;
			
			
			
			float s1 = w2.x - w1.x;
			
			float s2 = w3.x - w1.x;
			
			float t1 = w2.y - w1.y;
			
			float t2 = w3.y - w1.y;
			
			
			
			float r = 1.0f / (s1 * t2 - s2 * t1);
			
			
			
			var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			
			var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
			
			
			
			tan1[i1] += sdir;
			
			tan1[i2] += sdir;
			
			tan1[i3] += sdir;
			
			
			
			tan2[i1] += tdir;
			
			tan2[i2] += tdir;
			
			tan2[i3] += tdir;
			
		}
		
		
		
		for (int a = 0; a < vertexCount; ++a)
			
		{
			
			var n = mesh.normals[a];
			
			var t = tan1[a];
			
			
			
			var tmp = (t - n * Vector3.Dot(n, t)).normalized;
			
			tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
			
			tangents[a].w = Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f ? -1.0f : 1.0f;
			
		}
		
		
		
		return tangents;
		
	}

}
