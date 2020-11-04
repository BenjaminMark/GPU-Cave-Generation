
shader "Noise/TessellationFixed2D" 
{
    Properties 
    {
        _Tess ("Tessellation", float) = 4
        _Displacement ("Displacement", float) = 3
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", color) = (1,1,1,0)
        _Frequency("Frequency", float) = 10.0
		_Lacunarity("Lacunarity", float) = 2.0
		_Gain("Gain", float) = 0.5
		_Jitter("Jitter", Range(0,1)) = 1.0
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Lambert addshadow fullforwardshadows vertex:disp tessellate:tessFixed nolightmap
        #pragma target 5.0
        #include "ImprovedVoronoiNoise2D.cginc"
        #define OCTAVES 1
        #define EPSILON 0.01
		
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _DispTex;
        fixed4 _Color;
        float _Tess;
        float _Displacement;
        
        struct appdata 
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };
        
        struct Input 
        {
            float2 uv_MainTex;
        };

        float4 tessFixed()
        {
            return _Tess;
        }

        void disp(inout appdata v)
        {
            float d = fBm_F1_F0(v.texcoord.xy, OCTAVES) * _Displacement;
            v.vertex.xyz += v.normal * d;
        }
        
        float3 FindNormal(float2 uv, float u)
        {
            float ht0 = fBm_F1_F0(uv + float2(-u, 0), OCTAVES);
            float ht1 = fBm_F1_F0(uv + float2(u, 0), OCTAVES);
            float ht2 = fBm_F1_F0(uv + float2(0, -u), OCTAVES);
            float ht3 = fBm_F1_F0(uv + float2(0, u), OCTAVES);

            float3 va = normalize(float3(float2(0.1, 0.0), ht1-ht0));
            float3 vb = normalize(float3(float2(0.0,0.1), ht3-ht2));

           return cross(va,vb);
        }

        void surf(Input IN, inout SurfaceOutput o) 
        {
        
        	float3 norm = FindNormal(IN.uv_MainTex, EPSILON);

            o.Albedo = _Color;
            o.Normal = norm;
        }
        ENDCG
    }
    FallBack "Diffuse"
}