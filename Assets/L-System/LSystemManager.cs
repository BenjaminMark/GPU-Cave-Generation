using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;


public class LSystemManager : MonoBehaviour
{
	public GameObject voxelCube;	
	public InputManager inputMan;
	[HideInInspector]
	public bool useManualEvolution = false; //start the game with the gui off so it's less confusing
	public float timeToFade = 3.0f;

	public bool showMeshBoundingBoxes = true;
	public bool doDynamicOcclusionCulling = true;

	private EvolutionCommand nextCommand = new EvolutionCommand();


	private float timer;
	private bool premature = false;
	private bool loadError = false;
	private bool mating = false;
	private List<LRule> currentGenPicks = new List<LRule>();
	private List<LRule> lastGenPicks = new List<LRule>();
	private List<LSysContainer> savedSystems = new List<LSysContainer>();
	private string baseSaveString = "LSystem";
	private string baseLoadString = "File to Load";
	private string saveString;
	private string loadString;
	private int generation = 0;
	private bool picked = false;

	// Use this for initialization
	void Start ()
	{
		timer = timeToFade;
		saveString = baseSaveString;
		loadString = baseLoadString;
	}

	// Update is called once per frame
	void Update ()
	{
		if(timer < 0){
			timer = timeToFade;
			premature = false;
			loadError = false;
		}

		if(premature || loadError){
			timer -= Time.deltaTime;
		}

		if(Input.GetKeyUp(KeyCode.Escape)){
			useManualEvolution = !useManualEvolution;
			//Debug.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"+useManualEvolution);
			Camera.main.GetComponent<GUILayer>().enabled = useManualEvolution;
		}
	}

	int buttonWidth = 90;
	int buttonHeigth = 35;
	int savedHeight = 25;
	int textHeight = 25;
	void OnGUI(){


//#if UNITY_EDITOR || UNITY_STANDALONE 
//would be nice to have these in the web build as well. I just disabled the load and save

		if(useManualEvolution){
			GUI.TextArea(new Rect(10, 5, buttonWidth*2+60, textHeight-4), "Custom pick and combine cave trees:");

			if(GUI.Button(new Rect(10, 30, buttonWidth, buttonHeigth), "Re-Generate")){
				nextCommand = new EvolutionCommand(EvolutionState.MATING, voxelCube.GetComponent<LSystem>().getCurrentMainRule(), null);
				resetLSystem();
			}

			if(GUI.Button(new Rect(10, 40+buttonHeigth, buttonWidth, buttonHeigth), "Next Style")){
				nextSystem(); 
			}

			if(GUI.Button(new Rect(10, 50+buttonHeigth*2, buttonWidth, buttonHeigth), "Add to List")){
				if(!picked){
					currentGenPicks.Add(voxelCube.GetComponent<LSystem>().getCurrentMainRule());
					savedSystems.Add(voxelCube.GetComponent<LSystem>().getCurrentLsystem());
					picked = true;
				}
			}

			if(GUI.Button(new Rect(10, 60+buttonHeigth*3, buttonWidth*2-40, buttonHeigth), "Mate to Generation: " + (generation+2))){
				if(currentGenPicks.Count < 2){
					premature = true;

				} else {
					lastGenPicks = currentGenPicks;
					currentGenPicks = new List<LRule>();
					mating = true;
					generation++;
					nextSystem();
				}
			}

			if(GUI.Button(new Rect(10, 70+buttonHeigth*4, buttonWidth, buttonHeigth), "Pick Final")){
				voxelCube.GetComponent<LSystem>().getCurrentLsystem().save("LastPick.xml");
				useManualEvolution = false;
			}

			if(GUI.Button(new Rect(10, 80+buttonHeigth*5, buttonWidth+24, buttonHeigth-10), "Back To Previous")){
				try{
					nextCommand = new EvolutionCommand(EvolutionState.LOADING, null, LSysContainer.load("LastPick.xml"));
					useManualEvolution = false;
					resetLSystem();
				} catch (System.IO.FileNotFoundException e){
					Debug.LogError(e);
					loadError = true;
				}
			}

			if(GUI.Button(new Rect(10, 80+buttonHeigth*6, buttonWidth, buttonHeigth-10), "Reset")){
				nextCommand = new EvolutionCommand();
				resetLSystem();
            }

			for(int i = 0; i < savedSystems.Count; i++){
				if(GUI.Button(new Rect(Screen.width-10-buttonWidth, 10+i*10+i*savedHeight, buttonWidth, savedHeight), savedSystems[i].initialRule)){
						nextCommand = new EvolutionCommand(EvolutionState.LOADING, null, savedSystems[i]);
					resetLSystem();
				}
			}

			if(premature){
				GUIStyle style = new GUIStyle();
				style.fontSize = 18;
				GUI.Label(new Rect(Screen.width/2 - 150, Screen.height/2 - 150, 300, 300), "You must pick at least 2 L-Systems to proceed", style);
			}

			if(loadError){
				GUIStyle style = new GUIStyle();
				style.fontSize = 18;
				GUI.Label(new Rect(Screen.width/2 - 150, Screen.height/2 - 150, 300, 300), "Invalid Filename", style);
			}

			#if UNITY_EDITOR || UNITY_STANDALONE
			loadString = GUI.TextField(new Rect(10, Screen.height - 50 - textHeight*2 - buttonHeigth*2, buttonWidth, textHeight), loadString);

			if(GUI.Button(new Rect(10, Screen.height- 40 - textHeight - buttonHeigth*2, buttonWidth, buttonHeigth), "Load System")){
				try{
					nextCommand = new EvolutionCommand(EvolutionState.LOADING,null, LSysContainer.load(Path.Combine(Application.dataPath, "systems/" + loadString + ".xml")));
					loadString = baseLoadString;
					resetLSystem();
				} catch (System.IO.FileNotFoundException e){
					loadError = true;
					Debug.Log(e);
				}
			}

			saveString = GUI.TextField(new Rect(10, Screen.height - 20 - textHeight - buttonHeigth, buttonWidth, textHeight), saveString);

			if(GUI.Button(new Rect(10, Screen.height - 10 -  buttonHeigth, buttonWidth, buttonHeigth), "Save System")){
				voxelCube.GetComponent<LSystem>().getCurrentLsystem().save(Path.Combine(Application.dataPath, "systems/" + saveString + ".xml"));
				saveString = baseSaveString;
			}
			#endif
			/*
			if(GUI.Button(new Rect(Screen.width-10-buttonWidth, Screen.height-30-savedHeight*2, buttonWidth, savedHeight), "Regenerate")){
				nextCommand = new EvolutionCommand(EvolutionState.MATING, voxelCube.GetComponent<LSystem>().getCurrentMainRule(), null);
				resetLSystem();
			}
			*/

		}
		/*
		//GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = fontSize;
		GUI.skin.button.fontSize = 10;
		if(GUI.Button(new Rect(Screen.width+20-buttonWidth, Screen.height+2-savedHeight, buttonWidth*0.7f, savedHeight*0.65f), "Toggle GUI")){
			useManualEvolution = !useManualEvolution;
		} 
		GUI.skin.button.fontSize = 12;
		*/

		//#endif
	}

	public EvolutionCommand getCurrentCommand(){
		return nextCommand;
	}

	private void nextSystem(){
		if(!mating){
			nextCommand = new EvolutionCommand();
			resetLSystem();
		} else {
			LRule child = getMatedRule();
			nextCommand = new EvolutionCommand(EvolutionState.MATING, child, null);
			resetLSystem();
		}
	}

	public void resetLSystem(){
		inputMan.safeDestroy();

		foreach(GameObject obj in voxelCube.GetComponent<CleanUpList>().objects)
			Destroy(obj);
		GameObject tmp = Instantiate(voxelCube) as GameObject;
		Destroy(voxelCube.GetComponent<LSystem>().bigParent);
		Destroy (voxelCube.GetComponent<LSystem>().smallParent);
		Destroy(voxelCube);
		voxelCube = tmp;
		picked = false;
	}

	private LRule getMatedRule(){
		List<LRule> matingPair = lastGenPicks.OrderBy(x => UnityEngine.Random.value).Take(2).ToList();
		
		Debug.Log(matingPair[0].rule + "   " + matingPair[1].rule);
		
		LRule child = MateRules(matingPair[0], matingPair[1]);

		Debug.Log(child.rule);

		return child;
	}

	private LRule MateRules(LRule lhs, LRule rhs){

		StringBuilder dest = new StringBuilder(lhs.rule);
		StringBuilder source = new StringBuilder(rhs.rule);
		IntVector2 sperm = rhs.bracketList[UnityEngine.Random.Range(0, rhs.bracketList.Count-1)];
		IntVector2 egg = lhs.bracketList[UnityEngine.Random.Range(0, lhs.bracketList.Count-1)];

		dest.Remove(egg.x, egg.y-egg.x+1);
		dest.Insert(egg.x, source.ToString(sperm.x, sperm.y-sperm.x+1));

		Stack<IntVector2> bracketStack = new Stack<IntVector2>();
		LRule retVal = new LRule();

		for(int i = 0; i < dest.Length; i++){
			if(dest[i] == '['){
				bracketStack.Push(new IntVector2(i, 0));
			}
			if(dest[i] == ']'){
				IntVector2 tmp = bracketStack.Pop();
				tmp.y = i;
				retVal.bracketList.Add(tmp);
			}
		}

		retVal.symbol = voxelCube.GetComponent<LSystem>().mainSymbol;
		retVal.rule = dest.ToString();

		return retVal;
	}	
}

