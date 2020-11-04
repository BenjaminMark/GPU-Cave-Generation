//
Shader "Custom/OutlinedInvisibleCircle" {

    Properties 
    {
        _RampTex ("Color Ramp", 2D) = "white" {}
        _DispTex ("Displacement Texture", 2D) = "gray" {}
        _Displacement ("Displacement", float) = 0.1//Range(0, 2.0)) = 0.1
        _ChannelFactor ("ChannelFactor (r,g,b)", Vector) = (1,0,0,0)//(1,0,0)
        _Range ("Range (min,max)", Vector) = (0,0.5,0)
        _ClipRange ("ClipRange [0,1]", float) = 0.8
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range(-0.8, 0.5)) = 0.05 //Range(0, 0.03)) = 0.005

    }
 
 	CGINCLUDE
	#include "UnityCG.cginc"
 
	sampler2D _DispTex;
	float _Displacement;
	float3 _ChannelFactor;
	float2 _Range;
	float _ClipRange;
	sampler2D _RampTex;
	
	
	struct Input 
	{
		float2 uv_DispTex;
	}; 	
 	
	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};
	
	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};
	
	uniform float _Outline;
	uniform float4 _OutlineColor;
	
	
	ENDCG
  
    
    SubShader 
    {
 		//Tags { "RenderType"="Transparent" "Queue"="Transparent"}
	    //Blend SrcAlpha OneMinusSrcAlpha

 		
 		//SubShader
			//Name "MODEL"
	 		//Tags { "RenderType"="Transparent" "Queue"="Transparent"}
	 		Tags { "Queue"="Transparent"}
	        Blend SrcAlpha OneMinusSrcAlpha
	        Fog { Mode Off } 
	        Cull back//Off
	        //ZTest GEqual 
	        LOD 300
	 		
	        CGPROGRAM
	        #pragma surface surf Lambert vertex:disp nolightmap
	        #pragma target 3.0
	        #pragma glsl
	 
	        
	 
	        void disp (inout appdata_full v)
	        {
	            //float3 dcolor = tex2Dlod (_DispTex, float4(v.texcoord.xy,0,0));
	            //float d = (dcolor.r*_ChannelFactor.r + dcolor.g*_ChannelFactor.g + dcolor.b*_ChannelFactor.b);
	            //v.vertex.xyz += v.normal * d * _Displacement;
	            
	        }
	 
	        
	 
	        void surf (Input IN, inout SurfaceOutput o) 
	        {
	        
	            //float3 dcolor = tex2D (_DispTex, IN.uv_DispTex);
	            //float d = (dcolor.r*_ChannelFactor.r + dcolor.g*_ChannelFactor.g + dcolor.b*_ChannelFactor.b) * (_Range.y-_Range.x) + _Range.x;
	            //clip (_ClipRange-d);
	            //half4 c = tex2D (_RampTex, float2(d,0.5));
	            half4 c = fixed4(0,0,0,0);
	            o.Albedo = c.rgb;
	            o.Emission = c.rgb;//*c.a;
	            o.Alpha = 0;//c.a;
	        }
	        ENDCG
	    // /SubShader
	    
	    Pass {
			Name "BASE"
			Cull Back
			Blend Zero One

			// uncomment this to hide inner details:
			//Offset -8, -8

			SetTexture [_OutlineColor] {
				ConstantColor (0,0,0,0)
				Combine constant
			}
		}
 		
 		// note that a vertex shader is specified here but its using the one above
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always"}
			//Tags { "LightMode" = "Always" "Queue"="Background"}
			Cull Front

			// you can choose what kind of blending mode you want for the outline
			//Blend SrcAlpha OneMinusSrcAlpha // Normal
			//Blend One One // Additive
			Blend One OneMinusDstColor // Soft Additive
			//Blend DstColor Zero // Multiplicative
			//Blend DstColor SrcColor // 2x Multiplicative
 		
 			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
	        #pragma glsl
			
			v2f vert(appdata_full v) {
				// just make a copy of incoming vertex data but scaled according to normal direction
				v2f o;
				
				float3 dcolor = tex2Dlod (_DispTex, float4(v.texcoord.xy,0,0));
	            float d = (dcolor.r*_ChannelFactor.r + dcolor.g*_ChannelFactor.g + dcolor.b*_ChannelFactor.b);
	            v.vertex.xyz += v.normal * d * _Displacement;
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);
			
				o.pos.xy += offset * _Outline;//* o.pos.z //<<-- THIS doesn't work in ortographic cameras, for obvious reasons.
				o.color = _OutlineColor;
				return o;
			}
			
			half4 frag(v2f i) :COLOR {
				return i.color;
			}
			ENDCG
		}
	    
	    
    }
    FallBack "Diffuse"
}


