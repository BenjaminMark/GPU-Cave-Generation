Shader "Custom/BasicTessellation-RC-1290" {

    Properties {
    	_Color ("Main Color", Color) = (.5,.5,.5,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Offset ("Offset", Float) = 10
        _Tess ("Tessellation", Float) = 2
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
			#include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
			
            #pragma hull tessBase
            #pragma domain basicDomain
            #pragma target 5.0
            #define INTERNAL_DATA

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Offset;
            float _Tess;
            
            fixed4 _Color;

            struct v2f{
                float4 pos : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 tangent : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

        #ifdef UNITY_CAN_COMPILE_TESSELLATION
            struct inputControlPoint{
                float4 position : WORLDPOS;
                float4 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
            };

            struct outputControlPoint{
                float3 position : BEZIERPOS;            
            };

            struct outputPatchConstant{
                float edges[3]        : SV_TessFactor;
                float inside        : SV_InsideTessFactor;

                float3 vTangent[4]    : TANGENT;
                float2 vUV[4]         : TEXCOORD;
                float3 vTanUCorner[4] : TANUCORNER;
                float3 vTanVCorner[4] : TANVCORNER;
                float4 vCWts          : TANWEIGHTS;
            };


            outputPatchConstant patchConstantThing(InputPatch<inputControlPoint, 3> v){
                outputPatchConstant o;
              
  			    o.edges[0] = _Tess;
                o.edges[1] = _Tess;
                o.edges[2] = _Tess;
                o.inside = _Tess;

                return o;
            }

             // tessellation hull shader
            [domain("tri")]
            [partitioning("fractional_odd")]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("patchConstantThing")]
            [outputcontrolpoints(3)]
            inputControlPoint tessBase (InputPatch<inputControlPoint,3> v, uint id : SV_OutputControlPointID) {
                
				return v[id];
            }
        #endif // UNITY_CAN_COMPILE_TESSELLATION

            v2f vert (appdata_tan v){
                v2f o;

                o.texcoord = v.texcoord;
                o.pos = v.vertex;
                o.normal = normalize(v.normal);
                o.tangent = v.tangent;

                return o;
            }


            v2f displace (appdata_tan v){
                v2f o;        

                o.texcoord = TRANSFORM_TEX (v.texcoord, _MainTex);

                
                
                float3 blend_weights = abs(v.normal);
				// Tighten up the blending zone:
				blend_weights = ( blend_weights -0.05) ;//*7 0.2  //the - is the transition zone width; *x doesn't seem to do anything
				blend_weights = max(blend_weights, 0);
				// Force weights to sum to 1.0 (very important!)  
				blend_weights /= ((blend_weights.x + blend_weights.y + blend_weights.z)  ).xxx; 
				
				float4 blended_color; // .w hold spec value 
				float3 blended_bump_vec;  
				
				//float3 getCurl = get_Curl( i.wsCoord, _Octaves);
				float3 ws = mul(_Object2World, v.vertex).xyz;//i.wsCoord.xyz;
				//ws = ws + ws * normalize(clamp(normalize(i.theCurl), 0.2, 0.8))/_Octaves2;
				//ws = ws + ws * normalize(i.theCurl)/_Octaves2;
				
				
				float2 coord1 = ws.yz * 2;// * i.theCurl.yz;
				float2 coord2 = ws.zx * 2;// * i.theCurl.zx;  
				float2 coord3 = ws.xy * 2;// * i.theCurl.xy;
				
				//float4 col1 = tex2D(_ColorTex1, coord1);  //* 0.01 + float4(1.0,0.0,0.0,1.0); // uncomment to see the blending in red/green/blue only
				//float4 col2 = tex2D(_ColorTex2, coord2);   //* 0.01 + float4(0.0,1.0,0.0,1.0);
				//float4 col3 = tex2D(_ColorTex3, coord3);  //* 0.01 + float4(0.0,0.0,1.0,1.0);
				
				//float2 bumpFetch1 = tex2D(_BumpTex1, coord1).xy - 0.5;  
				//float2 bumpFetch2 = tex2D(_BumpTex2, coord2).xy - 0.5;  
				//float2 bumpFetch3 = tex2D(_BumpTex3, coord3).xy - 0.5;  
				
				//float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y);  
				//float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x);  
				//float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0); 
                
                float4 localTex1 = tex2D(_MainTex, coord1);
                float4 localTex2 = tex2D(_MainTex, coord2);
                float4 localTex3 = tex2D(_MainTex, coord3);

                //v.vertex.y += localTex.r * _Offset;
                v.vertex.x += localTex1.x * _Offset;
                v.vertex.y += localTex2.y * _Offset;
                v.vertex.z += localTex3.z * _Offset;

                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);

                return o;
            }

            

        #ifdef UNITY_CAN_COMPILE_TESSELLATION
            // tessellation domain shader
            [domain("tri")]
            v2f basicDomain (outputPatchConstant tessFactors, const OutputPatch<inputControlPoint,3> vi, float3 bary : SV_DomainLocation) {

                appdata_tan v;

                v.vertex = vi[0].position*bary.x + vi[1].position*bary.y + vi[2].position*bary.z;
				
				// <normal.z, normal.y, -normal.x> and <normal.y, -normal.x, normal.z>
				//float4 tang0 = float4(vi[0].normal.z, vi[0].normal.y, -vi[0].normal.x, 0);
				//float4 tang1 = float4(vi[1].normal.z, vi[1].normal.y, -vi[1].normal.x, 0);
				//float4 tang2 = float4(vi[2].normal.z, vi[2].normal.y, -vi[2].normal.x, 0);
                //v.tangent = tang0 *bary.x + tang1 *bary.y + tang2 *bary.z; 
                v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;

                v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;

                v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;

                v2f o = displace( v);

//                v2f o = vert_surf (v);

                return o;

            }
        #endif // UNITY_CAN_COMPILE_TESSELLATION


            float4 frag(in v2f IN):COLOR{
                //return tex2D (_MainTex, IN.texcoord);
                
                //fixed4 c = half4(IN.normal.x, IN.normal.y, IN.normal.z, 0);//_Color;
                //fixed4 c = half4(IN.tangent.x, IN.tangent.y, IN.tangent.z, 0);//_Color;
                //fixed4 c = IN.tangent;//_Color;
                fixed4 c = _Color;
                return c;
            }

            
            ENDCG
        }
    } 
}