

Shader "Noise/BlitNoise2D" 
{
	Properties 
	{
    	_MainTex("MainTex", 2D) = "black" { }
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	#include "ImprovedVoronoiNoise2D.cginc"
		
	sampler2D _MainTex;
	
	struct v2f 
	{
	    float4 pos : SV_POSITION;
	    float2 uv : TEXCOORD;
	};
	
	v2f vert (appdata_base v)
	{
	    v2f o;
	    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	    o.uv = v.texcoord.xy;
	    return o;
	}
	
	float4 Fractal_F0(v2f IN) : COLOR
	{
		float2 F = sqrt(inoise(IN.uv * _Frequency, _Jitter) * _Amp);
	    return tex2D(_MainTex, IN.uv) + F.xxxx;
	}
	
	float4 Fractal_F1_F0(v2f IN) : COLOR
	{
		float2 F = sqrt(inoise(IN.uv * _Frequency, _Jitter) * _Amp);
	    return tex2D(_MainTex, IN.uv) + F.yyyy - F.xxxx;
	}
	
	ENDCG
			
	SubShader 
	{
	    Pass 
		{
			ZTest Always Cull Off ZWrite Off
	  		Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment Fractal_F0
			#pragma target 3.0
			#pragma glsl
			#include "UnityCG.cginc"
			ENDCG
		}
		
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
	  		Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment Fractal_F1_F0
			#pragma target 3.0
			#pragma glsl
			#include "UnityCG.cginc"
			ENDCG
		}
		
	}
	
}

