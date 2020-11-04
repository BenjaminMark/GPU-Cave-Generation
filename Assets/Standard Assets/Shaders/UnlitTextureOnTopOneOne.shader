Shader "Custom/UnlitTextureOnTopOneOne" {

    Properties
    {
    	_Color ("Color", Color) = (1,1,1)
        //_Color ("Color Tint", Color) = (1,1,1,1)   
        _MainTex ("Base (RGB)", 2D) = "white"
    }
 
    Category
    {
    	Fog { Mode Off }
        Lighting Off
        Tags { "RenderType"="Opaque" "Queue"="Transparent+100"}
        
		ZTest Always ZWrite On//Off //ZTest Always//LEqual
        //ZWrite Off
                //ZWrite On  // uncomment if you have problems like the sprite disappear in some rotations.
        LOD 200
        Cull Back
        //BlendOp Max
		Blend one one
        //Blend SrcAlpha OneMinusSrcAlpha
        //Blend SrcAlpha OneMinusSrcAlpha
                //AlphaTest Greater 0.001  // uncomment if you have problems like the sprites or 3d text have white quads instead of alpha pixels.
        //Tags {Queue=Transparent}
 
        SubShader
        {
 
             Pass
             {
                SetTexture [_MainTex]
                {
                   ConstantColor [_Color]
                   Combine Texture * constant
                }
            }
        }
    }
}