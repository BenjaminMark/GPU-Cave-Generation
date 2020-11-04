using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;


public class LSystemSettings
{
	public Transform blockContainer;
	public GameObject block;
	public Transform largeBlockContainer;
	public GameObject largeBlock;
	public GameObject debugSphere;
	public string initialString = "F";
	public int masterSpread = 1;
	public int masterTurnAngle = 30;
	public int spreadStep = 1;
	public int angleStep = 5;
	public int generations = 2;
	public bool forceDrawLsystemTree = false;
	public float slopeLimit = 1.0f;
	public bool balancedMode = false;
	public float balanceStep = 0.05f;
	public bool targetMode = false;
	public Transform target;
	public float distanceWeight = 1;
	public bool constraintBoxMode = false;
	public Vector3 constraintBoxSize;
	public bool constraintSphereMode = false;
	public int constraintSphereRadius = 0;
	public TextAsset rawRules;
	public SymbolWeights initialWeights = new SymbolWeights();
	public bool connectEndPoints = true;
	public float endPointConnectChance = 0.3f;
	public bool loadSystem = false;
	//
	//List<Vector3> lPoints;
	public int displayOneEvery = 7;
	//private int lPointSampleFilter = 2;
	public bool evolveSystem = true;
	public int numMainRules = 1;
	public int minInitialLength = 5;
    public int maxInitialLength = 10;
    public float bracketChance = 0.4f;

	public LSystemSettings ()
	{

	}
}


