using UnityEngine;
using System.Collections;

public static class UtilityScript {



	public static bool arrayApproximately(float[] prevTarget, float[] target){
		
		for(int i=0; i< target.Length; i++){
			
			if( !Mathf.Approximately(target[i], prevTarget[i]))
				return false;
		}
		return true;
	}


}
