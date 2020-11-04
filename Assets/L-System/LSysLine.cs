
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class LSysLine
{
	public LPosition start {get;set;}
	public LPosition end {get;set;}

	public LSysLine (){
		start = new LPosition();
		end = new LPosition();
	}

	public LSysLine(LPosition start, LPosition end){
		this.start = start;
		this.end = end;
	}
}


