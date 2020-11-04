
using System;
using UnityEngine;
using System.Collections.Generic;

public class LSysLineEquator : IEqualityComparer<LSysLine>
{
	public bool Equals(LSysLine lhs, LSysLine rhs){
		return lhs.start == rhs.start && lhs.end == rhs.end;
	}

	public int GetHashCode(LSysLine line){
		return line.start.GetHashCode() + line.end.GetHashCode();
	}
}

