Shader "Custom/UnlitColorOnTop" {

 

Properties {

    _Color ("Color", Color) = (1,1,1)

}

 

SubShader {
	//Fog { Mode Off }
	Fog { Mode Off }
	//Fog { Color [_AddFog] }
	//Fog { Mode Exp2 }
    Lighting Off
    Tags { "RenderType"="Opaque" "Queue"="Transparent"}
    ZTest Always ZWrite On//Off //ZTest Always//LEqual
    //ZWrite Off
            //ZWrite On  // uncomment if you have problems like the sprite disappear in some rotations.
       
    Cull Back
    //BlendOp Max
	//Blend one one
    Blend SrcAlpha OneMinusSrcAlpha
    //Blend SrcAlpha OneMinusSrcAlpha
            //AlphaTest Greater 0.001  // uncomment if you have problems like the sprites or 3d text have white quads instead of alpha pixels.
    //Tags {Queue=Transparent}
	
    Color [_Color]

    Pass {}

} 

 

}
