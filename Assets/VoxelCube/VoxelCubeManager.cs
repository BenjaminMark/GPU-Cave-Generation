using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class VoxelCubeManager : MonoBehaviour 
{
	//http://www.yoda.arachsys.com/csharp/singleton.html
	//TODO: check this out: https://www.shadertoy.com/view/XslGRr
	//http://webstaff.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf
	
	public InputManager inputManager;
	public CubeMarcher cubeMarcher;


	public enum ComputeStates{
	
		p0_DoNothing = 0,
		p1_MoveBrushToNextTile = 1,
		p2_LoadAndNoise = 2,
		//p3_CellularAutomata = 3,
		p4_LandMarks = 3,
		//p5_VoxelFilteringCA = 4,
		p6_DisplayOnly = 4,
		p7_PP_Normals = 5
	}
	private ComputeStates _currentState;
	public ComputeStates currentState{//clever because with the setter we can not only set the enum, but also configure the state
		get{
			return _currentState;
		}
		set{
			_currentState = value; 
			OnEnterState();
		}
	}

	public GameObject cubeStateContainer;
	public GameObject LSysPrevContainer;
	public Camera auxCamera;
	[HideInInspector]
	public VoxelCubeStateBase[] cubeStateObjects;

	//public Action DoStart = DoNothing;
	public Action DoUpdate = DoNothing;
	//public Func<IEnumerator> ExitState = DoNothingCoroutine;
	public Action ExitState = DoNothing;
	public Action DoRender = DoNothing;// = RenderNothing;


	// The same particle data structure used by both the compute shader and the shader.
	public struct VoxelParticle
	{
		public Vector3 position;
		public Vector3 velocity;
		public float noise;
		public float prevNoise;
		public int flags;
	};
	public class PPParticle
	{
		public Vector3 position;
		public bool used;
		public bool isEdge = false;
		//public Vector3[] verts;
		public Vector3[] norms;
		//public int[] tris;
		public float avgNoise;
	};

	[HideInInspector]
	public List<TileFactory.LPoint> lPts;
	private int _currentTile = 0;
	public int currentTile{
		get{ 
			return _currentTile;
		}
	}
	public static List<VoxelCubeManager> list;	// Static list, just to call Render() from the camera.

	[HideInInspector]
	public int warp_Count = 10;				// The number particle /32.
	

	public float mouseStrengh = 100;		// velocity change from the mouse position.



	public float particleCubeSize = 1f;//1.9f;//should be the same as "spacing"; unless you want the cubes to be smaller than the grid.
	//public float sprayOrbRadius = 3;//0.5f;
	//public float sprayOrbOuterRadius = 1;
	
	public float lSystemResolution = 2;
	public int nrOfAutomataFramesToRun = 1;
	public int nrOfLandMarkFramesToRun = 15;
	public Material material;				// Use "particle" shader.
	public Material materialDark;				// Use "particle" shader.
	public Material materialBright;				// Use "particle" shader.
	//public bool enableMeshGeneration = true;
	//public float delayBeforeMeshGeneration = 1;
	public float ClipNrOfVerts0_to_1 = 1;
	public bool marchingTetrahedrons = false;



	public bool drawNormalLines = false;//also uncomment DebugNormals in the cubeMarcher
	[HideInInspector]
	public float flatShadingWeight = 0;
	[HideInInspector]
	public float normalNeighborCloseness = 1;
	public bool useFog = true;

	[HideInInspector]
	public UserSettings settings;
	public UserSettings settings1;
	public UserSettings settings2;
	public int whichSettingToUse = 2;
	[Range(0f, 0.18f)]
	//[Range(0f, 0.2f)]
	public float fineDetailYSlope = 0.18f;
	//[Range(1f, 150f)]
	[Range(0.001f, 150f)]
	public float noiseOctave = 100;
	//[Range(6f, 30f)]//upper limit should be between radius and outer radius
	[Range(6f, 30f)]//upper limit should be between radius and outer radius
	public float noiseDistAtten = 6;
	[Range(0.1f, 0.5f)]//using lerp.//the lsys curl noise is always >0.something
	public float metaballDistortionFade = 0.5f;//1.3f;


	public VoxelParticle[] particleArray;
	public PPParticle[] postProcessArray;

	public bool useOldCurlNoiseInstead = false;
	public Vector3 yourManualSeed = Vector3.zero;
	public bool useManualSeed = false;
	[HideInInspector]
	public bool frostySeed = false;
	[HideInInspector]
	public float[] randomSeed; 

	const int warpSize = 32; 				// GPUs process data by warp, 32 for every modern ones.
	[HideInInspector]
	public int particleCount; 						// = warpSize * warpCount.
	[HideInInspector]
	public int cubeSize;
	[HideInInspector]
	public float cubeSpacing = 1;//2f;

	[HideInInspector]
	//public ComputeBuffer postProcessBuffer;	// The GPU buffer holding the particules.
	public ComputeBuffer particleBuffer;	// The GPU buffer holding the particules.
	ComputeBuffer cubedVertBuffer;
	
	void Awake ()
	{

		//Application.targetFrameRate = 60;
		DoRender = actualRender;
		//DoRender = RenderNothing;
	}

	void Start () 
	{


		// Just init the static list 
		if (list == null)
			list = new List<VoxelCubeManager>();
		list.Add(this);

		SetSettings();
		SetSeed();//ComputeRandomSeed();
		Sync_CubeStateObjects_with_StateEnums();

		//64;
		cubeSize = 32;//28 + MarchingCubes.buff*2; //28 might exceed max allowed vertex count if the zone isn't hollow enough
		//Worst case scenarios, in which the cube has no unused/unassigned voxels:
		//	-for marching cubes with automata'd empty space, an approx limit is 28^3. <=> ~56478 / 65000 verts.
		//	-for marching cubes with only random noise, an approx limit is 21^3. <=> ~61023 / 65000 verts.
		//	-for marching tetrahedrons with automata'd empty space, an approx limit is 20^3. <=> ~55968 / 65000 verts.
		//	-for marching tetrahedrons with only random noise, an approx limit is 16^3. <=> ~53916 / 65000 verts.
		//particleCount = warpCount * warpSize;
		particleCount = cubeSize*cubeSize*cubeSize;//32768;//262144;//216000;
		warp_Count = particleCount / warpSize;
		Debug.Log("particleCount: "+particleCount+"; warpCount: "+warp_Count+"; cubeSize: "+cubeSize+"x"+cubeSize+"x"+cubeSize);
		//TODO figure out how is it to use 32, 32, 32
		
		// Init particles
		particleArray = new VoxelParticle[particleCount];
		//postProcessArray = new PPParticle[particleCount];



		//int temp = 0;
		//this creates the voxel cube. the initial positions
		int i = 0;
		int num = cubeSize;//60;//6
		for(int y=0; y < num; y++){
			for(int z=0; z < num; z++){
				for(int x=0; x < num; x++){
					//if(temp < 1064){
					//	temp++;
					particleArray[i].position = new Vector3(x * cubeSpacing, y * cubeSpacing, z * cubeSpacing);
					//particleArray[i].position /= 4;
					//particleArray[i].position = Random.insideUnitSphere * initialSpread;
					//particleArray[i].velocity = particleArray[i].position.normalized;
					particleArray[i].velocity = Vector3.zero;
					particleArray[i].noise = -20;
					particleArray[i].flags = -20;

					//postProcessArray[i].position = particleArray[i].position;
					//postProcessArray[i].used = 0;
					//postProcessArray[i].verts = new Vector3[15];
					//postProcessArray[i].norms = new Vector3[15];


					i++;
					//}
				}

			}
		}

		//this defines a cube around point (0, 0, 0). this will be used in addition to each point's position in the shader.
		Vector3[] cubeVerts = createBaseRelativeTinyCube();

		//this is the postprocessing buffer
		//postProcessBuffer = new ComputeBuffer(particleCount, 24);//16//28 // 24 = "stride" = size allocated for each particle, probably in bytes, no idea what happens with this
		//postProcessBuffer.SetData(postProcessArray);
		
		// Instantiate and initialise the GPU buffer.
		particleBuffer = new ComputeBuffer(particleCount, 36);//28 // 24 = "stride" = size allocated for each particle, probably in bytes, no idea what happens with this
		particleBuffer.SetData(particleArray);

		cubedVertBuffer = new ComputeBuffer(36, 12);//this is for the display shader; to display a cube instead of a point
		cubedVertBuffer.SetData(cubeVerts);



		// binding the buffer the compute shaders.
		for(int s=0; s< cubeStateObjects.Length; s++)
		{
			//Initializing all game states with the compute shader buffer and information 
			//and with other settings defined in this class
			cubeStateObjects[s].doStart(this);
		}


		//Also binding the buffer the display shader.
		//material.SetBuffer ("postProcessBuffer", postProcessBuffer);//this is for the shader shader
		materialBright.SetBuffer ("particleBuffer", particleBuffer);//this is for the shader shader
		
		materialBright.SetBuffer ("cubed_verts", cubedVertBuffer);
		materialBright.SetColor("_SpeedColor", Color.red);
		materialBright.SetFloat("_colorFade", 0.0f);


		materialDark.SetBuffer ("particleBuffer", particleBuffer);//this is for the shader shader
	
		materialDark.SetBuffer ("cubed_verts", cubedVertBuffer);
		materialDark.SetColor("_SpeedColor", Color.red);
		materialDark.SetFloat("_colorFade", 0.0f);


		currentState = ComputeStates.p0_DoNothing;


	}

	
	void Update () 
	{


		DoUpdate();



	}


	public void startComputeProcess(){
		#if UNITY_EDITOR
		Debug.Log("---> Tile based redering started. Tile count: "+TileFactory.tileList.Count);
		#endif
		_currentTile = 0;//0;
		computeNextTile();

	}

	private void computeNextTile(){

		if(currentTile < TileFactory.tileList.Count){
			//Debug.Log("Current tile: "+currentTile+"; < "+TileFactory.tileList.Count);
			lPts = TileFactory.tileList[currentTile].lPoints;

			inputManager.adjustVoxCamDist(TileFactory.tileList[currentTile].tilePos);

			currentState = ComputeStates.p1_MoveBrushToNextTile;
		}
		else{
			#if UNITY_EDITOR
			Debug.Log("Last tile: "+currentTile+"; out of "+TileFactory.tileList.Count);
			#endif
			onAllTilesProcessed();
		}
	}

	private void onAllTilesProcessed(){//COMPLETED
		//TODO: save all meshes in ExportedMeshContainer to disk.
		//save it as prefab or smth.
		currentState = ComputeStates.p0_DoNothing;
		inputManager.camera_.camera.backgroundColor = new Color32(172, 228, 255, 5);
		if(useFog)
			RenderSettings.fog =true;
		//LSysPrevContainer.SetActive(false);
		//auxCamera
		inputManager.endOfTileProcessing();
		//inputManager.enableInput();

		//crunchtime dirty hack; must do things properly.
		inputManager.auxVoxelCamera.enabled = false;
		inputManager.drawTextSprite1 = false;
		inputManager.generatingSprite.enabled = false;
	}

	private IEnumerator delayStartMarchingCubes(float time){
		//If you want to stop the shader after a certain number of frames, use stopComputeShadersAfterFrames instead
		yield return new WaitForSeconds(time);

		inputManager.disableInput();

		particleBuffer.GetData(particleArray);
		//postProcessBuffer.GetData(postProcessArray);
		//Debug.Log ("Marching cubes - GetData complete.");
		postProcessArray = new PPParticle[particleCount];
		Mesh mesh = cubeMarcher.CreateMesh(particleArray, postProcessArray);
		/*
		if(true){
			//once = false;
			for(int u=0; u< 32*32*10; u++){
				if(postProcessArray[u] != null){
					for(int e = 0; e< postProcessArray[u].verts.Length; e++){
						Debug.Log(u+"; 7777777777777 "+e+"; "+postProcessArray[u].verts[e]+"; p: "+postProcessArray[u].position);
						//Debug.Log(u+"; 7777777777777 "+postProcessArray[u].verts+"; p: "+postProcessArray[u].position);
					}
				}
			}
		}
		*/
		//FADE
		//material.SetFloat("_colorFade", 0.65f);
		//Debug.Log ("Marching cubes should be finished. [this is main]");



		//either postprocess the mesh and build it, or skip this whole step entirely and move to next tile
		if(mesh.vertices.Length >0){ 
			cueNextComputeStep();
			//note: this is "dangerous", as I am not checking if the current state is what I expect it to be.
			//postProcessBuffer.Release();
			//postProcessBuffer = new ComputeBuffer(particleCount, 36);//28 // 24 = "stride" = size allocated for each particle, probably in bytes, no idea what happens with this
			//postProcessBuffer.SetData(postProcessArray);
			//material.SetBuffer ("postProcessBuffer", postProcessBuffer);
			((S7_PP_Normals)cubeStateObjects[(int)currentState]).setMesh(mesh, postProcessArray);
		}
		else{
			#if UNITY_EDITOR
			Debug.Log("DISCARDED A MESH WITH 0 VERTICES_____________________!@#$#%#^$%&%^*&(*)) ");
			#endif
			forceResetToState0();
		}


	}

	//I left a copy above in the delayed version of the function, to keep/remember some notes/comments
	private void StartMarchingCubes(){
		inputManager.disableInput();
		
		particleBuffer.GetData(particleArray);

		postProcessArray = new PPParticle[particleCount];
		Mesh mesh = cubeMarcher.CreateMesh(particleArray, postProcessArray);

		//either postprocess the mesh and build it, or skip this whole step entirely and move to next tile
		if(mesh.vertices.Length >0){ 
			cueNextComputeStep();
			((S7_PP_Normals)cubeStateObjects[(int)currentState]).setMesh(mesh, postProcessArray);


		}
		else{
			#if UNITY_EDITOR
			Debug.Log("DISCARDED A MESH WITH 0 VERTICES_____________________!@#$#%#^$%&%^*&(*)) ");
			#endif
			forceResetToState0();
		}
	}

	/// <summary>
	/// This is triggered after the recalculation of normals for the already generated mesh for this current cube.
	/// And will instruct the cube to move to the next tile and start processing from state 0 again.
	/// </summary>
	public void OnPostProcessingEnded(Mesh mesh, Vector3[] outNorms){
	//public void OnPostProcessingEnded(Mesh mesh, Vector3[] outNorms, Vector3[] outVerts, int[] outTris){

		cubeMarcher.applyPostProcessedMesh(mesh, outNorms, TileFactory.tileList[currentTile].tileID, _currentTile);
		//cubeMarcher.applyPostProcessedMesh(mesh, outNorms, outVerts, outTris, TileFactory.tileList[currentTile].tileID);

		//restores the input
		StartCoroutine(inputManager.delaySetInput(0.1f, true));

		cueNextComputeStep();//this should be state 0

		_currentTile++;
		//if(!drawNormalLines || drawNormalLines && _currentTile<2)
		computeNextTile();
	}

	/// <summary>
	/// "Event" called by whatever the last compute shader state we run is.
	/// This function will begin running the Marching Cubes.
	/// </summary>
	public void OnComputingEnded(){
		/*
		if(delayBeforeMeshGeneration < 0.0f)
			delayBeforeMeshGeneration = 0.0f;
		Debug.Log ("Marching cubes will start in "+delayBeforeMeshGeneration+" seconds.");
		StartCoroutine(delayStartMarchingCubes(delayBeforeMeshGeneration));
		*/
		StartMarchingCubes();

		//TODO: 
		// - after this, we need to figure out:
		//			-how to apply material
		//		
		//	
	}


	/// <summary>
	/// Cues the next step.
	/// Assigns the next state ( the one at position of current enum + 1).
	/// NOTE: it wraps around (cue next step run on the last state, triggers state 0 (the "do nothing" state)).
	/// </summary>
	public void cueNextComputeStep(){
		
		if((int)currentState + 1 < Enum.GetNames(typeof(ComputeStates)).Length){
			//Debug.Log("moving from state "+currentState.ToString()+"; to state: "+((ComputeStates)(((int)currentState)+1)).ToString());
			#if UNITY_EDITOR
			Debug.Log("moving to state: "+((ComputeStates)(((int)currentState)+1)).ToString());
			#endif
			currentState = (ComputeStates)(((int)currentState)+1);
		}
		else{
			currentState = (ComputeStates)0;
			#if UNITY_EDITOR
			Debug.Log("moving to state 0");
			#endif
		}
	}

	/// <summary>
	/// Configures the state as soon as it is picked.
	/// Called by the Setter of currentState.
	/// </summary>
	void OnEnterState(){

		//If we have an exit state, then start it as a coroutine
		if(ExitState != null)
		{
			//StartCoroutine(ExitState());
			ExitState();
		}

		if(DoUpdate!=null)
			DoUpdate = cubeStateObjects[(int)currentState].doUpdate;
		if(ExitState!=null)
			ExitState = cubeStateObjects[(int)currentState].exitState;

		cubeStateObjects[(int)currentState].enterState();
	}
	

	/// <summary>
	/// Goes through all the children of the CubeStateContainer GameObject, 
	/// and assigns each child to a corresponding cubeStateObjects[enum CubeStates.*];
	/// </summary>
	public void Sync_CubeStateObjects_with_StateEnums(){//clever way to get the number of enums you have
		cubeStateObjects = new VoxelCubeStateBase[ Enum.GetNames(typeof(ComputeStates)).Length ];
		
		string s = "p0_DoNothing";
		
		
		for(int i=0; i< cubeStateObjects.Length; i++){
			int indexOfEnumWithSameNameAsChild = 0;
			s = cubeStateContainer.transform.GetChild(i).name;
			try
			{
				indexOfEnumWithSameNameAsChild = (int)Enum.Parse(typeof(ComputeStates), s);
			}
			catch(ArgumentException e)
			{
				Debug.LogError("ACHTUNG: The CubeState object of name \""+s+"\" does not match any of the CubeStates enum values. " + e.Message);
			}
			
			//Here I cleverly get a reference to each custom game state object, by looking for its generic base class.
			cubeStateObjects[indexOfEnumWithSameNameAsChild] = cubeStateContainer.transform.GetChild(i).GetComponent<VoxelCubeStateBase>();
			//Debug.Log("TESTING: added GO child of name: "+cubeStateContainer.transform.GetChild(i).name+" to the CubeStateObjects of index of the enum: "+s+" ("+indexOfEnumWithSameNameAsChild+");");
		}
		
		//cubeStateObjects[(int)CubeStates.p2_pointAndNoise].saySomething();
	}

	private void forceResetToState0(){
		StartCoroutine(inputManager.delaySetInput(0.01f, true));
		currentState = (ComputeStates)0;
		#if UNITY_EDITOR
		Debug.Log("moving to state 0");
		#endif
		_currentTile++;
		computeNextTile();
	}

	/// <summary>
	/// Does the nothing.
	/// This is a delegate I use in case we don't want to run any state's doUpdate() any more.
	/// </summary>
	public static void DoNothing(){
	}

	private void SetSettings(){
		if(whichSettingToUse == 1){
			settings = settings1;
		}
		else if(whichSettingToUse == 2){
			settings = settings2;
		}
		else{
			if(UnityEngine.Random.Range(-1.0f, 1.0f) < 0){
				settings = settings1;
			}
			else{
				settings = settings2;
			}
		}
	}

	private void ComputeRandomSeed(){


		float rangeMin = -8000.1f;//-8000.1f;
		float rangeMax = 8000.1f;//8000.1f;
		//rangeMin = 0;
		//rangeMax = 0;
		randomSeed = new float[3];
		//randomSeed[0] = 0;
		//randomSeed[1] = 0;
		//randomSeed[2] = 0;
		for(int i=0; i< 3; i++){
			randomSeed[i] = UnityEngine.Random.Range(rangeMin, rangeMax);
		}
		cubeMarcher.m_material.SetVector("_Seed", new Vector4(randomSeed[0], randomSeed[1], randomSeed[2], 0));
		//cubeMarcher.m_material.SetVector("_Seed", new Vector4(randomSeed[0], randomSeed[0], randomSeed[0], 0));
		Debug.Log("randomSeed: ["+randomSeed[0]+"]["+randomSeed[1]+"]["+randomSeed[2]+"]");
		//Debug.Log("randomSeed: ["+randomSeed[0]+"]["+randomSeed[0]+"]["+randomSeed[0]+"]");
	}

	private void SetSeed(){

		if(useManualSeed){
			randomSeed = new float[3];
			randomSeed[0] = yourManualSeed.x;
			randomSeed[1] = yourManualSeed.y;
			randomSeed[2] = yourManualSeed.z;
			cubeMarcher.m_material.SetVector("_Seed", new Vector4(randomSeed[0], randomSeed[1], randomSeed[2], 0));
			Debug.Log("randomSeed: ["+randomSeed[0]+"]["+randomSeed[1]+"]["+randomSeed[2]+"]");
		}
		else
			ComputeRandomSeed();
	}

	/// <summary>
	/// This defines vertices for a cube around point (0, 0, 0). 
	/// This will be used in addition to each point's position in the display shader, 
	/// in order to see tiny cubes on screen instead of a single point for each voxel.
	/// </summary>
	/// <returns>The base relative tiny cube.</returns>
	private Vector3[] createBaseRelativeTinyCube(){
		Vector3[] cubeShape = new Vector3[8];
		float div = cubeSpacing;// /2;
		// front bott left
		cubeShape[0] = Vector3.zero;
		cubeShape[0].x += particleCubeSize*div;
		cubeShape[0].y += particleCubeSize*div;
		cubeShape[0].z -= particleCubeSize*div;
		// back bott left
		cubeShape[1] = Vector3.zero;
		cubeShape[1].x += particleCubeSize*div;
		cubeShape[1].y -= particleCubeSize*div;
		cubeShape[1].z -= particleCubeSize*div;
		// back top left
		cubeShape[2] = Vector3.zero;
		cubeShape[2].x -= particleCubeSize*div;
		cubeShape[2].y -= particleCubeSize*div;
		cubeShape[2].z -= particleCubeSize*div;
		// front top left
		cubeShape[3] = Vector3.zero;
		cubeShape[3].x -= particleCubeSize*div;
		cubeShape[3].y += particleCubeSize*div;
		cubeShape[3].z -= particleCubeSize*div;
		
		// front bott right
		cubeShape[4] = Vector3.zero;
		cubeShape[4].x += particleCubeSize*div;
		cubeShape[4].y += particleCubeSize*div;
		cubeShape[4].z += particleCubeSize*div;
		// back bott right
		cubeShape[5] = Vector3.zero;
		cubeShape[5].x += particleCubeSize*div;
		cubeShape[5].y -= particleCubeSize*div;
		cubeShape[5].z += particleCubeSize*div;
		// back top right
		cubeShape[6] = Vector3.zero;
		cubeShape[6].x -= particleCubeSize*div;
		cubeShape[6].y -= particleCubeSize*div;
		cubeShape[6].z += particleCubeSize*div;
		// front top right
		cubeShape[7] = Vector3.zero;
		cubeShape[7].x -= particleCubeSize*div;
		cubeShape[7].y += particleCubeSize*div;
		cubeShape[7].z += particleCubeSize*div;
		
		return new Vector3[]//[24];
		{
			cubeShape[0], cubeShape[1], cubeShape[2],
			cubeShape[2], cubeShape[3], cubeShape[0],
			cubeShape[1], cubeShape[5], cubeShape[6],
			cubeShape[6], cubeShape[2], cubeShape[1],
			
			cubeShape[5], cubeShape[4], cubeShape[7],
			cubeShape[7], cubeShape[6], cubeShape[5],
			cubeShape[4], cubeShape[0], cubeShape[3],
			cubeShape[3], cubeShape[7], cubeShape[4],
			
			cubeShape[3], cubeShape[2], cubeShape[6],
			cubeShape[6], cubeShape[7], cubeShape[3],
			cubeShape[5], cubeShape[1], cubeShape[0],
			cubeShape[0], cubeShape[4], cubeShape[5]
		};
	}


	public void actualRender(){
		// Bind the pass to the pipeline then call a draw (this uses the buffer bound in Start() instead of a VBO).
		material.SetPass(0);//The Pass number is the Shader's pass in case it has multiple passes
		//Graphics.DrawProcedural(MeshTopology.Points, 12, particleCount);
		//Graphics.DrawProcedural(MeshTopology.Points, 8, 8);//particleCount);
		Graphics.DrawProcedural (MeshTopology.Triangles, 36, particleCount);
		//24 entries for cube vertices; you need 6 separate planes; unless you do custom per triangle stuff in shader: http://forum.unity3d.com/threads/142908-Procedural-Cube-Generation
		//Debug.Log("++++++++++++++++++++++++++++++++++++++++ACTUAL RENDER");
	}



	[HideInInspector]
	public bool renderLock = false;
	// Called by the camera in OnRender
	public void Render () 
	{

		//if(!renderLock)
			DoRender();
	}

	void OnDestroy()
	{
		list.Remove(this);
		
		// Unity cries if the GPU buffer isn't manually cleaned
		particleBuffer.Release();
		//postProcessBuffer.Release();
		cubedVertBuffer.Release();
	}
}
