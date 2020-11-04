using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class S7_PP_Normals : VoxelCubeStateBase {

	//this can remain empty if you don't want your state to have a compute shader.
	//public ComputeShader cs; //Defined in this.base
	//private VoxelCubeStateManager stateManager;//Defined in this.base

	//private bool once = true;
	//private int nrOfPPNormalsFramesToRun = 0;
	//private Mesh mesh;
	//private ComputeBuffer outNormsBuff;
	private List<Vector3> outNorms;
	//private List<Vector3> outVerts;
	//private List<int> outTris;

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



		}

		//TODO: Note: the points and the compute buffer are still on the GPU at this point
		// and should remain there, eventhough here you need to send/use the Mesh (not the points) to the GPU.
	}
	
	public override void doUpdate() {
		//base.doUpdate();
		//cs.SetFloat("deltaTime", Time.deltaTime);

		/*
		if(once){
			StartCoroutine(stopStateAfterFrames(nrOfPPNormalsFramesToRun));
			once = false;
		}
		*/

		//base.csDispatch();
	}

	private IEnumerator stopStateAfterFrames(int frames){
		//cs.SetInt("frame", nrOfPPNormalsFramesToRun-frames);

		while (frames > 0){
			//Debug.Log("frames left for automata to run: "+frames);
			frames--;
			yield return 0;//new WaitForSeconds(0);
		}
		
		//TODO: here you must get the normals back from the GPU

		//mesh.normals takes a Vector3. mesh.vertices is another vector3


		//outNormsBuff.GetData(outNorms);


		//Debug.Log("Mesh normals calculated.");
		//stateManager.OnPostProcessingEnded(mesh, outNorms);
	}

	public bool V3Far(Vector3 a, Vector3 b){
		return Vector3.SqrMagnitude(a - b) < 20.0f;//1.0f;//TODO: bind this to normalNeighborCloseness
	}
	public bool V3Close(Vector3 a, Vector3 b){
		//http://answers.unity3d.com/questions/395513/vector3-comparison-efficiency-and-float-precision.html
		/*
			 0.0001 equals 0.01 m, or 1 cm as noticed by @higekun in the comments 
			(for 0.1 mm the value should be 0.00000001)
		 */
		return Vector3.SqrMagnitude(a - b) < stateManager.normalNeighborCloseness;//1.0f;//TODO: bind this to normalNeighborCloseness
	}
	public bool V3Equal(Vector3 a, Vector3 b){
		return Vector3.SqrMagnitude(a - b) < 0.0001f;
	}
	int cToI(int x, int y,int z){
		return x + (stateManager.cubeSize) * (z + (stateManager.cubeSize) * y);
	}
	struct Neighbour
	{
		//public Vector3 vert;
		public Vector3 norm;
		public float avgNoise;
		//public Neighbour(Vector3 _vert, Vector3 _norm, float _avgNoise){
		public Neighbour(Vector3 _norm, float _avgNoise){
			//vert = _vert;
			norm = _norm;
			avgNoise = _avgNoise;
		}
	};
	//int cap = 0;
	/*
	void computeSmoothNormalsCPU(Mesh mesh, VoxelCubeManager.PPParticle[] ppVoxels){
		int tricount = 0;
		int jmod3 = 0;
		//List<Vector3> toutNorms = new List<Vector3>();
		//List<Vector3> toutVerts = new List<Vector3>();
		//List<int> toutTris = new List<int>();
		for(int i=0; i< ppVoxels.Length; i++){
			if(ppVoxels[i] != null){
				LinkedList<Neighbour> ns = new LinkedList<Neighbour>();
				//LinkedList<Neighbour> ns2 = new LinkedList<Neighbour>();
				Vector3 index = new Vector3(
					(ppVoxels[i].position.x - TileFactory.tileList[stateManager.currentTile].tilePos.x) / stateManager.cubeSpacing,
					(ppVoxels[i].position.y - TileFactory.tileList[stateManager.currentTile].tilePos.y) / stateManager.cubeSpacing,
					(ppVoxels[i].position.z - TileFactory.tileList[stateManager.currentTile].tilePos.z) / stateManager.cubeSpacing
					);
				//Fetching all neighbours
				for(int m = -1; m <= 1; m++){
					for(int n = -1; n <= 1; n++){
						for(int o = -1; o <= 1; o++){
							int idx = cToI((int)index.x + m,
							               (int)index.y + n, 
							               (int)index.z + o);
							if(idx>=0 && idx < ppVoxels.Length){
								VoxelCubeManager.PPParticle tempPoint = ppVoxels[idx];
								//if(Vector3.Distance(tempPoint.position, ppVoxels[i].position) > stateManager.cubeSpacing*2){
									//TODO, you sure 1x cubeSpacing is enough?
								//	ppVoxels[i].isEdge = true;
								//	if(tempforcecap< 120){
								//		Debug.Log("IS EDGE ================================================================= ");
								//		Debug.Log ("tempPoint.position: "+tempPoint.position+"; ppVoxels[i].position: "+ppVoxels[i].position+"; stateManager.cubeSpacing: "+stateManager.cubeSpacing+"; Distance: "+Vector3.Distance(tempPoint.position, ppVoxels[i].position));
								//		tempforcecap ++;
								//	}
								//}  
								if(tempPoint != null){

									//This is really shitty (slow).... but to set up a better data structure would mean 1 entire extra pass through the grid..
									for(int k=0; k< tempPoint.verts.Length; k++){
										ns.AddLast(new Neighbour(tempPoint.verts[k], tempPoint.norms[k]));
									}
								}
							}
						}
					}
				}
				//TODO: remove the .isEdge if you don't use it
				//bool isOnTheEdge = false;
				//for each vertex in this voxel, we calculate its smooth normal
				for(int j=0; j< ppVoxels[i].verts.Length; j++){
					if(!ppVoxels[i].isEdge){// || overrideEdgeLvl >0){
						//ns2 = new LinkedList<Neighbour>();
						int neighbourCount = 0;
						int neighbourCount2 = 0;
						Vector3 average = Vector3.zero;
						//Vector3 average2 = Vector3.zero;
						foreach(Neighbour n in ns){
						//for(int k = 0; k<ns.Count; k++){
							if( V3Close(ppVoxels[i].verts[j], n.vert)){
								neighbourCount++;
								average += n.norm;
								if(V3Equal(ppVoxels[i].verts[j], n.vert)){
									neighbourCount2++;
								}
								//if(V3Far(ppVoxels[i].verts[j], n.vert)){
								//	neighbourCount2++;
								//	average2 += n.norm;
								//}
								
								//ns2.AddLast(n);
								//ns.Remove(n);
							}
							//else{
							//	ns2.AddLast(n);
							//}

						}
						//ns = ns2;
						//TODO: NOTE: Values: Flat Shading Weight: "0.3". Normal Neighbor Closeness: "1.7" or "1.5".
						average /= neighbourCount;//+1;
						//average2 /= neighbourCount2;
						//average = ((average + average2)/2 + ppVoxels[i].norms[j] * stateManager.flatShadingWeight)/2;
						average = (average + ppVoxels[i].norms[j] * stateManager.flatShadingWeight)/2;
						average.Normalize();
						jmod3 = j%3;
						if(jmod3==0){
							tricount = outVerts.Count;
						}
						outNorms.Add(average);
						//outTris.Add (ppVoxels[i].tris[j]);
						outTris.Add(tricount + MarchingCubes.windingOrder[jmod3]);
						outVerts.Add(ppVoxels[i].verts[j]);
					}
				}
			}
		}
		Debug.Log(" Mesh normals calculated. outNorms.l: "+outNorms.Count+"; outVerts.l: "+outVerts.Count+"; outTris.l: "+outTris.Count);
		stateManager.OnPostProcessingEnded(mesh, outNorms.ToArray(), outVerts.ToArray(), outTris.ToArray());
	}
	*/
	void computeGemsNormalsCPU(Mesh mesh, VoxelCubeManager.PPParticle[] ppVoxels){

		Vector3[] neighboursx = {
			new Vector3(-1, 0, 0), new Vector3(1, 0, 0), 
			new Vector3(0, -1, 0), new Vector3(0, 1, 0), 
			new Vector3(0, 0, -1), new Vector3(0, 0, 1), 
		};

		Vector3 voxelNormal = Vector3.zero;
		for(int i=0; i< ppVoxels.Length; i++){
			//if(ppVoxels[i] != null && !ppVoxels[i].isEdge){
			if(ppVoxels[i] != null && ppVoxels[i].used && !ppVoxels[i].isEdge){
				
				//Neighbour[] ns = new Neighbour[6];
				float[] ns = new float[neighboursx.Length];
				//Vector3[] ns3 = new Vector3[neighboursx.Length];
				Vector3 ns3 = new Vector3();
				int ns3Count = 0;

				Vector3 index = new Vector3(
					(ppVoxels[i].position.x - TileFactory.tileList[stateManager.currentTile].tilePos.x) / stateManager.cubeSpacing,
					(ppVoxels[i].position.y - TileFactory.tileList[stateManager.currentTile].tilePos.y) / stateManager.cubeSpacing,
					(ppVoxels[i].position.z - TileFactory.tileList[stateManager.currentTile].tilePos.z) / stateManager.cubeSpacing
					);

				//Fetching 6 neighbours
				for(int k= 0; k < 6; k++){
				
					int idx = cToI((int)(index.x + neighboursx[k].x),
					               (int)(index.y + neighboursx[k].y), 
					               (int)(index.z + neighboursx[k].z)
					               );
					//there are fringe cases, I suspect of rendered triangles from outside the cave walls, of points where their neighbours are -20s
					if(idx>=0 && idx < ppVoxels.Length && ppVoxels[idx]!=null){
						ns[k] = ppVoxels[idx].avgNoise;
						//ns[k] = ppVoxels[idx].avgNoise <0? ppVoxels[idx].avgNoise*2 : ppVoxels[idx].avgNoise;
						//if(ns[k]<0){
							//ns[k] *=2;;
						//}
					}
					else
						ns[k] = 0;
				}
				//TODO: inefficient to crawl through the same points twice! do some sort of lookup
				for(int m = -1; m <= 1; m++){
					for(int n = -1; n <= 1; n++){
						for(int o = -1; o <= 1; o++){
							if(!(m == 0 && n == 0 && o == 0)){
								int idx = cToI((int)index.x + m,
								               (int)index.y + n, 
								               (int)index.z + o);
								if(idx>=0 && idx < ppVoxels.Length && ppVoxels[idx]!=null && ppVoxels[idx].used){
									for(int k=0; k< ppVoxels[idx].norms.Length; k+=3){//each vertex normal is the same per triangle
										ns3 += ppVoxels[idx].norms[k];
										ns3Count++;
									}
								}
							}
						}
					}
				}
				//ns3 /= ns3Count;
				ns3 /= ns3Count/4;
				ns3.Normalize();
				/*
				int ct = 0;
				for(int x = -1; x<2; x+=2){
					for(int k= 0; k < 3; k++){
						for(int l= 0; l <3; l++){
						
							int idx = cToI((int)(index.x + x),
							               (int)(index.y + k), 
							               (int)(index.z + l)
							               );
							//TODO: there are fringe cases, I suspect of rendered triangles from outside the cave walls, of points where their neighbours are -20s
							if(idx>=0 && idx < ppVoxels.Length && ppVoxels[idx]!=null){
								ns[ct] += ppVoxels[idx].avgNoise;
							}

						}
					}
					ns[ct] /=9;
					ct++;
				}
				for(int x = -1; x<2; x+=2){
					for(int k= 0; k < 3; k++){
						for(int l= 0; l <3; l++){
							
							int idx = cToI((int)(index.x + k),
							               (int)(index.y + x), 
							               (int)(index.z + l)
							               );
							//TODO: there are fringe cases, I suspect of rendered triangles from outside the cave walls, of points where their neighbours are -20s
							if(idx>=0 && idx < ppVoxels.Length && ppVoxels[idx]!=null){
								ns[ct] += ppVoxels[idx].avgNoise;
							}
							
						}
					}
					ns[ct] /=9;
					ct++;
				}
				for(int x = -1; x<2; x+=2){
					for(int k= 0; k < 3; k++){
						for(int l= 0; l <3; l++){
							
							int idx = cToI((int)(index.x + k),
							               (int)(index.y + l), 
							               (int)(index.z + x)
							               );
							//TODO: there are fringe cases, I suspect of rendered triangles from outside the cave walls, of points where their neighbours are -20s
							if(idx>=0 && idx < ppVoxels.Length && ppVoxels[idx]!=null){
								ns[ct] += ppVoxels[idx].avgNoise;
							}
							
						}
					}
					ns[ct] /=9;
					ct++;
				}
				*/
				voxelNormal.x = ns[0] - ns[1];
				voxelNormal.y = ns[2] - ns[3];
				voxelNormal.z = ns[4] - ns[5];
				voxelNormal *=-1;
				//voxelNormal = -voxelNormal.normalized;

				for(int j=0; j< ppVoxels[i].norms.Length; j++){

					//ppVoxels[i].norms[j] = (voxelNormal + ppVoxels[i].norms[j] * stateManager.flatShadingWeight)/2;
					//ppVoxels[i].norms[j].Normalize();

					//voxelNormal = ppVoxels[i].norms[j] + voxelNormal;
					//voxelNormal = (ppVoxels[i].norms[j] + voxelNormal)/2;
					//voxelNormal.Normalize();


					//voxelNormal = (ns3 + voxelNormal)/2;
					//voxelNormal = ns3*0.3f + voxelNormal*0.7f;
					//voxelNormal.Normalize();

					//voxelNormal = (ppVoxels[i].norms[j] + (ns3 + voxelNormal));

					voxelNormal = ppVoxels[i].norms[j] + ns3 + voxelNormal;
					//Vector3 noise = (SimplexNoise.Noise.get_Curl(voxelNormal/50)+Vector3.one)*0.35f;
					//voxelNormal += noise;

					//Vector3 d = Vector3.up;
					//voxelNormal = new Vector3(voxelNormal.x * d.x, voxelNormal.y * d.y, voxelNormal.z * d.z);

					//No Man's Normals
					//voxelNormal = ppVoxels[i].norms[j] + voxelNormal;
					//Vector3 d = (voxelNormal - ns3.normalized).normalized /2;
					//voxelNormal = (voxelNormal + 
					//               new Vector3(voxelNormal.x * d.x, voxelNormal.y * d.y, voxelNormal.z * d.z));
	
					//voxelNormal = (ppVoxels[i].norms[j]*1.75f + (ns3 + voxelNormal*1.5f));
					//voxelNormal = ((ns3 + voxelNormal));
					//voxelNormal = (ppVoxels[i].norms[j] + (ns3));
					//voxelNormal = (ppVoxels[i].norms[j] *0.5f + (ns3 + voxelNormal)/2)/2;
					voxelNormal.Normalize();

					outNorms.Add(voxelNormal);
					//outNorms.Add(ppVoxels[i].norms[j]);
				}


			}
		}
		#if UNITY_EDITOR
		Debug.Log(" Mesh normals calculated. outNorms.l: "+outNorms.Count+";");
		#endif
		stateManager.OnPostProcessingEnded(mesh, outNorms.ToArray());
	}

	//this is a second, custom, doStart()
	public void setMesh(Mesh mesh, VoxelCubeManager.PPParticle[] ppVoxels){
		//mesh = _mesh;	


		outNorms = new List<Vector3>();
		//outVerts = new List<Vector3>();
		//outTris = new List<int>();

		computeGemsNormalsCPU(mesh, ppVoxels);



		//outNorms = new Vector3[mesh.normals.Length];
		//if(outNormsBuff != null)
		//		outNormsBuff.Release();
		//outNormsBuff = new ComputeBuffer(195000, 36, ComputeBufferType.Append);
		//outNormsBuff.SetData(outNorms);
		//cs.SetBuffer(0, "outNormsASB", outNormsBuff);
		//cs.SetBuffer(0, "postProcessBuffer", stateManager.postProcessBuffer);

		//Debug.Log("PP Buffer set!");
		//TODO: load mesh to compute shader before the next update arrives.
	}

	public override void enterState(){

		
		//once = true;
	}
}
