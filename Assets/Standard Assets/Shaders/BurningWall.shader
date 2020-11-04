//  www.slindev.com 
Shader "Custom/Transparent Cutout/BurningWall"
{
	
    Properties

    {

       // _Color("Dissolve Color (RGB), Range (A)", Color) = (1.0, 1.0, 1.0, 1.0)

        //_FireRim("Fire Rim Thickness", Range(0, 0.5)) = 0.25
        _FireRim("Fire Rim Thickness", float) = 0.25
        //_Cutoff("Dissolve state", Range(-0.35, 1.1)) = 0.5
        _Cutoff("Dissolve state", Range(0.05, 0.95)) = 0.5
        //_Power("Fire Power", Range(0, 4)) = 0.5
        //_Power("Fire Power", float) = 0.5

        _MainTex("Base (RGB), Dissolve Mask (A)", 2D) = "white" {}
		_FireTex ("Fire (RGB)", 2D) = "white" {}
      	//_RimTex ("Fire Rim (RGB)", 2D) = "white" {}
      	_BurntTex ("Burnt Texture (RGB)", 2D) = "white" {}
      	_Mask ("Culling Mask", 2D) = "white" {}
    }

    

  



    

    //CG/HLSL shader used on all other devices with shader support

    SubShader

    {
    	

        Tags {"IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		//Tags {"Queue" = "Geometry+500"}
		
		
		
        Pass
        {
			
            //Tags {"LightMode" = "Always" }
			Lighting Off
            //Cull Off
			

            CGPROGRAM
                #pragma vertex vert

                #pragma fragment frag

                //#pragma fragmentoption ARB_precision_hint_fastest

                #include "UnityCG.cginc" 
				//#pragma exclude_renderers gles
                //uniform float4 _Color;
                uniform float4 _MainTex_ST; // Needed for TRANSFORM_TEX(v.texcoord, _MainTex)

                uniform float _FireRim;
                uniform float _Cutoff;
                //uniform float _Power;
 
                uniform sampler2D _MainTex;
                uniform sampler2D _Mask;
				uniform sampler2D _BurntTex;
                uniform sampler2D _FireTex;

				uniform sampler2D unity_Lightmap;
    			uniform float4 unity_LightmapST;

                struct appdata_base2

                {

                    float4 vertex : POSITION;

                    float4 texcoord : TEXCOORD0;
                    
                    float4 texcoord2 : TEXCOORD1;

					//float2 texcoord3 : Color; 

                }; 

                

                struct v2f

                {

                    float4 pos : POSITION;

                    float2 texcoord : TEXCOORD0;
                    
                    float2 texcoord2 : TEXCOORD1;

					float2 texcoord3 : Color; 
                };

 

                v2f vert(appdata_base2 v)

                {

                    v2f o;

                    

                    o.pos = mul(UNITY_MATRIX_MVP, v.vertex); 

					o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex) ;//http://shattereddeveloper.blogspot.dk/2011/09/handling-texture-tiling-and-offset-in.html
					
                    o.texcoord2.xy = v.texcoord.xy;
                    o.texcoord3.xy = v.texcoord2.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    
                    
                    //o.texcoord3.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    //o.texcoord.xy = v.texcoord.xy;// * unity_LightmapST.xy + unity_LightmapST.zw;
                    
                    //o.texcoord2.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    //o.texcoord.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    //o.texcoord.xy = v.texcoord.xy * unity_LightmapST.xy;// + unity_LightmapST.zw;
                    //o.texcoord.xy = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    
                    //o.texcoord2.xy = v.texcoord2.xy * unity_LightmapST.xy + unity_LightmapST.zw;

                    

                    return o;

                }

                // Decodes lightmaps:
				// - doubleLDR encoded on GLES
				// - RGBM encoded with range [0;8] on other platforms using surface shaders
				//inline fixed3 DecodeLightmap2(fixed4 color) {
				//    #if defined(SHADER_API_GLES) && defined(SHADER_API_MOBILE)
				//        return 2.0 * color.rgb;
				        //return color.rgb + color.rgb;
				//    #else
				//        return (8.0 * color.a) * color.rgb;
				//    #endif
				//}

                float4 frag(v2f i) : COLOR

                { 

                    half4 Color = tex2D(_MainTex, i.texcoord);
                    half4 BurntTex = tex2D(_BurntTex, i.texcoord);
                    half4 Mask = tex2D(_Mask, i.texcoord2);
                    half4 FireTex = tex2D(_FireTex, i.texcoord2);
                    //half4 Mask = tex2D(_Mask, i.texcoord2);
                    //half4 FireTex = tex2D(_FireTex, i.texcoord2);
					
					half4 lightmap = tex2D(unity_Lightmap, i.texcoord3);

                    
					if(Mask.a < _Cutoff)
					{
                        Color.rgb = BurntTex.rgb;
                        //Color.rgb = DecodeLightmap(lightmap) * BurntTex.rgb;
					}
					else
					if(Mask.a < _Cutoff + _FireRim)
                    {
                    	//float sub = Mask.a - _Cutoff;
                        //Color.rgb *= FireTex.rgb * 1/(sub + sub + sub + sub);//*3.2);//FireTex;//_Color.rgb;
                        Color.rgb *= FireTex.rgb * 1/((Mask.a - _Cutoff)*4);//*3.2);//FireTex;//_Color.rgb;
                    }
                    else
                    {
                    	//Color.rgb *= DecodeLightmap(tex2D(lightmap, i.texcoord2));// * _Power;
                    	
                    	Color.rgb *= DecodeLightmap(lightmap);
                    	
                    	//Color.rgb *= 2.0*lightmap;
                    	//Color.rgb *= 2.0*lightmap;
                    }

					//Color.rgb *= DecodeLightmap(lightmap);
					//Color.rgb *= DecodeLightmap(lightmap);
					
					//Color.rgb *= DecodeLightmap(tex2D(unity_Lightmap, i.texcoord.xy));
					
					
					
					//Color.rgb *= (DecodeLightmap(tex2D(unity_Lightmap, i.uv[1]))) * _Power;
                    //clip(Color.a-_Cutoff);
					
					
					
                    return Color;

                }

            ENDCG

        }
        
      
        
        //Pass
		//{
			//SetTexture [unity_Lightmap] {
            //            matrix [unity_LightmapMatrix]
            //}
            //SetTexture [_MainTex] {
            //            combine texture * previous Double//, texture * primary
            //}
			//SetTexture[unity_Lightmap] {Matrix[unity_LightmapMatrix] Combine texture * _Color Double}
			
		//}

    }

    


}