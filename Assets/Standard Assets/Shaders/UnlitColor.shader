Shader "Custom/UnlitColor" {

 

Properties {

    _Color ("Color", Color) = (1,1,1)

}

 

SubShader {
	//Fog { Mode Off }
	Fog { Mode Off }
	//Fog { Color [_AddFog] }
	//Fog { Mode Exp2 }
	
    Color [_Color]

    Pass {}

} 

 

}
