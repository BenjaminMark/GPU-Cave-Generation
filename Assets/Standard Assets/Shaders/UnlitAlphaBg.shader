Shader "Custom/UnlitAlphaBg" {

    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)   
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white"
    }
 
 //transparent shader in backgroun dqueue. not working? 
 //http://answers.unity3d.com/questions/494760/transparent-shader-in-background-queue.html
    Category
    {
        Lighting Off
        ZWrite Off//make sure not to write to the depth buffer. Else pixels behind will be occluded.
                //ZWrite On  // uncomment if you have problems like the sprite disappear in some rotations.
        Cull Off //Back 
        Blend SrcAlpha OneMinusSrcAlpha //SrcAlpha OneMinusSrcAlpha //
                //AlphaTest Greater 0.001  // uncomment if you have problems like the sprites or 3d text have white quads instead of alpha pixels.
        Tags {"RenderType"="Transparent" "Queue"="Background+20"}//"Queue"="Background+2500"}//{Queue=Transparent}
 
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