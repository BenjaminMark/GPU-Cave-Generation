
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;

using deb = UnityEngine.Debug;


public class LSystem : MonoBehaviour {

	//public GameObject settings;

	public GameObject manager;
	public Transform blockContainer;
	public GameObject block;
	public Transform largeBlockContainer;
	public GameObject largeBlock;
	public GameObject debugSphere;
	public string initialString = "F";
	public float expandChance = 1.0f;
	public int masterSpread = 1;
    public int masterTurnAngle = 30;
	public int angledeviation = 10;
	public int spreadStep = 1;
	public int angleStep = 5;
	public int nrOfRewrites = 2;
	public bool forceDrawLsystemTree = false;
	public float slopeLimit = 1.0f;
	public bool balancedMode = false;
	public float balanceStep = 0.05f;
	public bool targetMode = false;
	public Transform target;
	public float distanceWeight = 1;
	public bool constraintBoxMode = false;
	public Bounds constraintBoxSize;
	public bool constraintSphereMode = false;
	public int constraintSphereRadius = 0;
	public TextAsset rawRules;
	public bool connectEndPoints = true;
	public float endPointConnectChance = 0.3f;
	public int endPointMaxDist = 200;
	//The stuff you divide the point with
	public float endPointFrequency = 200;
	//The stuff you divide the noise with
	public float endPointAmplitude = 9;
	public bool loadSystem = false;
	public int displayOneEvery = 7;
	private int lPointSampleFilter = 2;
	public bool useRandomRule = true;
	public int numMainRules = 1;
	public int minInitialLength = 5;
	public int maxInitialLength = 10;
	public float bracketChance = 0.4f;
	public SymbolWeights initialWeights = new SymbolWeights();
	public Action<Vector3> PipeOutThisPoint;// = DoNothing;
	public Vector3 lPreviewOffset = new Vector3(-60, 15, 0);

	[HideInInspector]

	private int gCount = 0;
	private VoxelCubeManager particleComputer;
	public char mainSymbol = 'Z';
	private List<LRule> mainRules = new List<LRule>();
	private Dictionary<char, List<LRule>> subRules = new Dictionary<char, List<LRule>>();
	private float balanceWeight = 0.0f;
	List<LPosition> endPointList = new List<LPosition>();
	HashSet<LSysLine> lineSet = new HashSet<LSysLine>( new LSysLineEquator());

	private Dictionary<SubRuleType, float> sWeights = new Dictionary<SubRuleType, float>();
	private LSysContainer container = new LSysContainer();

	private EvolutionCommand currentCommand;
	private int currentGen = 0;
	public GameObject smallParent;
	public GameObject bigParent;
	private int numPoints = 0;
	private Vector3 minPoint = Vector3.zero;
	private Vector3 maxPoint = Vector3.zero;



	/*
	 * L-System Symbol Legend:
	 * C: Curve
	 * F: Forward
	 * L: Yaw CClockwise
	 * R: Yaw Clockwise
	 * +: Roll Clockwise
	 * -: roll CClockwise
	 * U: Pitch up
	 * D: pitch down
	 * O: Increase the angle (obtuse)
	 * A: Decrease the angle (acute)
	 * B: Increase the spread
	 * S: Decrease the spread
	 * [ and ]: Begin and end branch
	 */

	
	void Start () {
		//Controls where the points are sent to when drawing.
		particleComputer = gameObject.GetComponent<VoxelCubeManager>();
		if(particleComputer){
			TileFactory.Start(particleComputer);
			if(forceDrawLsystemTree)
				PipeOutThisPoint = pipePtToBoth;
			else
				PipeOutThisPoint = pipePtToVoxelManager;
		}
		else{
			PipeOutThisPoint = pipePtToWorld;
		}
		
		smallParent = new GameObject();
		bigParent = new GameObject();
		bigParent.transform.parent = largeBlockContainer;
		smallParent.transform.parent = blockContainer;

		StringBuilder axiom = new StringBuilder();
		currentCommand = manager.GetComponent<LSystemManager>().getCurrentCommand();

		
		//Loads either the last saved system or a specified system
		if(loadSystem || currentCommand.state == EvolutionState.LOADING){
			if(loadSystem)
				container = LSysContainer.load(Path.Combine(Application.dataPath, "TestFile.xml"));
			if(currentCommand.state == EvolutionState.LOADING)
				container = currentCommand.prevToLoad;

			axiom = new StringBuilder(container.axiom);
			GetComponent<VoxelCubeManager>().yourManualSeed = new Vector3(container.randomSeed[0], container.randomSeed[1], container.randomSeed[2]);
			GetComponent<VoxelCubeManager>().useManualSeed = true;
			masterSpread = container.spread;
			masterTurnAngle = container.angle;
			mainRules = new List<LRule>();
			LRule rule = new LRule(masterSpread, masterTurnAngle);
			rule.symbol = mainSymbol;
			rule.rule = container.initialRule;
			mainRules.Add(rule);
		}else {
			//Load the rules from the text asset
			loadRules();
			fillSWeights();

			int count = 0;

			do{
				if(count > 50)
					break;

				axiom = new StringBuilder(initialString);
                
                if(useRandomRule && currentCommand.state != EvolutionState.MATING){
					mainRules = new List<LRule>();
					for(int i = 0; i < numMainRules; i++){
						mainRules.Add(GenRndRule());
					}
				}
				
				if(currentCommand.state == EvolutionState.MATING){
                    mainRules = new List<LRule>();
                    mainRules.Add(currentCommand.ruleToDraw);
				}



				for(int i = 0; i < nrOfRewrites; i++){
					currentGen = i;
					axiom = expandAxiom(axiom, mainRules, new LPosition(masterSpread, masterTurnAngle));
				}
				count++;
			} while(axiom.Length > 10000 || axiom.Length < 1500);

			foreach(var rule in mainRules){
				deb.Log (rule.rule);
            }
            
            container.axiom = axiom.ToString();
			container.initialRule = mainRules[0].rule;
            container.randomSeed = GetComponent<VoxelCubeManager>().randomSeed;
			container.spread = masterSpread;
			container.angle = masterTurnAngle;
		}

		deb.Log("Total Length: " + axiom.Length);

		drawSystem(new LPosition(masterSpread, masterTurnAngle), 0, axiom, false);
		if(connectEndPoints)
			connectEnds(container);

		Vector3 dim = maxPoint-minPoint;
		deb.Log(dim);
		deb.Log(numPoints);
		deb.Log("Percentage points: " + numPoints/(dim.x*dim.y*dim.z));

		//if(numPoints > 20000){
			//manager.GetComponent<LSystemManager>().resetLSystem();
		//}
		/*
		if(!loadSystem)
			container.save(Path.Combine(Application.dataPath, "TestFile.xml"));	
		*/

		if(particleComputer){
			//particleComputer.enabled = true;
			//particleComputer.lPts = lPoints;
			particleComputer.startComputeProcess();
		}

	}

	// Update is called once per frame
	void Update () {

	}

	void OnDrawGizmos(){
		Gizmos.DrawWireCube(constraintBoxSize.center+lPreviewOffset, constraintBoxSize.extents/4);
	}

	public LSysContainer getCurrentLsystem(){
		return container;
	}

	public LRule getCurrentMainRule(){
		return mainRules[0];
	}


	/// <summary>
	/// Expands the axiom.
	/// </summary>
	/// <returns>The axiom.</returns>
	/// <param name="axiom">Axiom.</param>
	/// <param name="rules">Rules.</param>
	private StringBuilder expandAxiom(StringBuilder axiom, List<LRule> rules, LPosition pos){
		LPosition currentPos = new LPosition(pos);
		Stack<LPosition> savedPos = new Stack<LPosition>();
		
		StringBuilder lastResult = axiom;
		axiom = new StringBuilder();
		
		for(int j = 0; j < lastResult.Length; j++){
			char symbol = lastResult[j];
			
			if(symbol != mainSymbol){
				switch(symbol){
				case '[':
					savedPos.Push(currentPos);
					break;
				case ']':
					currentPos = savedPos.Pop();
					break;
	            default:
	                currentPos = calculateDirection(symbol, currentPos);
	                break;
                }
				axiom.Append(symbol);
                continue;
            }
            
			if(currentGen == 0 || UnityEngine.Random.value < expandChance){
				axiom.Append(expandMainSymbol(ref currentPos, rules));
			} else {
				axiom.Append(mainSymbol);
			}
        }
		return axiom;
    }
    
	/// <summary>
	/// Expands the main symbol.
	/// </summary>
	/// <returns>The rule chosen to replace the main symbol</returns>
	/// <param name="curPos">Current position.</param>
	/// <param name="rules">Rules.</param>
    private string expandMainSymbol(ref LPosition curPos,List<LRule> rules){
		List<StochasticRule> stochasticRules = new List<StochasticRule>(); 
		StochasticRule max = new StochasticRule();

		foreach(LRule rule in rules){
			LRule finalRule = new LRule(rule);

			finalRule.rule = insertSubRules(finalRule.rule, curPos);

			finalRule.direction = calculateRuleDir(finalRule.rule, curPos);

			float currentWeight = adjustWeights(finalRule, curPos);
			
			StochasticRule tempRule = new StochasticRule();
			tempRule.probability = UnityEngine.Random.value + currentWeight;
			tempRule.rule = finalRule;
			max = max.maxRule(tempRule);
			stochasticRules.Add(tempRule);
		}

		//Tunnels increase the balanceweight, rooms decrease it.
		if(balancedMode){
			if(max.rule.type == LRuleType.tunnel){
				balanceWeight += balanceStep;
			} else if(max.rule.type == LRuleType.room){
				balanceWeight -= balanceStep;
			}
		}

		curPos = max.rule.direction;
		return max.rule.rule;
	}

	///Parses a main rule, inserting subrules according to their weights
	private string insertSubRules(string stringRule, LPosition curPos){
		List<StochasticRule> stochasticRules = new List<StochasticRule>(); 
		StochasticRule max = new StochasticRule();
		char[] initRule = stringRule.ToCharArray();
		LPosition pos = new LPosition(curPos);
		StringBuilder parsedRule = new StringBuilder();
		StringBuilder rollingString = new StringBuilder();

		foreach(char symbol in initRule){

			if(currentGen == 0 && symbol == '5'){
				parsedRule.Append('F');
				rollingString.Append('F');
				continue;
			}

			List<LRule> curRules = new List<LRule>();

			if(!subRules.TryGetValue(symbol, out curRules)){
				parsedRule.Append(symbol);
				rollingString.Append(symbol);
				continue;
			}

			pos = calculateRuleDir(rollingString.ToString(), pos);
			foreach(LRule orgRule in curRules){
				LRule rule = new LRule(orgRule);

				rule.direction = calculateRuleDir(rule.rule, pos);
				float currentWeight = adjustWeights(rule, pos);

				StochasticRule tempRule = new StochasticRule();
				tempRule.probability = UnityEngine.Random.value + currentWeight;
				tempRule.rule = rule;
				max = max.maxRule(tempRule);
				stochasticRules.Add(tempRule);
			}

			//Tunnels increase the balanceweight, rooms decrease it.
			if(balancedMode){
				if(max.rule.type == LRuleType.tunnel){
					balanceWeight += balanceStep;
				} else if(max.rule.type == LRuleType.room){
                    balanceWeight -= balanceStep;
                }
			}

			pos = max.rule.direction;
			parsedRule.Append(max.rule.rule);
			max = new StochasticRule();
			rollingString = new StringBuilder();
		}

		return parsedRule.ToString();
	}

	///Adjusts the weight of the rule depending on the modes selected.
	private float adjustWeights(LRule rule, LPosition currentPos){
		float currentWeight = rule.weight;

		//Adjusts the weight depending on its direction in respect to the target
		if(targetMode){

			float curDist = Vector3.Distance(target.position*4, currentPos.pos);
			float newDist = Vector3.Distance(target.position*4, rule.direction.pos);
			
			float distweight = ((curDist - newDist)/(masterSpread*1.5f)) * distanceWeight;

			Vector3 direct = (target.position*4-currentPos.pos).normalized * -1;
			Vector3 ruledir = (Quaternion.Euler(rule.direction.rot)*Vector3.forward).normalized;

			float angle = Mathf.Acos(Vector3.Dot(direct, ruledir));

			if(angle > Mathf.PI)
				angle = Mathf.PI*2 - angle;

			float angleweight = angle*distanceWeight;

			currentWeight = (angleweight+distweight)/2;
		}

		//Adjusts the weight depending on how many of the opposite type has come before
		//Tunnels are more likely with a negative balanceweight, rooms with a positive
		if(balancedMode){
			if(rule.type == LRuleType.tunnel){
				currentWeight -= balanceWeight;
			} else if(rule.type == LRuleType.room){
				currentWeight += balanceWeight;
			}
		}

		return currentWeight;
	}

	///Draws the actual L-system and sends the point to the next step in the pipeline
	private int drawSystem(LPosition pos, int lastIndex, StringBuilder axiom, bool evolving, Individual ind = null, Vector3? endPoint = null){
		LPosition localPos = new LPosition(pos);
		int index = lastIndex;
		
		while(index < axiom.Length){
			if(endPoint != null){
				if(Vector3.Distance(localPos.pos, endPoint.Value) < masterSpread*2){
					drawLine(localPos.pos, endPoint.Value);
					break;
				}
			}


			switch(axiom[index]){
			case 'F':
				LPosition lastPos = new LPosition(localPos);
				localPos.rot = NormalizeAngles(localPos.rot);
				localPos.pos += Quaternion.Euler(localPos.rot) * new Vector3(0, 0, localPos.spread);
				localPos.pos = new Vector3(Mathf.Round(localPos.pos.x), Mathf.Round(localPos.pos.y), Mathf.Round(localPos.pos.z));

				if(constraintBoxMode){
					if(!constraintBoxSize.Contains(lastPos.pos)){
						int prevEnd = endPointList.BinarySearch(lastPos);
						if( prevEnd < 0){
	                       	endPointList.Add(lastPos);
                        }
						localPos = lastPos;
                        break;
					}
				}

				if(constraintSphereMode){
					if(Vector3.Distance(localPos.pos, Vector3.zero) > constraintSphereRadius)
						break;
				}


				if(!evolving){
					LSysLine curLine = new LSysLine(lastPos, localPos);
					if(!lineSet.Contains(curLine)){
						if(checkIfEndPoint(index, axiom)){
							int prevEnd = endPointList.BinarySearch(lastPos);
							if( prevEnd >= 0){
								endPointList[prevEnd] = localPos;
							} else {
								endPointList.Add(localPos);
							}
						}
						lineSet.Add(curLine);
						drawLine(lastPos.pos, localPos.pos); 
					}
				} else {
					/*
					ind.numPoints += Mathf.RoundToInt(Vector3.Distance(lastPos.pos, localPos.pos));
					float maxX = Mathf.Max(ind.dimensions.x, localPos.pos.x);
					float maxY = Mathf.Max(ind.dimensions.y, localPos.pos.y);
					float maxZ = Mathf.Max(ind.dimensions.z, localPos.pos.z);
					
					ind.dimensions = new Vector3(maxX, maxY, maxZ);
					*/
				}

				maxPoint.x = Mathf.Max(maxPoint.x, localPos.pos.x);
				maxPoint.y = Mathf.Max(maxPoint.y, localPos.pos.y);
				maxPoint.z = Mathf.Max(maxPoint.z, localPos.pos.z);

				minPoint.x = Mathf.Min(minPoint.x, localPos.pos.x);
				minPoint.y = Mathf.Min(minPoint.y, localPos.pos.y);
				minPoint.z = Mathf.Min(minPoint.z, localPos.pos.z);
				break;									
			case '[':
				index = drawSystem(localPos, index+1, axiom, evolving, ind);
	            break;
	        case ']':
	            return index;
	            //break;
	        default:
	            localPos = calculateDirection(axiom[index], localPos);
	            break;
            }
            index++;
        }
		return index;
	}

	///Connects the endpoints of the branches chosen in drawSystem() together.
	private void connectEnds(LSysContainer container){

		if(loadSystem || currentCommand.state == EvolutionState.LOADING){
			foreach(LSysLine line in container.connections){
				drawRandomLine(line.start, line.end);
			}
		} else {
			int[] numbers = new int[endPointList.Count];
			for(int i = 0; i < endPointList.Count; i++){
				numbers[i] = i;
			}

			int[] perm = numbers.OrderBy(x => UnityEngine.Random.Range(0, int.MaxValue)).ToArray();

			bool start = true;
			LPosition startPos = new LPosition(masterSpread, masterTurnAngle);

			foreach(int num in perm){
				if(UnityEngine.Random.value < endPointConnectChance){
					if(start){
						startPos = endPointList[num];
						LSysLine line = new LSysLine();
						line.start = startPos;
						container.connections.Add(line);
						start = false;
					} else {
						LPosition endPoint = endPointList[num];
						if(Vector3.Distance(endPoint.pos, container.connections.Last().start.pos) <= endPointMaxDist){
							container.connections.Last().end = endPoint;
							drawRandomLine(startPos, endPoint);
							start = true;
						} else {
							container.connections.Last().start = endPoint;
							start = false;
						}
					}
				}
			}

			//If We're missing an endpoint at the end of the list, we just remove the last element
			if(container.connections.Count > 0 && container.connections.Last().end.pos == Vector3.zero){
				container.connections.RemoveAt(container.connections.Count-1);
			}
		}
	}

	private void drawRandomLine(LPosition start, LPosition end){
		Vector3 point = start.pos;


		//Instantiate(debugSphere, new Vector3(start.pos.x+lPreviewOffset.x, start.pos.y+lPreviewOffset.y, start.pos.z+lPreviewOffset.z), Quaternion.identity);
		//Instantiate(debugSphere, new Vector3(end.pos.x+lPreviewOffset.x, end.pos.y+lPreviewOffset.y, end.pos.z+lPreviewOffsez), Quaternion.identity);
		float div = 0.175f;//8;
		GameObject ob = Instantiate(debugSphere, new Vector3(start.pos.x*div+lPreviewOffset.x, start.pos.y*div+lPreviewOffset.y, start.pos.z*div+lPreviewOffset.z), Quaternion.identity) as GameObject;
		ob.transform.parent = blockContainer;
		GetComponent<CleanUpList>().objects.Add(ob);
		GameObject ob2 = Instantiate(debugSphere, new Vector3(end.pos.x*div+lPreviewOffset.x, end.pos.y*div+lPreviewOffset.y, end.pos.z*div+lPreviewOffset.z), Quaternion.identity) as GameObject;
		ob2.transform.parent = blockContainer;
		GetComponent<CleanUpList>().objects.Add(ob2);



		Vector3 dist = (end.pos - start.pos)*2;
		int N = (int)Mathf.Max(Mathf.Abs (dist.x), Mathf.Max (Mathf.Abs (dist.y), Mathf.Abs (dist.z)));
		Vector3 step = (dist / N)/2;

		for (int i = 0; i < N; i++) {
			numPoints++;
			point = point + step;
			VoxelCubeManager tmp = GetComponent<VoxelCubeManager>();
			Vector3 noisePoint = point;
			noisePoint.x += tmp.randomSeed[0];
			noisePoint.y += tmp.randomSeed[1];
			noisePoint.z += tmp.randomSeed[2];
			Vector3 noise = SimplexNoise.Noise.get_Curl(noisePoint/endPointFrequency)/endPointAmplitude;
			if(point.y == 0)
				point.y = 1;
			Vector3 modulatedPoint = point + new Vector3(point.x*noise.x, point.y * (noise.y/2), point.z*noise.z)/2;
			PipeOutThisPoint(modulatedPoint);
			if(i == 0){
				drawLine(point-step, modulatedPoint);
			} else if(i == N-1){
				drawLine(modulatedPoint, end.pos);
			}

		}
	}


	///Draws a rasterized line between two points
	private void drawLine(Vector3 start, Vector3 end){
		Vector3 point = start;
		numPoints++;

		Vector3 dist = end - start;
		int N = (int)Mathf.Max(Mathf.Abs (dist.x), Mathf.Max (Mathf.Abs (dist.y), Mathf.Abs (dist.z)));
		Vector3 step = dist / N;
		
		for (int i = 0; i < N; i++) {
			point = point + step;
            PipeOutThisPoint(point);
			numPoints++;
        }
    }

	///Checks if there is any more forward symbols between the current and the end of the branch (])
	private bool checkIfEndPoint(int index, StringBuilder axiom){
		int offset = 1;
		bool endPoint = true;

		while(index+offset < axiom.Length && axiom[index+offset] != ']'){
			if(axiom[index+offset] == 'F' || axiom[index+offset] == '0'){
				endPoint = false;
				break;
			}
			offset++;
		}

		return endPoint;
	}

   ///Loads a set of rules from a rules file
	private void loadRules(){
		//Syntax:
		//Symbol;Rule;type;weight;
		
		
		String[] rawSplitRules = rawRules.text.Split(new String[] {"\n"}, StringSplitOptions.None);
		
		foreach(String rawRule in rawSplitRules){

			//Rules can be commented with with a /, and empty lines can be put in the rule file, and be ignored by the parser
			if(rawRule.Equals("") || rawRule[0] == '/')
				continue;

			String[] splitRule = rawRule.Split(new String[] {";"}, StringSplitOptions.None);
			LRule newRule = new LRule(masterSpread, masterTurnAngle);

			newRule.symbol = splitRule[0][0];
			newRule.rule = splitRule[1];
			newRule.type = (LRuleType)Enum.Parse(typeof(LRuleType), splitRule[2]);
			try{
				newRule.weight = Convert.ToSingle(splitRule[3]);
			} catch (Exception e){
				print(e.Message);
				return;
			}

			if(newRule.symbol == mainSymbol){
				mainRules.Add(newRule);
			} else {
				if(subRules.ContainsKey(newRule.symbol)){
					subRules[newRule.symbol].Add(newRule);
				} else {
					subRules.Add(newRule.symbol, new List<LRule>(){newRule});
				}
			}
		}
	}

	///Calculates the rotation added by rotation symbols
	private LPosition calculateDirection(char symbol, LPosition curPos){ 
		LPosition retVal = new LPosition(curPos);
		int angleChange = UnityEngine.Random.Range(-angledeviation, angledeviation);


		switch(symbol){
		case 'L':
			retVal.rot.y -= curPos.turnAngle+angleChange;
			break;
		case 'R':
			retVal.rot.y += curPos.turnAngle+angleChange;
			break;
		case '+':
			retVal.rot.z += curPos.turnAngle+angleChange;
			break;
		case '-':
			retVal.rot.z -= curPos.turnAngle+angleChange;
			break;
		case 'U':
			retVal.rot.x -= curPos.turnAngle+angleChange;
			if((retVal.rot.x > slopeLimit || retVal.rot.x <-slopeLimit) && slopeLimit > 0){
				retVal.rot.x = -slopeLimit;
	        }
	        break;
		case 'D':
			retVal.rot.x += curPos.turnAngle+angleChange;
			if((retVal.rot.x > slopeLimit || retVal.rot.x <-slopeLimit) && slopeLimit > 0){
				retVal.rot.x = slopeLimit;
	        }
	        break;
		case 'O':
			retVal.turnAngle += angleStep;
			break;
		case 'A':
			retVal.turnAngle -= angleStep;
			break;
		case 'B':
			retVal.spread += spreadStep;
			break;
		case 'S':
			retVal.spread -= spreadStep;
			if(retVal.spread < 1)
				retVal.spread = 1;
			break;
	    default:
	        break;
        }
        
        return retVal;
    }

	///Calculates the position that the L-system would be at after parsing this rule
	private LPosition calculateRuleDir(string ruleString, LPosition pos){
		LPosition curPos = new LPosition(pos);
		char[] rule = ruleString.ToCharArray();
		int depth = 0;
		
		
		foreach(char sym in rule){
			if(depth > 0 && sym != ']')
				continue;
			
			switch(sym){
			case 'F':
				curPos.rot = NormalizeAngles(curPos.rot);
				curPos.pos += Quaternion.Euler(curPos.rot) * new Vector3(0, 0, curPos.spread);
				break;
			case '[':
				depth++;
				break;
			case ']':
				depth--;
				break;
	        default:
	            curPos = calculateDirection(sym, curPos);
				break;
            }
        }
        
        return curPos;
    }
    
	private Vector3 NormalizeAngles (Vector3 angles)
	{
		angles.x = NormalizeAngle (angles.x);
		angles.y = NormalizeAngle (angles.y);
		angles.z = NormalizeAngle (angles.z);
		return angles;
	}
	
	private float NormalizeAngle (float angle)
	{
		while (angle>360)
			angle -= 360;
		while (angle<0)
			angle += 360;
		return angle;
	}


	/*
	 * Following code deals with evolving the main rule.
	 */

	

	///Generate an individual with random subrules
	private LRule GenRndRule(){
		LRule retVal = new LRule(masterSpread, masterTurnAngle);
		StringBuilder rule = new StringBuilder();
		int length = UnityEngine.Random.Range(minInitialLength, maxInitialLength);
		bool bBrackets = false;
		int depth = 0;
		bool doubleBracket = false;
		Stack<IntVector2> bracketStack = new Stack<IntVector2>();

		for(int i = 0; i < length; i++){
			if(UnityEngine.Random.value < bracketChance && !doubleBracket && i != 0){
				bBrackets = true;
				if(depth == 0){
					rule.Append('[');
					doubleBracket = true;
					depth++;
					bracketStack.Push(new IntVector2(i, 0));
				} else {
					if(UnityEngine.Random.value < 0.5){ //TODO: MAke a better way to choose if we should start a new bracket or end the last.
						rule.Append('[');
						doubleBracket = true;
						depth++;
						bracketStack.Push(new IntVector2(i, 0));
					} else {
						rule.Append(']');
						depth--;
						IntVector2 tmpBracket = bracketStack.Pop();
						tmpBracket.y = i;
						retVal.bracketList.Add(tmpBracket);
					}
				}
				length++; //To avoid brackets counting
			} else {
				doubleBracket = false;
				SubRuleType maxRule = SubRuleType.LINE;
				float maxRoll = float.NegativeInfinity;

				foreach(SubRuleType type in Enum.GetValues(typeof(SubRuleType))){
					float weight;
					if(!sWeights.TryGetValue(type, out weight))
						weight = 0.0f;
					float roll = weight + UnityEngine.Random.value;
					if(roll > maxRoll){
						maxRoll = roll;
						maxRule = type;
					}
				}

				rule.Append((char)maxRule);
			}
		}
		
		for(int i = depth; i > 0; i--){
			rule.Append(']');
			IntVector2 tmpBracket = bracketStack.Pop();
			tmpBracket.y = rule.Length-1;
			retVal.bracketList.Add(tmpBracket);
        }

		if(!bBrackets){
			int start = UnityEngine.Random.Range (0, rule.Length-2);
			rule.Insert(start, "[");
			int end = UnityEngine.Random.Range(start+2, rule.Length);
			rule.Insert(end, "]");
			IntVector2 brackets = new IntVector2();
			brackets.x = start;
			brackets.y = end;
			retVal.bracketList.Add(brackets);
		}


		retVal.rule = rule.ToString();
		retVal.symbol = mainSymbol;
		return retVal;
	}




	/// <summary>
	/// Fills the dictionary with ruletypes and weights.
	/// </summary>
	private void fillSWeights(){
		sWeights.Add(SubRuleType.CURVE, initialWeights.curveWeight);
		sWeights.Add(SubRuleType.HUMP, initialWeights.humpWeight);
		sWeights.Add(SubRuleType.LINE, initialWeights.lineWeight);
		sWeights.Add(SubRuleType.ROOM, initialWeights.roomWeight);
		sWeights.Add(SubRuleType.UTURN, initialWeights.uTurnWeight);
	}


	private void destroyEverything(){
		foreach(GameObject obj in GetComponent<CleanUpList>().objects)
			Destroy(obj);
		Instantiate(this.gameObject);
		Destroy(this.gameObject);
	}


    /*
	 *The following functions handles piping the point to the next step in the pipeline. 
	 */


	/// <summary>
	/// Pipes the point to voxel manager.
	/// </summary>
	/// <param name="point">Point.</param>
	private void pipePtToVoxelManager(Vector3 point){
		
		if(trimPoint())
			TileFactory.AddPointToATile(new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z)));
		
	}
	
	/// <summary>
	/// Pipes the point to both.
	/// </summary>
	/// <param name="point">Point.</param>
	private void pipePtToBoth(Vector3 point){
		point*=0.7f;//point/=2;
		//if(trimPointGrid(point))
		if(trimPoint()){
			TileFactory.AddPointToATile(new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z)));
		}
		if(gCount % 20 == 0){
			point/=4;
			GameObject ob = Instantiate(largeBlock, new Vector3(point.x+lPreviewOffset.x, point.y+lPreviewOffset.y, point.z+lPreviewOffset.z), Quaternion.identity) as GameObject;
			//ob.transform.parent = largeBlockContainer;
			//GetComponent<CleanUpList>().objects.Add(ob);
			ob.transform.parent = bigParent.transform;
			//DOoesn't work very well with the largeBlock icosahedron. it's slow as fuck. tried with sprites
			//(Instantiate(block, new Vector3(point.x+lPreviewOffset.x, point.y+lPreviewOffset.y, point.z+lPreviewOffset.z), Quaternion.identity) as GameObject).transform.parent = blockContainer;

		}
		else if(gCount % 2 == 0){
			point/=4;
			//(Instantiate(block, new Vector3(point.x-30, point.y, point.z), Quaternion.identity) as GameObject).transform.parent = blockContainer;
			GameObject ob = Instantiate(block, new Vector3(point.x+lPreviewOffset.x, point.y+lPreviewOffset.y, point.z+lPreviewOffset.z), Quaternion.identity) as GameObject;
			//ob.transform.parent = blockContainer;
			//GetComponent<CleanUpList>().objects.Add(ob);
			ob.transform.parent = smallParent.transform;
		}
	}

	/// <summary>
	/// Pipes the point to world.
	/// </summary>
	/// <param name="point">Point.</param>
	private void pipePtToWorld(Vector3 point){
		//float mod = (stateManager.cubeSize/2)* stateManager.cubeSpacing;
        float mod = 60;//28;
        Instantiate(block, new Vector3(point.x+mod, point.y+mod, point.z), Quaternion.identity);
	}

	/// <summary>
	/// Trims the point.
	/// </summary>
	/// <returns><c>true</c>, if point was trimed, <c>false</c> otherwise.</returns>
	private bool trimPoint(){
		if(gCount % displayOneEvery == 0){
			gCount++;
			return true;
		}
		else{
            gCount++;
            return false;
        }
    }

	private bool trimPointGrid(Vector3 point){
		//if(point.x % displayOneEvery == 0 || point.y % displayOneEvery == 0 || point.z % displayOneEvery == 0){
		//point*=10000;
		//if(SimplexNoise.Noise.Generate(point.x, point.y, point.z) == lPointSampleFilter){

		//if((point.x+ point.y+ point.z)%lPointSampleFilter == 0){
		if((point.x+ point.y)%lPointSampleFilter == 0 ||
		   (point.y+ point.z)%lPointSampleFilter == 0 ||
		   (point.x+ point.z)%lPointSampleFilter == 0
		   ){
			return true;
			//return trimPoint();
		}
		else{
			//gCount++;
			return false;
		}

		//return trimPoint();
	}

}