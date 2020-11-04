// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

//http://forum.unity3d.com/threads/161541-basic-lambert-shader-does-not-work
//http://kylehalladay.com/all/blog/2013/10/13/Multi-Light-Diffuse.html
//https://stackoverflow.com/questions/21079675/what-are-passes-and-multiple-shader-passes-and-their-private-variables
//http://docs.unity3d.com/Documentation/Components/RenderTech-ForwardRendering.html
//http://docs.unity3d.com/Documentation/Components/SL-PassTags.html

//http://www.gamasutra.com/blogs/JoeyFladderak/20140416/215612/Let_there_be_shadow.php

//Point lights vert frag: http://unitygems.com/noobs-guide-shaders-5-bumped-diffuse-shader/

//http://forum.unity3d.com/threads/127162-Problems-with-porting-triplanar-shader

Shader "Custom/OutlinedDiffuseNormals" {
	Properties {
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_Contrast ("Lambert Contrast (2)", Float) = 2
		_BumpPower ("Bump Contrast (1)", Float) = 1
		_TexPower ("Texture Contrast (1)", Float) = 1
		_Octaves ("Curl Octaves (1)", Float) = 1
		_Octaves2 ("Curl Octaves (2)", Float) = 1
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_OutlineColor2 ("Outline Color 2", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.006)) = .005
		_OutlineTop ("OutlineTop width", Range (.002, 0.006)) = .005
		//_ColorTex4 ("Marble (RGB)", 2D) = "white" { }
		//_ColorTex5 ("Marble2 (RGB)", 2D) = "white" { }
		_Mar_scale ("Marble Texture Scale", Range (.05, 0.5)) = 0.5
		_Mar_Cont ("Marble Contrast", Float) = 1
		_Mar_Sat ("Marble Saturation", Float) = 1
		_Tex_scale ("Projected Texture Scale", Float) = 1
		_Bump_scale ("Bumpmap Texture Scale", Float) = 1
		_Seed ("Noise Seed", Vector) = (0,0,0,0)
		_ColorTex1 ("Base 1 (RGB)", 2D) = "white" { }
		_ColorTex2 ("Base 2 (RGB)", 2D) = "white" { }
		_ColorTex3 ("Base 3 (RGB)", 2D) = "white" { }
		_BumpTex1 ("Bump 1", 2D) = "bump" { }
		_BumpTex2 ("Bump 2", 2D) = "bump" { }
		_BumpTex3 ("Bump 3", 2D) = "bump" { }
		
		
		//_EdgeLength ("Edge length", Range(3,50)) = 10
		//_Smoothness ("Smoothness", Range(0,1)) = 0.5
		
	}
//#pragma target 5.0
//http://docs.unity3d.com/Documentation/Components/SL-ShaderPrograms.html
CGINCLUDE
//#include "UnityCG.cginc"
//#include "Lighting.cginc" //<--probably included in UnityCG.cginc

#include "UnityCG.cginc"
#include "AutoLight.cginc"

//#include "CurlNoise.cginc"

//#pragma fragmentoption ARB_precision_hint_fastest
//#pragma multi_compile LIGHTMAP_OFF
//#pragma exclude_renderers flash
#pragma only_renderers d3d11

	struct appdata {
		float4 vertex : POSITION;
		half3 normal : NORMAL;
		half4 tangent : TANGENT; //equivalent to "appdata_tan" in UnityCG.cginc
		float4 texcoord : TEXCOORD0;
		//fixed4 color : COLOR;
	};
	 
	struct v2fO {
		float4 pos : POSITION;
		fixed4 color : COLOR;
		//float fog : FOGC;
	};
	 
	
	
	fixed4 _LightColor0; // Colour of the light used in this pass.
	fixed4 _Color;
	
	//TODO: what is one and what is the other? do I need both if I don't use it in the vertex?
	uniform sampler2D _ColorTex1;
	uniform float4 _ColorTex1_ST;
	uniform sampler2D _ColorTex2;
	uniform float4 _ColorTex2_ST;
	uniform sampler2D _ColorTex3;
	uniform float4 _ColorTex3_ST;
	
	//uniform sampler2D _ColorTex4;
	//uniform float3 _ColorTex4_ST;
	//uniform sampler2D _ColorTex5;
	//uniform float3 _ColorTex5_ST;
	
	uniform sampler2D _BumpTex1;
	uniform float4 _BumpTex1_ST;
	uniform sampler2D _BumpTex2;
	uniform float4 _BumpTex2_ST;
	uniform sampler2D _BumpTex3;
	uniform float4 _BumpTex3_ST;
	
	 
	uniform float _Outline;
	uniform float _OutlineTop;
	uniform float4 _OutlineColor;
	uniform float4 _OutlineColor2;
	uniform float _Contrast;
	uniform float _BumpPower;
	uniform float _TexPower;
	uniform float _Octaves;
	uniform float _Octaves2;
	uniform float _Tex_scale;
	uniform float _Bump_scale;
	uniform float4 _Seed;
	uniform float _Mar_scale;
	uniform float _Mar_Cont;
	uniform float _Mar_Sat;
	 
	v2fO vertOutline(appdata v) {
		// just make a copy of incoming vertex data but scaled according to normal direction
		v2fO o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		
		float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);
		
		o.pos.xy += offset * o.pos.z * lerp(_Outline,_OutlineTop,-v.normal.y);
		
		//o.color = _OutlineColor * v.color;
		//float3 n = normalize(v.vertex);
		//o.color = _OutlineColor;// *  n.y;
		//float3 tn = float3(v.normal.z, v.normal.y, -v.normal.x);
		o.color = lerp(_OutlineColor,_OutlineColor2,-v.normal.y);
		//o.color = half4(v.normal * 0.5 + 0.5, 1);
		//o.color.r = 1.0-v.color.r;
		//o.color.g = 1.0-v.color.g;
		//o.color.b = 1.0-v.color.b;

		//o.fog = o.pos.z;


		return o;
	}
	

	#define NOISE_SIMPLEX_1_DIV_289 0.00346020761245674740484429065744f


	

	float mod289(float x) {
		return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
	}

	float2 mod289(float2 x) {
		return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
	}

	float3 mod289(float3 x) {
		return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
	}

	float4 mod289(float4 x) {
		return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
	}

	/*
	// ( x*34.0 + 1.0 )*x = 
	// x*x*34.0 + x
	float permute(float x) {
		return mod289(
			x*x*34.0 + x
		);
	}
	*/
	float3 permute(float3 x) {
		return mod289(
			x*x*34.0 + x
		);
	}

	float4 permute(float4 x) {
		return mod289(
			x*x*34.0 + x
		);
	}


	/*
	float taylorInvSqrt(float r) {
		return 1.79284291400159 - 0.85373472095314 * r;
	}
	*/
	float4 taylorInvSqrt(float4 r) {
		return 1.79284291400159 - 0.85373472095314 * r;
	}


	float snoise(float3 v)
	{
		const float2 C = float2(
			0.166666666666666667, // 1/6
			0.333333333333333333  // 1/3
		);
		const float4 D = float4(0.0, 0.5, 1.0, 2.0);
		
		//v+=seedOffset;
		//v.x += 234.2341;
		//v.y += -573.7534;
		//v.z += 321.154;
		
		
	// First corner
		float3 i = floor( v + dot(v, C.yyy) );
		float3 x0 = v - i + dot(i, C.xxx);
		
	// Other corners
		float3 g = step(x0.yzx, x0.xyz);
		float3 l = 1 - g;
		float3 i1 = min(g.xyz, l.zxy);
		float3 i2 = max(g.xyz, l.zxy);
		
		float3 x1 = x0 - i1 + C.xxx;
		float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
		float3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y
		
	// Permutations
		i = mod289(i);
		float4 p = permute(
			permute(
				permute(
						i.z + float4(0.0, i1.z, i2.z, 1.0 )
				) + i.y + float4(0.0, i1.y, i2.y, 1.0 )
			) 	+ i.x + float4(0.0, i1.x, i2.x, 1.0 )
		);
		
	// Gradients: 7x7 points over a square, mapped onto an octahedron.
	// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
		float n_ = 0.142857142857; // 1/7
		float3 ns = n_ * D.wyz - D.xzx;
		
		float4 j = p - 49.0 * floor(p * ns.z * ns.z); // mod(p,7*7)
		
		float4 x_ = floor(j * ns.z);
		float4 y_ = floor(j - 7.0 * x_ ); // mod(j,N)
		
		float4 x = x_ *ns.x + ns.yyyy;
		float4 y = y_ *ns.x + ns.yyyy;
		float4 h = 1.0 - abs(x) - abs(y);
		
		float4 b0 = float4( x.xy, y.xy );
		float4 b1 = float4( x.zw, y.zw );
		
		//float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
		//float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
		float4 s0 = floor(b0)*2.0 + 1.0;
		float4 s1 = floor(b1)*2.0 + 1.0;
		float4 sh = -step(h, 0.0);
		
		float4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
		float4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;
		
		float3 p0 = float3(a0.xy,h.x);
		float3 p1 = float3(a0.zw,h.y);
		float3 p2 = float3(a1.xy,h.z);
		float3 p3 = float3(a1.zw,h.w);
		
	//Normalise gradients
		float4 norm = taylorInvSqrt(float4(
			dot(p0, p0),
			dot(p1, p1),
			dot(p2, p2),
			dot(p3, p3)
		));
		p0 *= norm.x;
		p1 *= norm.y;
		p2 *= norm.z;
		p3 *= norm.w;
		
	// Mix final noise value
		float4 m = max(
			0.6 - float4(
				dot(x0, x0),
				dot(x1, x1),
				dot(x2, x2),
				dot(x3, x3)
			),
			0.0
		);
		m = m * m;
		
		return 42.0 * dot(
			m*m,
			float4(
				dot(p0, x0),
				dot(p1, x1),
				dot(p2, x2),
				dot(p3, x3)
			)
		);

	}
	 /// -------------- / noise

	float noise0(half3 myP)
	{
	    float output = snoise(myP);
	    return output;
	}

	float noise1(half3 myP)
	{
	    half3 newP = half3(myP.y + 31.416, myP.z - 47.853, myP.x + 12.793);
	    return snoise(newP);

	}

	float noise2(half3 myP)
	{
	    half3 newP = half3(myP.z - 233.145, myP.x - 113.408, myP.y + 185.31);
	    return snoise(newP);

	}


	half3 potential(float x, float y, float z)
	{
	    //float roughness = 0.75;   
	    half3 psi = 0;
	    int i;
	    //float valueMult = 1;//1;

	    half3 myP = half3(x,y,z);

	    //for (i = 0; i < floor(octaves); i++)
	    //{
	        //float sx = myP.x / 0.4;
	        //float sy = myP.y / 0.4;
	        //float sz = myP.z / 0.4;
	        float sx = myP.x / 0.4;
	        float sy = myP.y / 0.4;
	        float sz = myP.z / 0.4;
	        half3 newP = half3(sx,sy,sz);            
	        half3 tPsi = half3(noise0(newP), noise1(newP), noise2(newP));

	        psi += tPsi;//* valueMult;
	        //valueMult = valueMult * roughness;
	    //}
	    
	return psi;
	}


	//half3 get_velocity(half3 myP, float octaves)
	half3 get_Curl(half3 myP)
	{
		myP.x += _Seed.x;
		myP.y += _Seed.y;
		myP.z += _Seed.z;
		
		float deltaX = 2.5;//0.5//0.0001;

	    float v0 = ( potential(myP.x,myP.y + deltaX, myP.z).z - potential(myP.x, myP.y - deltaX, myP.z).z )
	                //- ( potential(myP.x, myP.y, myP.z + deltaX).y - potential(myP.x, myP.y, myP.z - deltaX).y ) / (2 * deltaX);
	                - ( potential(myP.x, myP.y, myP.z + deltaX).y - potential(myP.x, myP.y, myP.z - deltaX).y ) / ( deltaX);

	    float v1 = ( potential(myP.x, myP.y, myP.z + deltaX).x - potential(myP.x, myP.y, myP.z - deltaX).x )
	                //- ( potential(myP.x + deltaX, myP.y, myP.z).z - potential(myP.x - deltaX, myP.y, myP.z).z ) / (2 * deltaX);
	                - ( potential(myP.x + deltaX, myP.y, myP.z).z - potential(myP.x - deltaX, myP.y, myP.z).z ) / ( deltaX);

	    float v2 = ( potential(myP.x + deltaX, myP.y, myP.z).y - potential(myP.x - deltaX, myP.y, myP.z).y )
	                //- ( potential(myP.x, myP.y + deltaX, myP.z).x - potential(myP.x, myP.y - deltaX, myP.z).x ) / (2 * deltaX);
	                - ( potential(myP.x, myP.y + deltaX, myP.z).x - potential(myP.x, myP.y - deltaX, myP.z).x ) / ( deltaX);

	    return half3(v0,v1,v2);
	} 


	//TODO: make calculations efficient; like deltaXx2; also in the vert n frag
	// you should also remove this sort of fuckery: *0.8 +  col2.xyzw * blend_weights.yyyy *1.4
	
ENDCG
 
SubShader 
{
	//Tags {"Queue" = "Geometry-100" }
	
	//TODO: good idea for getting rid of the overlapping bright streaks
	//Tags { "Queue" = "Transparent"}// "IgnoreProjector"="True" "RenderType"="Transparent"}
	Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}// this reaveals the stitching bright streaks (1-voxel-edge z-fighting). but I offset the meshes by a tiny bit at generation time
	
	//Fog { Color [_AddFog] }
	//Fog { Mode Exp2 }
	
	
	Pass {
		Name "OUTLINE"
		Tags { "LightMode" = "Always" }
		Cull Front//Off//TODO: Cull Front
		ZWrite On//Off//ZWrite On
		//ZTest Always
		ZTest LEqual
		ColorMask RGB //alpha channel not used
		// you can choose what kind of blending mode you want for the outline
		Blend Off
		AlphaTest Off
		Cull Front
		//Blend SrcAlpha OneMinusSrcAlpha //normal 
		//Blend DstColor SrcColor //normal 
		//Blend One One // Additive
		//Blend One OneMinusDstColor // Soft Additive
		//Blend DstColor Zero // Multiplicative
		//Blend DstColor SrcColor // 2x Multiplicative
		//BlendOp Max
		//Blend one one
		//Offset 50,50
		Lighting Off
	 	//Fog { Mode Off }
	 
		CGPROGRAM
			#pragma vertex vertOutline
			#pragma fragment frag
			//#pragma fragmentoption ARB_fog_exp2
				
			half4 frag(v2fO i) :COLOR { 
				// THIS IS ONLY FOR THE OUTLINE
				//can blend outline here, or in vertf
				return i.color; 
			}
		ENDCG
	}
	
	Pass {
		Tags { "LightMode" = "ForwardBase" }// This Pass tag is important or Unity may not give it the correct light information.
		//It sure is! Without it, the light starts in the wrong direction and doesn't move.
		Name "BASE"
		ZWrite On //write to depth buffer, to occlude other objects
		ZTest LEqual
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend Off
		AlphaTest Off
		Cull Back
		//Blend SrcAlpha OneMinusSrcAlpha
		//ColorMask RGB //alpha channel not used
		//Lighting On
		//Fog { Mode Exp2 }
		
		
		CGPROGRAM
			#pragma vertex vertBase
			#pragma fragment fragBase
			//This is to ensure the shader compiles properly for the needed passes. As with the tag, for any additional lights in their own pass, fwdbase becomes fwdadd.
			#pragma multi_compile_fwdbase 
			//#pragma fragmentoption ARB_fog_exp2
			//#define V2F_POS_FOG float4 pos : POSITION; float fog : FOGC
			struct v2f {
				float4 pos : POSITION;
				half3 normal : TEXCOORD0;
				half3 lightDir: TEXCOORD1; 
				//fixed4 color : COLOR;
				LIGHTING_COORDS(2,3) // Macro to send shadow & attenuation to the vertex shader.
				//half3  vertexLighting : TEXCOORD5;
				//float fog : FOGC;
				half4 wsCoord : TEXCOORD4;
				half3 theCurl : TEXCOORD5;
				//float2 uv_ColorTex1 : TEXCOORD5;
				//float2 uv_ColorTex2 : TEXCOORD6;
				//float2 uv_ColorTex3 : TEXCOORD7;
				//float2 uv_BumpTex1 : TEXCOORD8;
				//float2 uv_BumpTex2 : TEXCOORD9;
				//float2 uv_BumpTex3 : TEXCOORD10;
				//uniform sampler2D colorTex1 : TEXUNIT0
				//uniform sampler2D colorTex2 : TEXUNIT1
				//uniform sampler2D colorTex3 : TEXUNIT2
				//uniform sampler2D bumpTex1 : TEXUNIT3
				//uniform sampler2D bumpTex2 : TEXUNIT4
				//uniform sampler2D bumpTex3 : TEXUNIT5
				
			}; 
			
			v2f vertBase(appdata v) {

				v2f o;
				//PositionFog( v.vertex, o.pos, o.fog );
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);//we do a transformation here.verex position gets transform into world space. It's actually ModelViewProjection space
				//o.fog = o.pos.z;
				o.wsCoord = mul(_Object2World, v.vertex);//worldSpace coord for the tex projection in the fragment. http://forum.unity3d.com/threads/81741-How-to-get-worldPos-in-fragment-shader
				 
				o.theCurl = get_Curl( o.wsCoord.xyz/_Octaves);
				
				//o.uv_ColorTex1 = TRANSFORM_TEX(v.texcoord, _ColorTex1);
				//o.uv_ColorTex1 = v.texcoord.xy;
				//o.uv_BumpTex1 = TRANSFORM_TEX (v.texcoord, _BumpTex1);
				
				
				
				//o.lightDir = normalize(WorldSpaceLightDir( v.vertex ));
				//o.normal = normalize( mul( _Object2World, float4(v.normal, 0) ) ); //similar transformation but this does on normal .Wait a min,why this ".xyz" thing?
				
				
				if (0.0 == _WorldSpaceLightPos0.w) // directional light?
	            {
	               //o.lightDir = normalize(float3(_WorldSpaceLightPos0));
	               o.lightDir = ObjSpaceLightDir(v.vertex);
	            } 
	            else // point or spot light
	            {
	               o.lightDir = normalize(_WorldSpaceLightPos0.xyz - mul(_Object2World, v.vertex).xyz);
	            }
				
				//o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normal = v.normal;
				
						
				//In the vertex program we use the TANGENT_SPACE_ROTATION macro to create our rotation matrix to convert object space to tangent space.
				//For this macro to work our input structure must be called v and it must contain a normal called normal and a tangent called tangent.
				//TANGENT_SPACE_ROTATION; // but, Why?
				//We then calculate the direction in object space to the light we are dealing with (at the moment the most important light) using the built in function ObjSpaceLightDir(v.vertex).  We want the light direction in object space because we have a transformation from that to tangent space - which we immediately apply by multiplying our new rotation matrix by the direction
				//o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
				//HERE's Why:
				//In the fragment function we are going to unpack the normal from it's encoded format in the texture map and use that, on its own, as the normal for our Lambert function.  That's because all that tangent space rotation on the light direction has already got it to take account of the normal of the face of the model we are rendering.
				//because we're making a bumped shader. we want our striations to pop.
				//TODO: see if you want to use a texture for bump, or if you want to use pure curl noise as bump, in which case you don't need to sample it from a tex here. but maybe you'd want to calculate it here
				
				
				TRANSFER_VERTEX_TO_FRAGMENT(o); // Macro to send shadow & attenuation to the fragment shader.
				
				//o.color = _Color;
				
				
				return o; 
			}
			
			// THIS IS FOR THE ACTUAL MESH
				//TODO: implement gems pixel shader here
				//TODO: Do you believe in magic? o.color = half4(v.normal * 0.5 + 0.5, 1);
				//TODO: make up your mind about half3 fixed3 or float3, 
				//gems: http://http.developer.nvidia.com/GPUGems3/gpugems3_ch01.html
			half4 fragBase(v2f i) :COLOR { 
				//i.lightDir = normalize(i.lightDir);
				float3 n = i.normal;// UnpackNormal(tex2D (_BumpTex1, i.uv_BumpTex1)); 
				float3 ntheCurl = normalize(i.theCurl);
				
				float3 blend_weights = abs(n);
				// Tighten up the blending zone:
				blend_weights = ( blend_weights -0.05) ;//*7 0.2  //the - is the transition zone width; *x doesn't seem to do anything
				blend_weights = max(blend_weights, 0);
				// Force weights to sum to 1.0 (very important!)  
				blend_weights /= ((blend_weights.x + blend_weights.y + blend_weights.z) * _TexPower ).xxx; 
				
				
				
				// Now determine a color value and bump vector for each of the 3  
				// projections, blend them, and store blended results in these two  
				// vectors:  color and bump
				float3 blended_color; // .w hold spec value 
				float3 blended_bump_vec;  
				
				//float3 getCurl = get_Curl( i.wsCoord, _Octaves);
				float3 ws = i.wsCoord.xyz;
				//ws = ws + ws * normalize(clamp(normalize(i.theCurl), 0.2, 0.8))/_Octaves2;
				
				ws = ws + ws * ntheCurl/_Octaves2;
				
		
				
				// Compute the UV coords for each of the 3 planar projections.  
				// _Tex_scale (default ~ 1.0) determines how big the textures appear. 
				//je suppose que v2f.wsCoord signifie : vertex-2-fragment : world space coordinate 
				float2 coord1 = ws.yz * _Tex_scale;// * i.theCurl.yz;
				float2 coord2 = i.wsCoord.zx * _Tex_scale;// * i.theCurl.zx;  
				float2 coord3 = ws.xy * _Tex_scale;// * i.theCurl.xy;
				// This is where you would apply conditional displacement mapping.  
				//if (blend_weights.x > 0) coord1 = . . .  
				//if (blend_weights.y > 0) coord2 = . . .  
				//if (blend_weights.z > 0) coord3 = . . .   
				// Sample color maps for each projection, at those UV coords.  
				float4 col1 = tex2D(_ColorTex1, coord1);  //* 0.01 + float4(1.0,0.0,0.0,1.0); // uncomment to see the blending in red/green/blue only
				float4 col2 = tex2D(_ColorTex2, coord2);   //* 0.01 + float4(0.0,1.0,0.0,1.0);
				float4 col3 = tex2D(_ColorTex3, coord3);  //* 0.01 + float4(0.0,0.0,1.0,1.0);
				
				float3 mar1 = tex2D(_ColorTex1,coord1 *_Mar_scale);//*_Mar_Cont;  
				float3 mar3 = tex2D(_ColorTex3,coord3 *_Mar_scale);//*_Mar_Cont;
				//float3 col4 = tex2D(_ColorTex4, float2(ntheCurl.x, ntheCurl.y));
				//float3 blended_marble = (mar1.xyz * blend_weights.xxx *0.8 + 
				//                		 mar3.xyz * blend_weights.zzz * 0.8)*_Mar_Cont; 
				float3 blended_marble = (mar1.xyz * blend_weights.xxx + 
				                		 mar3.xyz * blend_weights.zzz)*_Mar_Cont; 
				
				
				// Sample bump maps too, and generate bump vectors.  
				// (Note: this uses an oversimplified tangent basis.)  
				float2 bumpFetch1 = tex2D(_BumpTex1, coord1*_Bump_scale).xy - 0.5;  
				float2 bumpFetch2 = tex2D(_BumpTex2, coord2*_Bump_scale).xy - 0.5;  
				float2 bumpFetch3 = tex2D(_BumpTex3, coord3*_Bump_scale).xy - 0.5;  
				//Tangent and Binormal vectors are vectors that are perpendicular to each other and the normal vector which essentially describe the direction of the u,v texture coordinates with respect to the surface that you are trying to render. Typically they can be used alongside normal maps which allow you to create sub surface lighting detail to your model(bumpiness).
				//this is what makes cross textures, why you need a flipped tecture. it's straight not curved and seamless.
				float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y);  //TODO: better tangents.
				float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x);  
				float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0); 
				//this is how you calculate the tangent AND BITANGENT for x:
				// <normal.z, normal.y, -normal.x> and <normal.y, -normal.x, normal.z>
				 
				// Finally, blend the results of the 3 planar projections.  
				//if(blend_weights.y > 0.8)
				//blended_color = col1.xyzw * blend_weights.xxxx +  
				//                col2.xyzw * blend_weights.yyyy +  
				//                col3.xyzw * blend_weights.zzzz; 
				//else
				//TODO:
				blend_weights.y = max((blend_weights.y -(0.5/blend_weights.y) + 0.625),0);
				blended_color = (col1.xyz * blend_weights.xxx +
				                col3.xyz * blend_weights.zzz) * 0.8
				                +  col2.xyz * blend_weights.yyy *1.3;  
				blended_bump_vec = bump1.xyz * blend_weights.xxx +  
				                   bump2.xyz * blend_weights.yyy +  
				                   bump3.xyz * blend_weights.zzz;  
				                   
				// Apply bump vector to vertex-interpolated normal vector.  
				n = normalize(n + (blended_bump_vec ) * _BumpPower);
				//n = normalize(n + (blended_bump_vec + blended_marble*0.65) * _BumpPower);
				
				//We first start with the base colour of the ambient light.
				//float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.rgb;
				
				//We work out how far away our real light is.  If this was a directional light then the vector is already normalized so the distance will be 1 (no affect).  Then we work out the final attenuation of the light by multiplying out the distance from the light squared (dot(v,v) is the square of v's magnitude helpfully) by the intensity of the light (represented in unity_LightAtten)
				//For a directional light we are multiplying out by 1/1 + attenuation - in other words we are dividing the underlying colour (and hence brightness) by the attenuation + 1. This is why we multiply the final colour by 2.
				//For a point light we are also making its brightness fall off in relation to the square of the distance to the light.
				//float lengthSq = dot(i.lightDir, i.lightDir);
				//fixed atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[0].z * lengthSq);//LIGHT_ATTENUATION(i); //What's this? // Macro to get you the combined shadow & attenuation value.
				//fixed atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0[0].z * lengthSq);//LIGHT_ATTENUATION(i); //What's this? // Macro to get you the combined shadow & attenuation value.
				fixed atten = LIGHT_ATTENUATION(i); //What's this? // Macro to get you the combined shadow & attenuation value.
				//fixed atten = 1.0 / (lengthSq * lengthSq); //What's this? // Macro to get you the combined shadow & attenuation value.
			
				fixed4 c;// = fixed4(0, 0, 0, 0);// = tex2D(_ColorTex1, i.uv_ColorTex1);// * _ColorTex1_ST.xy + _ColorTex1_ST.zw);//TODO: WHY THESE <--- check tut
				//c.rgb = blended_color;// / _TexPower;
				//TODO: note: the blended marble should also be added to the forwardAdd
				//Overlay photoshop
				c.rgb = _Color.rgb * 1-_Mar_Sat*(1-blended_color)*(1-blended_marble);// / _TexPower;
				//c.rgb += blended_marble*_Mar_Cont;
				c.a = _Color.a;

				
				//float3 col4 = tex2D(_ColorTex4, float2(ntheCurl.x, ntheCurl.y));//ws.yz*ws.zx*ws.xy);///coord2);
				//col4 = lerp(c.rgb, col4, col4.r*col4.g*col4.b);
				
				//Angle to the light. the "diff"		
				float3 lightDir = normalize(i.lightDir);
				fixed lambert = saturate(dot(n, lightDir));
				//fixed noBump = saturate(dot(i.normal, lightDir));
				//now we can apply our light's color to that^ (dot angle) in combination with the attenuation.		
				
				
				//c.rgb = UNITY_LIGHTMODEL_AMBIENT.rgb * 2 * c.rgb;  // Ambient term. Only do this in Forward Base. It only needs calculating once.
				//c.rgb += (c.rgb * _LightColor0.rgb * lambert) * (atten * 2); // Diffuse and specular.
				//lightColor += (_LightColor0.rgb + fixed4(i.vertexLighting, 1.0)) * (lambert * atten);
				//lightColor += _LightColor0.rgb * (lambert * atten);
		        //c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * c.rgb *2) + (c.rgb * _LightColor0.rgb * lambert) * (atten *2 );
		        
		        //c.rgb = ((UNITY_LIGHTMODEL_AMBIENT.rgb * c.rgb ) + (c.rgb * _LightColor0.rgb * lambert) * atten )*_Contrast
		        

		        
		        c.rgb = (
		        		 (UNITY_LIGHTMODEL_AMBIENT.rgb *(c.rgb +blended_marble*_Mar_Cont)) //c.rgb +blended_marble
		        		 + (c.rgb * _LightColor0.rgb * lambert ) * atten  
		        		 //+ noBump * blended_marble /_Mar_Cont
		        		)*_Contrast
		        		//* (noBump * blended_marble + blended_marble)*_Mar_Cont
		        		;//+blended_marble*_Mar_Cont;//+col4/2;
		        
		        
		        //if(i.theCurl.y > 0.98){// && n.y < 0.85){ //0.8 == 1.25
		  
			    if(n.y > 0.0001 && n.y < 0.15 && i.theCurl.y > 0.97){
			       	c.rgb *= (i.theCurl.y*1.02)/(n.y*10);
			    }

				
				
				//c.a = _Color.a + _LightColor0.a * atten;
		
				/*
				float3 lambert = saturate( dot(i.lightDir, i.normal ) );
				fixed4 c;// = tex2D (_ColorTex1, i.uv_ColorTex1);
				c.rgb = lambert * i.color;//_Color//TODO: don't think I need to send the color from vertex program. Unless vertex rainbow
				c.a = i.color.a; 
				*/
				return c; 
			}
		ENDCG
		
	}
	
	Pass {
		Tags {"LightMode" = "ForwardAdd"} // Again, this pass tag is important otherwise Unity may not give the correct light information.
    	Blend One One// Additively blend this pass with the previous one(s). This pass gets run once per pixel light.
		//Blend One OneMinusDstColor // Soft Additive
		//Blend SrcAlpha OneMinusSrcAlpha
		Name "ADD"
		//ZWrite On
		ZTest LEqual
		ZWrite On
		//Blend Off
		AlphaTest Off
		Cull Back 
		Fog { Color (0,0,0,0) }
		//Fog { Color (0.2, 0.2 , 0.2 ,0) }//this makes a light box :(

		CGPROGRAM
			#pragma vertex vertBaseAdd
			#pragma fragment fragBaseAdd
			// This line tells Unity to compile this pass for forward add, giving attenuation information for the light.
			#pragma multi_compile_fwdadd   
			//#pragma fragmentoption ARB_fog_exp2
			//#define V2F_POS_FOG float4 pos : POSITION; float fog : FOGC
			struct v2f {
				float4 pos : POSITION;
				half3 normal : TEXCOORD0;
				half3 lightDir: TEXCOORD1; 
				//float2 uv_ColorTex1 : TEXCOORD2;
				//fixed4 color : COLOR;
				LIGHTING_COORDS(2,3) // Macro to send shadow & attenuation to the vertex shader.
				//half3  vertexLighting : TEXCOORD5;
				//float2 uv_BumpTex1 : TEXCOORD6;
				//float fog : FOGC;
				half4 wsCoord : TEXCOORD4;
				half3 theCurl : TEXCOORD5;
			}; 
			      
			v2f vertBaseAdd(appdata v) {
				v2f o;
				//PositionFog( v.vertex, o.pos, o.fog );
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);//we do a transformation here.verex position gets transform into world space
				o.wsCoord = mul(_Object2World, v.vertex);
				
				o.theCurl = get_Curl( o.wsCoord.xyz/_Octaves);
				
				//o.fog = o.pos.z;
				//o.uv_ColorTex1 = v.texcoord.xy;
				//o.uv_BumpTex1 = TRANSFORM_TEX (v.texcoord, _BumpTex1);

				

				//TANGENT_SPACE_ROTATION; 
				//o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
				//o.lightDir = ObjSpaceLightDir(v.vertex);
				
				if (0.0 == _WorldSpaceLightPos0.w) // directional light?
	            {
	               o.lightDir = ObjSpaceLightDir(v.vertex);
	            } 
	            else{ // point or spot light
	               o.lightDir = normalize(_WorldSpaceLightPos0.xyz - mul(_Object2World, v.vertex).xyz);
	               //o.lightDir = _WorldSpaceLightPos0.xyz - mul(_Object2World, v.vertex);

	            }
				
				o.normal = v.normal;
				
				TRANSFER_VERTEX_TO_FRAGMENT(o); // Macro to send shadow & attenuation to the fragment shader.
				
				//o.vertexLighting = float3(0.0, 0.0, 0.0);
				
				return o;
			}
			
			half4 fragBaseAdd(v2f i) :COLOR { 
				float3 n = i.normal;// UnpackNormal(tex2D (_BumpTex1, i.uv_BumpTex1)); 
				
				float3 blend_weights = abs(n);
				// Tighten up the blending zone:
				blend_weights = ( blend_weights - 0.05) ;//*0.1;//*7 0.2  
				blend_weights = max(blend_weights, 0);
				// Force weights to sum to 1.0 (very important!)  
				blend_weights /= ((blend_weights.x + blend_weights.y + blend_weights.z) * _TexPower ).xxx ; 
				
				// Now determine a color value and bump vector for each of the 3  
				// projections, blend them, and store blended results in these two  
				// vectors:  color and bump
				float3 blended_color; // .w hold spec value 
				float3 blended_bump_vec;  
				
				//float3 getCurl = get_Curl( i.wsCoord, _Octaves);
				float3 ws = i.wsCoord.xyz;
				//ws = ws + ws * normalize(clamp(normalize(i.theCurl), 0.2, 0.8))/_Octaves2;
				ws = ws + ws * normalize(i.theCurl)/_Octaves2;
				
				// Compute the UV coords for each of the 3 planar projections.  
				// _Tex_scale (default ~ 1.0) determines how big the textures appear. 
				//je suppose que v2f.wsCoord signifie : vertex-2-fragment : world space coordinate 
				float2 coord1 = ws.yz * _Tex_scale;// * i.theCurl.yz;
				float2 coord2 = ws.zx * _Tex_scale;// * i.theCurl.zx;  
				float2 coord3 = ws.xy * _Tex_scale;// * i.theCurl.xy;
				// This is where you would apply conditional displacement mapping.  
				//if (blend_weights.x > 0) coord1 = . . .  
				//if (blend_weights.y > 0) coord2 = . . .  
				//if (blend_weights.z > 0) coord3 = . . .   
				// Sample color maps for each projection, at those UV coords.  
				float4 col1 = tex2D(_ColorTex1, coord1);  //* 0.01 + float4(1.0,0.0,0.0,1.0); // uncomment to see the blending in red/green/blue only
				float4 col2 = tex2D(_ColorTex2, coord2);   //* 0.01 + float4(0.0,1.0,0.0,1.0);
				float4 col3 = tex2D(_ColorTex3, coord3);  //* 0.01 + float4(0.0,0.0,1.0,1.0);
				
				// Sample bump maps too, and generate bump vectors.  
				// (Note: this uses an oversimplified tangent basis.)  
				float2 bumpFetch1 = tex2D(_BumpTex1, coord1*_Bump_scale).xy - 0.5;  
				float2 bumpFetch2 = tex2D(_BumpTex2, coord2*_Bump_scale).xy - 0.5;  
				float2 bumpFetch3 = tex2D(_BumpTex3, coord3*_Bump_scale).xy - 0.5;   
				float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y);  //TODO: better tangents. http://unitygems.com/noobs-guide-shaders-5-bumped-diffuse-shader/
				float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x);  
				float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);  
				//TODO: see if you can copy one side to the other (x and z) without 2 calculations
				 
				// Finally, blend the results of the 3 planar projections.  
				//if(blend_weights.y > 0.8)
				//blended_color = col1.xyzw * blend_weights.xxxx +  
				//                col2.xyzw * blend_weights.yyyy +  
				//                col3.xyzw * blend_weights.zzzz;  
				//else
				blend_weights.y = max((blend_weights.y -(0.5/blend_weights.y) + 0.625),0);//-0.5/b/0.625 //0.714
 				blended_color = (col1.xyz * blend_weights.xxx +
				                col3.xyz * blend_weights.zzz) * 0.8
				                +  col2.xyz * blend_weights.yyy *1.3;  
				blended_bump_vec = bump1.xyz * blend_weights.xxx +  
				                   bump2.xyz * blend_weights.yyy +  
				                   bump3.xyz * blend_weights.zzz;  
				                   
				// Apply bump vector to vertex-interpolated normal vector.  
				n = normalize(n + blended_bump_vec * _BumpPower);
				
				//float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				//float lengthSq = dot(i.lightDir, i.lightDir);
				// Macro to get you the combined shadow & attenuation value.
				fixed atten = LIGHT_ATTENUATION(i);//1.0 / (1.0 + lengthSq * unity_LightAtten[0].z);
				//fixed atten = 1.0 / (1.0 + lengthSq * unity_4LightAtten0[0].z);
					
				fixed4 c;// = fixed4(0, 0, 0, 0);//tex2D(_ColorTex1, i.uv_ColorTex1);
				c.rgb = blended_color*_Color;// / _TexPower;
				c.a = _Color.a;// + fixed4(i.vertexLighting, 1.0);	
				//for correct lambert, don't comment the *= _Color above. I left it this way because it looks bitchin with 2 lights
			
				fixed lambert = saturate(dot(n, normalize(i.lightDir)));
				
				

				//float3 lightColor = _LightColor0.rgb * (lambert * atten);
		        //c.rgb = lightColor * c.rgb * 2;
		        
		        c.rgb = (c.rgb * _LightColor0.rgb * lambert) * (atten * _Contrast );
				c.a = _Color.a + _LightColor0.a * atten;
				
				
				return c;  
			}                                    
		ENDCG
		
	}
}

 				
 
	Fallback "Diffuse"
}