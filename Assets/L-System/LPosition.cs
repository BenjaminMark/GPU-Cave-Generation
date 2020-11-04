using System;
using UnityEngine;
using System.Collections.Generic;

public class LPosition : IComparable<LPosition>
{
	public Vector3 pos;
	public Vector3 rot;
	public int spread;
	public int turnAngle;

	public LPosition (int spread, int angle){
		this.pos = Vector3.zero;
		this.rot = Vector3.zero;
		this.spread = spread;
		this.turnAngle = angle;
	}

	public LPosition(LPosition pos){
		this.pos = pos.pos;
		this.rot = pos.rot;
		this.spread = pos.spread;
		this.turnAngle = pos.turnAngle;
	}

	public LPosition(){
		this.pos = Vector3.zero;
		this.rot = Vector3.zero;
		this.spread = 20;
		this.turnAngle = 45;
	}

	public int CompareTo(LPosition lhs){
		if(this.pos.sqrMagnitude < lhs.pos.sqrMagnitude)
			return -1;
		
		if(this.pos == lhs.pos)
			return 0;

		return 1;
	}
}


