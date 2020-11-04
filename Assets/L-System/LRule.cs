
using System;
using UnityEngine;
using System.Collections.Generic;

public class LRule{

	public char symbol;
	public string rule;
	public LRuleType type;
	public LPosition direction;
	public float weight;
	public List<IntVector2> bracketList;


	public LRule (int initSpread, int initAngle){
		symbol = '0';
		rule = "";
		type = LRuleType.tunnel;
		direction = new LPosition(initSpread, initAngle);
		weight = 0.0f;
		bracketList = new List<IntVector2>();
	}

	public LRule (LRule rule){
		this.symbol = rule.symbol;
		this.rule = rule.rule;
		this.type = rule.type;
		this.direction = new LPosition(rule.direction);
		this.weight = rule.weight;
		this.bracketList = new List<IntVector2>(rule.bracketList);
	}

	public LRule(){
		symbol = '0';
		rule = "";
		type = LRuleType.tunnel;
		direction = new LPosition();
		weight = 0.0f;
		bracketList = new List<IntVector2>();
	}
}
