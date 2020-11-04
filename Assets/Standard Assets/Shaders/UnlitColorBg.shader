Shader "Custom/UnlitColorBg" {

 

Properties {

    _Color ("Color", Color) = (1,1,1)

}

 

SubShader {
	//Fog { Mode Off }
	Fog { Mode Off }
	//Fog { Color [_AddFog] }
	//Fog { Mode Exp2 }
	Lighting Off
    ZWrite Off//make sure not to write to the depth buffer. Else pixels behind will be occluded.
            //ZWrite On  // uncomment if you have problems like the sprite disappear in some rotations.
    Cull Off //Back 
    Blend SrcAlpha OneMinusSrcAlpha //SrcAlpha OneMinusSrcAlpha //
            //AlphaTest Greater 0.001  // uncomment if you have problems like the sprites or 3d text have white quads instead of alpha pixels.
    Tags {"RenderType"="Transparent" "Queue"="Background+30"}//"Queue"="Background+2500"}//{Queue=Transparent}
 
 
    Color [_Color]

    Pass {}

} 

 

}
