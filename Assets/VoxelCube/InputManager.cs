using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

	public VoxelCubeManager voxelStateBase;
	public CubeMarcher cubeMarcher;
	public GameObject camera_;
	public Camera mainCam;
	public GameObject myCameraPivot;
	public Camera auxCamera;
	public Camera auxVoxelCamera;
	public GameObject pivotOpp;
	//public GameObject plane;
	public GameObject auxGui;
	public GameObject LSisPreviewCont;
	public GameObject largeCubeContainer;
	public GameObject infoGUI;
	public GameObject quadOverlay;
	public GameObject coordAxis;

	public GameObject playerCamPivot;
	public GameObject playerCamRig;
	public ProtectCameraFromWallClip wallClipScript;
	public FreeLookCam freeLookCam;
	public AutoCam jetPlaneCam;
	public GameObject thirdPersonChar;
	public GameObject thirdPersonJet;
	public SpriteRenderer generatingSprite;

	public Transform characterSpawnPoint;

	public float standardCharCamDist = -8;
	public float standardCharCamHeight = 4;

	public float standardJetCamDist = -13;
	public float standardJetCamHeight = 6;

	public bool iceMaterial = false;

	//TODO: Make state machine like in voxelstatebase instead of this "if" temporary bullshit
	private int playerState = 0;

	/*
	public bool inputEnabled{
		get { return m_input; }
		set { m_input = value; }
	}*/
	private bool inputEnabled = true;
	private Vector2 mousePosLastTime;
	private float lastUpdateTime;

	private Vector3 origPlayerCamPivot;
	private Quaternion origPlayerCamPivotRotation;
	private Vector3 origPlayerCamRig;

	private Vector3 origJetCamPivot;
	private Quaternion origJetCamPivotRotation;

	private GameObject currentPC;

	//private Vector3 prevCamAngle = Vector3.forward;
	//private Vector3 prevCamPos = Vector3.zero;

	//private float camAngleThresh = 0.5f;
	//private float camPosThresh = 0.5f;

	public Action HandleDynamicOcclusionCulling;// = DoNothing;

	private void DoNothing(){
	}
	
	//private Action DoOnGameWorld = VoxelCubeManager.DoNothing;

	// Use this for initialization
	void Start () {
		auxGui.SetActive(true);

		lastPos = mainCam.transform.position;
		initFreeCamView();
		enableWelcomeCam();
		if(!lsysMan.useManualEvolution){

			infoGUI.SetActive(true);
		}
		else{
			infoGUI.SetActive(false);
		}

		quadOverlay.SetActive(true);

		//DoOnGameWorld = firstRunOfGameWorld;

		//mainCam = pivot.GetComponent<Camera>();

		enableVoxelCubeVisualization();
		enableBrightVoxelCube();

		playerCamPivot.transform.localPosition = new Vector3(0, standardCharCamHeight, 0);


		origPlayerCamPivot = playerCamPivot.transform.localPosition;
		origPlayerCamPivotRotation = playerCamPivot.transform.localRotation;
		origPlayerCamRig = playerCamRig.transform.localPosition;

		origJetCamPivot = new Vector3(0, standardJetCamHeight, 0);
		origJetCamPivotRotation = playerCamPivot.transform.localRotation;

		playerCamPivot.transform.localPosition = Vector3.zero;

		currentPC = myCameraPivot;

		voxelStateBase.frostySeed = false;

		if(lsysMan.doDynamicOcclusionCulling){
		
			HandleDynamicOcclusionCulling = handleDynamicOcclusionCulling;
		}
		else{
			HandleDynamicOcclusionCulling = DoNothing;
		}

		GC.Collect();
		enabledCPointers = new List<TransRenderer>[sharing+1];
		for(int i=1; i<= sharing; i++){
			enabledCPointers[i] = new List<TransRenderer>();
			
		}
		transDict =	new Dictionary<int, TransRenderer> ();

		onceChar = true;
		drawTextSprite1 = true;

		initSpriteScale = generatingSprite.transform.localScale * 500;
	}

	private float sprAlpha1 = 0.83f;
	private float addSUb1 = 0.035f;//0.017f;//0.035f;
	private float addSubVal = 0.035f;//0.017f;
	private Vector3 initSpriteScale = Vector3.one;
	[HideInInspector]
	public bool drawTextSprite1 = true;
	void FixedUpdate(){
		if(drawTextSprite1){
			if(sprAlpha1>0.9f){
				addSUb1 = -addSubVal;
			}else
			if(sprAlpha1<0){
				addSUb1 = addSubVal;
			}
			sprAlpha1 += addSUb1;
			generatingSprite.color = new Color(1f,1f,1f,sprAlpha1);
			//Debug.Log(generatingSprite.color);
			//generatingSprite.transform.localPosition = Vector3.one;
			//generatingSprite.transform.position = mainCam.ScreenToWorldPoint(new Vector3(Screen.width - Screen.width*0.18f, 10, 1));
			generatingSprite.transform.position = mainCam.ScreenToWorldPoint(new Vector3(Screen.width - 90, Screen.height-10, 1));
			//generatingSprite.transform.localScale = initSpriteScale * 1/(mainCam.orthographicSize);
			generatingSprite.transform.localScale = initSpriteScale * 1/(Screen.height);
		}

	}

	int buttonWidth = 90;
	//int buttonHeigth = 35;
	int savedHeight = 25;
	//int textHeight = 25;
	void OnGUI(){

		if(lsysMan.useManualEvolution){

			if(GUI.Button(new Rect(Screen.width-10-buttonWidth*2, Screen.height-26-savedHeight, buttonWidth*2, savedHeight), "Toggle Canyon / Ice Textures")){
				iceMaterial = !iceMaterial;
				if(iceMaterial){
					switchToIce();
				}
				else{
					switchToCanyon();
				}
			}
		}


	}


	//clicking between 2 things distance too large if
	//do the -10 CA step
	//make build, possibly w sounds commented out


	public LSystemManager lsysMan;
	public GameObject staticLightCanyon;
	public GameObject characterLightIce;
	public GameObject characterLightCanyon;
	public GameObject camLightIce;
	//public GameObject camLightCanyon;
	private Vector3 iceAmbient = new Vector3(0, 0.4588235f, 0.5019607f);//new Vector3(0, 117, 128);
	private Vector3 canyonAmbient = new Vector3(0.231372549f, 0.29019607f, 0.431372549f);//new Vector3(59, 74, 110);
	//private Vector3 regularCamSkyColor = new Vector3(0.6745f, 0.89411f, 1f);//(172, 228, 255);
	//private Vector3 iceCamSkyColor = new Vector3(0.286274f, 0.286274f, 0.286274f);//(73, 73, 73);
	private void switchToIce(){
		voxelStateBase.frostySeed = true;
		//cubeMarcher.m_material = cubeMarcher.ice_material;
		cubeMarcher.m_material.shader = cubeMarcher.ice_material.shader;
		cubeMarcher.m_material.CopyPropertiesFromMaterial(cubeMarcher.ice_material);

		//mainCam.backgroundColor = new Color(iceCamSkyColor.x, iceCamSkyColor.y, iceCamSkyColor.z);

		RenderSettings.ambientLight = new Color(iceAmbient.x, iceAmbient.y, iceAmbient.z, 1);

		staticLightCanyon.SetActive(false);
		//camLightCanyon.SetActive(false);
		characterLightCanyon.SetActive(false);

		characterLightIce.SetActive(true);
		camLightIce.SetActive(true);

		thirdPersonChar.GetComponent<ThirdPersonCharacter>().advancedSettings.highFrictionMaterial = thirdPersonChar.GetComponent<ThirdPersonCharacter>().advancedSettings.bakZeroFrictionMaterial;
	}

	private void switchToCanyon(){
		voxelStateBase.frostySeed = false;
		//cubeMarcher.m_material = cubeMarcher.canyon_material;
		cubeMarcher.m_material.shader = cubeMarcher.canyon_material.shader;
		cubeMarcher.m_material.CopyPropertiesFromMaterial(cubeMarcher.canyon_material);

		//mainCam.backgroundColor = new Color(regularCamSkyColor.x, regularCamSkyColor.y, regularCamSkyColor.z);

		RenderSettings.ambientLight = new Color(canyonAmbient.x, canyonAmbient.y, canyonAmbient.z, 1);

		staticLightCanyon.SetActive(true);
		//camLightCanyon.SetActive(true);
		characterLightCanyon.SetActive(true); 
		
		characterLightIce.SetActive(false);
		camLightIce.SetActive(false);

		thirdPersonChar.GetComponent<ThirdPersonCharacter>().advancedSettings.highFrictionMaterial = thirdPersonChar.GetComponent<ThirdPersonCharacter>().advancedSettings.bakHighFrictionMaterial;
	}

	private bool onceChar = true;
	private Vector3 lastPos = Vector3.zero;
	// Update is called once per frame
	void Update () {
	

		if(Input.GetKeyDown(KeyCode.H)){
			//toggleAux();
			cycleGUI();
		}


		switch (playerState)
		{
		case 1://3rd person astrella
			lastPos = thirdPersonChar.transform.position;
			break;
		case 2://flight
			lastPos = thirdPersonJet.transform.position;
			break;
		default://state 0 is your own free view.
			lastPos = myCameraPivot.transform.position;
			break;
		}


		if(Input.GetKeyDown(KeyCode.Return)){
			playerState++;
			Vector3 p;

			switch (playerState)
			{
			case 1://3rd person astrella
				doDefaultGameCam();

				playerCamPivot.transform.localPosition = origPlayerCamPivot;
				playerCamPivot.transform.localRotation = origPlayerCamPivotRotation;
				playerCamRig.transform.localPosition = origPlayerCamRig;

				if(onceChar){
					thirdPersonChar.transform.position = characterSpawnPoint.position;
					onceChar = false;
				}
				else{
					thirdPersonChar.transform.position = lastPos;//new Vector3(camera_.transform.position.x, camera_.transform.position.y-1.2f, camera_.transform.position.z);
				}
				thirdPersonChar.SetActive(true);

				wallClipScript.enabled = true;
				freeLookCam.enabled = true;

				p = camera_.transform.localPosition;
				p.z += standardCharCamDist;
				camera_.transform.localPosition = p;
				wallClipScript.revertToOriginalDistance();

				currentPC = thirdPersonChar;

				//lastPos = thirdPersonChar.transform.position;
				
				break;
			case 2://flight
				doDefaultGameCam();

				playerCamPivot.transform.localPosition = origJetCamPivot;
				playerCamPivot.transform.localRotation = origJetCamPivotRotation;
				playerCamRig.transform.localPosition = origPlayerCamRig;

				thirdPersonJet.transform.position = lastPos;//new Vector3(camera_.transform.position.x, camera_.transform.position.y-1.2f, camera_.transform.position.z);;
				thirdPersonJet.transform.rotation = camera_.transform.rotation;
				wallClipScript.revertToOriginalDistance();

				//thirdPersonJet.transform.position = camera_.transform.position;
				thirdPersonChar.SetActive(false);
				thirdPersonJet.SetActive(true);
				thirdPersonJet.GetComponent<ObjectResetter>().ResetNowDamnit();


				freeLookCam.enabled = false;
				jetPlaneCam.enabled = true;

				p = camera_.transform.localPosition;
				p.z += standardJetCamDist;
				camera_.transform.localPosition = p;
				wallClipScript.revertToOriginalDistance();


				//				PLANE
				currentPC = thirdPersonJet;

				lastPos = thirdPersonJet.transform.position;
				
				break;
			default://state 0 is your own free view.

				initFreeCamView();

				break;
			}
		}


		//handles zoom, pan, rotate, etc.
		if(inputEnabled){
			handleInput();

			//FREEZE
			if(voxelStateBase.frostySeed){
				//int div = 2;//5000;//40 wo dot
				//int divRot = 2000;//5000;//40 wo dot
				//Vector3 pos = myCameraPivot.transform.position.normalized/div;// /10;
				//Vector3 rot = new Vector3(myCameraPivot.transform.rotation.x, myCameraPivot.transform.rotation.y, myCameraPivot.transform.rotation.z)/div;

				//Vector3 pos = camera_.transform.position.normalized/div;// /10;
				//Vector3 rot = new Vector3(camera_.transform.rotation.x, camera_.transform.rotation.y, camera_.transform.rotation.z)/divRot;
				Vector3 rot = new Vector3(camera_.transform.rotation.x, camera_.transform.rotation.y, camera_.transform.rotation.z);
				//float res = Vector3.Dot(pos, rot); 


				//float hardcodedClamp = 0.16f;//max was 0025, min ! 0005

				//pos.x += Math.Abs(rot.x);

				//pos.y += Math.Abs(rot.y);
				//Debug.Log(pos.y);

				//pos.z += Math.Abs(rot.z);

				
				//pos=pos.normalized * 6;
				//pos=pos.normalized * 6  /pos.magnitude;
				Vector3 pos = currentPC.transform.position.normalized * currentPC.transform.position.magnitude/8 + rot;
				//Debug.Log(pos);

				cubeMarcher.m_material.SetVector("_Seed",  /*
				new Vector4(voxelStateBase.randomSeed[0] + pos.x + pivot.transform.rotation.x, 
				            voxelStateBase.randomSeed[1] + pos.y + pivot.transform.rotation.y, 
				            voxelStateBase.randomSeed[2] + pos.z + pivot.transform.rotation.z, 0));*//*
				new Vector4(voxelStateBase.randomSeed[0] + pos.x * (pivot.transform.rotation.x)/100, 
				            voxelStateBase.randomSeed[1] + pos.y * (pivot.transform.rotation.y)/100, 
				            voxelStateBase.randomSeed[2] + pos.z * (pivot.transform.rotation.z)/100, 0));*/
					/*new Vector4(voxelStateBase.randomSeed[0] * (pos.x * rot.x), 
					            voxelStateBase.randomSeed[1] * pos.y * rot.y, 
					            voxelStateBase.randomSeed[2] * pos.z * rot.z, 0)); *//*
					new Vector4(voxelStateBase.randomSeed[0] * (pos.x +Math.Abs(rot.x)), 
					            voxelStateBase.randomSeed[1] * (pos.y +Math.Abs(rot.y)), 
					            voxelStateBase.randomSeed[2] * (pos.z +Math.Abs(rot.z)), 0)); */
				new Vector4(voxelStateBase.randomSeed[0] + (pos.x ), 
				            voxelStateBase.randomSeed[1] + (pos.y ), 
				            voxelStateBase.randomSeed[2] + (pos.z ), 0)); 
				
			}
		}

		if(Time.time - lastUpdateTime > 0.1f || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)){
			mousePosLastTime = Input.mousePosition;
			lastUpdateTime = Time.time;
		}

		HandleDynamicOcclusionCulling();
	}

	private void initFreeCamView(){
		playerState = 0;
		
		myCameraPivot.transform.position = lastPos;//camera_.transform.position;
		myCameraPivot.transform.rotation = Quaternion.identity; 
		playerCamPivot.transform.localPosition = playerCamRig.transform.localPosition = Vector3.zero;
		playerCamPivot.transform.rotation = Quaternion.identity;
		
		
		
		wallClipScript.enabled = false;
		freeLookCam.enabled = false;
		jetPlaneCam.enabled = false;
		
		thirdPersonChar.SetActive(false);
		thirdPersonJet.SetActive(false);
		
		
		camera_.transform.localPosition = Vector3.zero;
		
		lastPos = myCameraPivot.transform.position;
	}
	 
	public void OnDrawGizmos(){
		/*
		float scrCentreX = mainCam.pixelWidth/2;
		float scrCentreY = mainCam.pixelHeight/2;
		float fovAngle = mainCam.fieldOfView;

		for(int i = 0; i < mainCam.pixelWidth; i+=resolution){
			for(int j=0; j < mainCam.pixelHeight; j+=resolution){
				Ray ray = mainCam.ScreenPointToRay(new Vector3(i, j, 0));
				//Gizmos.DrawCube(ray.GetPoint(1), new Vector3(0.002f, 0.002f, 100.02f)); 
				Vector3 dir = mainCam.transform.TransformDirection(Vector3.forward);
				//dir.z = mainCam.farClipPlane*3.5f;
				dir.z = mainCam.farClipPlane*4;

				dir.x = (i-scrCentreX+resolution/2)*fovAngle/60; 
				dir.y = (j-scrCentreY+resolution/2)*fovAngle/60;


				//Gizmos.DrawLine(ray.GetPoint(0), dir);
			}
		} 
		*/
	}

	public void safeDestroy(){
		HandleDynamicOcclusionCulling = DoNothing;

		if(enabledCPointers != null){
			for(int i=1; i<= sharing; i++){
				if(enabledCPointers[i] != null){
					for(int k=0; k< enabledCPointers[i].Count; k++){
						enabledCPointers[i][k].ccount = 0;
						enabledCPointers[i][k].trans = null;
						enabledCPointers[i][k].uid = 0;
						enabledCPointers[i][k].usedBy = null;
						
						enabledCPointers[i][k] = null;
					}
					enabledCPointers[i].Clear();
					enabledCPointers[i] = null;
				}
			}
			enabledCPointers = null;
			transDict.Clear();
			transDict = null;
		}
		GC.Collect();
	}


	private int c=1;
	private static int sharing = 24;//17;//50;//17;//13;//17;//20;//6;	//spread operation over this many frames
	private int step = 48;//51;//100;//51;//52;//100;//90; 			//cast a ray, once every 'step' pixels in screen space, per frame.
	private int resolution = 2;//3;//3;//4;//3;//15;//30;//50	//offset the rays by 'resolution', every frame, 
												//	so that you eventually cover the whole screen in rays.
												//	(every 'resolution' pixels there has been a ray, in the past 'sharing' number of frames).
												//You get (1920 / 'step') * (1080 / 'step') raycasts per frame.
												// 		That's 830 raycasts for step = 50; Unity limit is 4096 for highest quality setting.
	private class TransRenderer{
		public Renderer trans;
		public int ccount;
		public int uid;
		public bool[] usedBy;// = new bool[7];
		public TransRenderer(Renderer _trans, int _c, int _uid){
			trans = _trans;
			ccount = _c;
			uid = _uid;
			usedBy = new bool[sharing+1];
		}

	}
	//private List<TransRenderer> enabledRenderers = new List<TransRenderer>();
	private List<TransRenderer>[] enabledCPointers;// = new List<TransRenderer>[sharing+1];
	private int terrainLayer = 1<<30 | 1<<31;
	private void handleDynamicOcclusionCulling(){
		//Debug.Log("Vector3.Angle(camera_.transform.forward, prevCamAngle: "+Vector3.Angle(camera_.transform.forward, prevCamAngle));
		//Debug.Log(" Vector3.Distance(camera_.transform.position, prevCamPos): "+ Vector3.Distance(camera_.transform.position, prevCamPos));


		if(c>sharing){
			c=1;
		}

		float scrCentreX = mainCam.pixelWidth/2;
		float scrCentreY = mainCam.pixelHeight/2;
		float fovAngle = mainCam.fieldOfView / 24;

		Vector3 dir = Vector3.zero;
		float dirz = mainCam.farClipPlane*4;
		int stepHalf = step/2;

		for(int k=0; k < enabledCPointers[c].Count; k++){
			if(enabledCPointers[c][k].trans != null){
			
				enabledCPointers[c][k].ccount--;
				enabledCPointers[c][k].usedBy[c]=false;

				//Debug.Log("PRE_______________________________________----___"+enabledCPointers[c][k].ccount+"______---____________");

				if(enabledCPointers[c][k].ccount <1){
					enabledCPointers[c][k].trans.enabled = false;

					transDict.Remove(enabledCPointers[c][k].uid);

					enabledCPointers[c].Remove(enabledCPointers[c][k]);
				}
			}

		}
		//enabledCPointers[c].Clear();

		for(int i = c*resolution; i < mainCam.pixelWidth; i+=step){
			for(int j= c*resolution; j < mainCam.pixelHeight; j+=step){

				//Ray ray = mainCam.ScreenPointToRay(new Vector3(i, j, 0));
				Vector3 ray = mainCam.ScreenToWorldPoint(new Vector3(i, j, -1));
				RaycastHit hit;

				dir.x = (i-scrCentreX+stepHalf)*fovAngle;///24;//24; 
				dir.y = (j-scrCentreY+stepHalf)*fovAngle;///24;//24;
				dir.z = dirz;//mainCam.farClipPlane*4;
				dir = mainCam.transform.TransformDirection(dir);
					
				if (Physics.Raycast(ray, dir, out hit, mainCam.farClipPlane, terrainLayer)){

					#if UNITY_EDITOR
					Debug.DrawRay(ray, dir, Color.gray);
					//Debug.DrawLine(ray, dir, Color.yellow);
					#endif

					//curse you, one sided mesh collision detection system!
					RaycastHit hit2;
					if (Physics.Linecast(hit.point, mainCam.transform.position, out hit2, terrainLayer)){
					//if (Physics.Raycast(hit.point, -mainCam.transform.position, out hit2, mainCam.farClipPlane, 1<<30)){

						//if(!hit2.collider.transform.renderer.enabled){
						if(hit2.collider.gameObject.layer == 30)
							occDBCheckIn(c, hit2.collider.transform.renderer);
						//}
						#if UNITY_EDITOR
						Debug.DrawLine(hit.point, hit2.point, Color.green);
						#endif
					}
					//else{
					//if(!hit.collider.transform.renderer.enabled){
					//hit.collider.transform.renderer.enabled = true;
					//enabledRenderers.Add(hit.collider.transform);
					if(hit.collider.gameObject.layer == 30)
						occDBCheckIn(c, hit.collider.transform.renderer);
					//}
					//}
				}
			}
		}
	

		c++;
	}
	
	private Dictionary<int, TransRenderer> transDict;// =	new Dictionary<int, TransRenderer> ();

	public void occDBCheckIn(int c, Renderer obtransform){
		TransRenderer t = new TransRenderer(obtransform, 1, obtransform.GetHashCode());
		t.usedBy[c] = true;
		bool found = false;

		TransRenderer tt;// = t;
		if(transDict.TryGetValue(t.uid, out tt)){
			if(tt.usedBy[c] != true){
				tt.usedBy[c] = true;
				tt.ccount++;

			}
			found = true;

		}
		
		if(!found){
			t.trans.enabled = true;
			//enabledRenderers.Add(t);
			enabledCPointers[c].Add(t);
			transDict.Add(t.uid, t);

		}
	}

/*
	private void firstRunOfGameWorld(){
		StartCoroutine(delaySetClearFlags(2f));

		DoOnGameWorld = VoxelCubeManager.DoNothing;
	}
	*/

	private IEnumerator delaySetClearFlags(float time){
		yield return new WaitForSeconds(time);
		
		auxVoxelCamera.clearFlags = CameraClearFlags.Nothing;
	}

	public IEnumerator delaySetInput(float time, bool val){
		yield return new WaitForSeconds(time);
		
		inputEnabled = val;//!inputEnabled;
		//Debug.Log("Mouse&Keyboard Input enabled: "+inputEnabled);
	}

	public void disableInput(){
		inputEnabled = false;
		//Debug.Log("Mouse&Keyboard Input enabled: "+inputEnabled);
	}

	public void enableInput(){
		inputEnabled = true;
		//Debug.Log("Mouse&Keyboard Input enabled: "+inputEnabled);
	}

	public void endOfTileProcessing(){
		disableAux();
	}

	private void disableAux(){
		auxCamera.enabled = false;
		//auxVoxelCamera.enabled = true;
		auxGui.SetActive(false);
		LSisPreviewCont.SetActive(false);
	}


	private void enableAux(){
		auxCamera.enabled = true;
		//auxVoxelCamera.enabled = false;
		auxGui.SetActive(true);
		LSisPreviewCont.SetActive(true);
	}

	private void disableInfo(){
		infoGUI.SetActive(false);
	}

	private void enableInfo(){
		infoGUI.SetActive(true);
	}

	private void toggleAux(){
		auxCamera.enabled = !auxCamera.enabled;
		//auxVoxelCamera.enabled = !auxVoxelCamera.enabled;
		auxGui.SetActive(!auxGui.activeInHierarchy);
		LSisPreviewCont.SetActive(!LSisPreviewCont.activeInHierarchy);

		handleVoxelCubeVisualization();

	}

	private void handleVoxelCubeVisualization(){
		if(auxCamera.enabled){
			voxelStateBase.renderLock = true;
			voxelStateBase.DoRender = VoxelCubeManager.DoNothing;
			//Debug.Log ("%%%%%%%%%%%%%%%% voxelStateBase.DoRender = VoxelCubeManager.RenderNothing;");
		}
		else{
			voxelStateBase.renderLock = false;
			voxelStateBase.DoRender = voxelStateBase.actualRender;
			//Debug.Log ("%%%%%%%%%%%%%%%% voxelStateBase.DoRender = voxelStateBase.actualRender;");
		}
	}

	private void enableVoxelCubeVisualization(){
		voxelStateBase.renderLock = false;
		voxelStateBase.DoRender = VoxelCubeManager.DoNothing;
	}

	private void disableVoxelCubeVisualization(){
		voxelStateBase.renderLock = true;
		voxelStateBase.DoRender = voxelStateBase.actualRender;
	}


	private void enableBrightVoxelCube(){
		voxelStateBase.material = voxelStateBase.materialBright;
		auxVoxelCamera.enabled = true;
	}

	private void disableBrightVoxelCube(){
		voxelStateBase.material = voxelStateBase.materialDark;
		auxVoxelCamera.enabled = false;
	}

	private int cycle = 1;
	private void cycleGUI(){
		cycle++;

		switch (cycle)
		{
		case 1://show lsystem + information & controls "GUI"

			enableWelcomeCam();


			break;
		case 2://show just lsystem preview
			disableInfo();
			largeCubeContainer.SetActive(true);

			disableBrightVoxelCube();
			disableVoxelCubeVisualization();

			break;
		default://show game with no overlays
			doDefaultGameCam();

			break;
		}

		//handleVoxelCubeVisualization();

	}

	private void enableWelcomeCam(){
		enableAux();
		enableInfo();
		RenderSettings.fog =false;
		largeCubeContainer.SetActive(false);
		
		enableVoxelCubeVisualization();
		enableBrightVoxelCube();
	}
	
	private void doDefaultGameCam(){
		cycle = 0;
		
		disableAux();
		//disableInfo();
		RenderSettings.fog =true;
		
		auxVoxelCamera.enabled = true;
		enableVoxelCubeVisualization();
		
		//DoOnGameWorld();
	}

	//float minimumX = -360F;
	//float maximumX = 360F;
	float minimumY = -80F;
	float maximumY = 80F;
	float rotationY = 0F;//-44.1F;//0F;
	//int firstClick = 0;
	//int firstRot= 0;
	//Vector2 sensitivity = new Vector2(3, 3);
	Vector2 sensitivity = new Vector2(2.7f, 2.7f);
	void fpsLook(){//https://stackoverflow.com/questions/8465323/unity-fps-rotation-camera
		
		float rotationX = myCameraPivot.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity.x;
		
		rotationY += Input.GetAxis("Mouse Y") * sensitivity.y;
		rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
		
		myCameraPivot.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		
		//plane.transform.rotation = pivot.transform.rotation;
		//plane.SetActive(false);
		//Debug.Log(rotationY);
	}

	/// <summary>
	/// Changes the camera which renders the voxel cube visualization, to depth only, or nothing, based onn how far the camera is from the current render position.
	/// </summary>
	public void adjustVoxCamDist(Vector3 tilePos){
		//Debug.Log("TilePos: "+tilePos);
		if(Vector3.Distance(tilePos, auxVoxelCamera.transform.position) < mainCam.farClipPlane){

			auxVoxelCamera.clearFlags = CameraClearFlags.Depth;
			auxVoxelCamera.depth = mainCam.depth-0.5f;
		}else{
			auxVoxelCamera.depth = mainCam.depth;
			auxVoxelCamera.clearFlags = CameraClearFlags.Nothing;
		}
	}

	private float scrollSpeedMod = 1.25f;//5
	private float scrolAdd = 0;
	private float lastZoomTime = 0;
	private float zoomTimeLimit = 0.8f;
	private float origZoomTimeLimit = 0.8f;
	private float origScrollSpeedMod = 1.25f;
	//float timesfiveRestore = 0;
	void handleInput(){

		if(Input.GetKeyDown(KeyCode.J)){
			iceMaterial = !iceMaterial;
			if(iceMaterial){
				switchToIce();
			}
			else{
				switchToCanyon();
			}
		}
		
		if(playerState == 0){

			if(Input.GetMouseButton(1)){

				fpsLook();
				/*
				if(Input.GetKey(KeyCode.C)){
					pivot.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * 100);
				}
				else {
					fpsLook();
				}
				*/
			}

			if(Input.GetMouseButton(0) || Input.GetMouseButton(2)){
				Vector3 diff = Input.mousePosition - new Vector3(mousePosLastTime.x, mousePosLastTime.y, 0);
				diff.z = 0;
														//was4
				myCameraPivot.transform.Translate(-diff * 2.7f * Time.deltaTime, myCameraPivot.transform);
			}
		}


		if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0){

			if(voxelStateBase && voxelStateBase.currentTile < TileFactory.tileList.Count)
				adjustVoxCamDist(TileFactory.tileList[voxelStateBase.currentTile].tilePos);
			else
				adjustVoxCamDist(coordAxis.transform.position);
				


			if(playerState==0){
				handleZoomSpeedAcceleration();
				lastZoomTime = Time.time;
				myCameraPivot.transform.position -= myCameraPivot.transform.TransformDirection(Vector3.forward)*scrollSpeedMod;
				pivotOpp.transform.position += myCameraPivot.transform.TransformDirection(Vector3.forward)*scrollSpeedMod;
			}
			else if (playerState ==1){
				//camera_.transform.position -= pivot.transform.TransformDirection(Vector3.forward);
				wallClipScript.tweakDefaultDistance(myCameraPivot.transform.TransformDirection(Vector3.forward),1);
				//Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~>> ScrollDown: camera_.transform.position: "+camera_.transform.position);
			}


		} else 
		if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0){

			if(voxelStateBase && voxelStateBase.currentTile < TileFactory.tileList.Count)
				adjustVoxCamDist(TileFactory.tileList[voxelStateBase.currentTile].tilePos);
			else
				adjustVoxCamDist(coordAxis.transform.position);



			if(playerState==0){
				myCameraPivot.transform.position += myCameraPivot.transform.TransformDirection(Vector3.forward)*scrollSpeedMod;
				pivotOpp.transform.position -= myCameraPivot.transform.TransformDirection(Vector3.forward)*scrollSpeedMod;
				handleZoomSpeedAcceleration();
				lastZoomTime = Time.time;
			}
			else if (playerState ==1){
				//camera_.transform.position += pivot.transform.TransformDirection(Vector3.forward);
				wallClipScript.tweakDefaultDistance(myCameraPivot.transform.TransformDirection(Vector3.forward), -1);
				//Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~>> ScrollUP: camera_.transform.position: "+camera_.transform.position);
			}

		}

		/*
		if(Time.time - lastUpdateTime > 0.1f || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)){
			mousePosLastTime = Input.mousePosition;
			lastUpdateTime = Time.time;
		}
		*/
	
	}

	private void handleZoomSpeedAcceleration(){


		if(Time.time - lastZoomTime < zoomTimeLimit){
			scrolAdd += scrollSpeedMod/40;
			scrollSpeedMod += scrolAdd;
			zoomTimeLimit += zoomTimeLimit/100;
			if(zoomTimeLimit>1.8f)
				zoomTimeLimit = 1.8f;
			if(scrollSpeedMod>20)
				scrollSpeedMod = 20;
		}
		else{
			scrolAdd = 0;
			scrollSpeedMod = origScrollSpeedMod;
			zoomTimeLimit = origZoomTimeLimit;
		}

		//Debug.Log("scrollSpeedMod= "+scrollSpeedMod+"; scrolAdd= "+scrolAdd+"; zoomTimeLimit= "+zoomTimeLimit);
	}
}
