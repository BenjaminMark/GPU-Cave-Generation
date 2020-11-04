Shader "Custom/particleCubeBak" 
{
	// Bound with the inspector.
 	Properties 
 	{
        _Color ("Main Color", Color) = (0, 1, 1,0.3)
        _SpeedColor ("Speed Color", Color) = (1, 0, 0, 0.3)
        _colorSwitch ("Switch", Range (0, 120)) = 60
        _LColor ("L Color", Color) = (0, 0, 1, 1)
        _NoiseColor0 ("Noise Color 0", Color) = (0, 0, 0, 0)
        _NoiseColor1 ("Noise Color 1", Color) = (1, 1, 1, 0)
        //_generatedCubeSize ("Generated Cube Size", Range (0.5, 4)) = 1
        _generatedLCubeSize ("Generated L Cube Size [0.5,4]", float) = 1
        _generatedNoiseCubeSize ("Generated Noise Cube Size [0.5,4]", float) = 1
        _colorFade ("Fadeout", Range (0, 1)) = 0
    }

	SubShader 
	{
		Pass 
		{
			//Tags { "Queue"="Transparent+1"}
			//Blend SrcAlpha OneMinusSrcColor
			//Blend SrcAlpha DstAlpha
			//Blend SrcAlpha SrcColor
			//Blend SrcAlpha one
			
			//BlendOp Max
			//Blend SrcAlpha one
			
			BlendOp Max
			Blend one one
			
			
			ZTest Always 
			
			CGPROGRAM
			#pragma target 5.0
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			// The same particle data structure used by both the compute shader and the shader.
			struct VoxelParticle
			{
				float3 position;
				float3 velocity;
				float noise;
				float prevNoise;
				int flags;
				//float3 cubeVerts[8];
			};
			
			// structure linking the data between the vertex and the fragment shader
			struct FragInput
			{
				float4 color : COLOR;
				float4 position : SV_POSITION;
			};
			
			// The buffer holding the particles shared with the compute shader.
			StructuredBuffer<VoxelParticle> particleBuffer;
			StructuredBuffer<float3> cubed_verts;
			
			// Variables from the properties.
			float4 _Color;
			float4 _SpeedColor;
			float _colorSwitch;
			float4 _LColor;
			float4 _NoiseColor0;
			float4 _NoiseColor1;
			float _generatedLCubeSize;
			float _generatedNoiseCubeSize;
			float _colorFade;
			
			// DX11 vertex shader these 2 parameters come from the draw call: "1" and "particleCount", 
			// SV_VertexID: "1" is the number of vertex to draw peer particle, we could easily make quad or sphere particles with this.
			// SV_InstanceID: "particleCount", number of particles...
			FragInput vert (uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				FragInput fragInput;
				
				// color computation
				
				if(particleBuffer[inst].flags == -20){
					float speed = length(particleBuffer[inst].velocity);
					float lerpValue = clamp(speed / _colorSwitch, 0, 1);
					fragInput.color = lerp(_Color, _SpeedColor, lerpValue);// + cubed_verts[id];
					
					// position computation
					fragInput.position = mul (UNITY_MATRIX_MVP, float4(	particleBuffer[inst].position +
																	cubed_verts[id],
																	1));
				}
				else if(particleBuffer[inst].flags == -10){
					fragInput.color = _LColor;
					
					// position computation
					fragInput.position = mul (UNITY_MATRIX_MVP, float4(	particleBuffer[inst].position +
																	cubed_verts[id]*_generatedLCubeSize,
																	1));
				}
				else{//else means it is actual noise, between -1 and 1
					//float4 cc = _NoiseColor + particleBuffer[inst].noise - _NoiseColor.a;
					float4 cc = lerp(_NoiseColor0, _NoiseColor1, (particleBuffer[inst].noise+1)/2) - _NoiseColor0.a;
					
					
					/*
					if(particleBuffer[inst].noise > 0.9){
						cc.r = 1;
						cc.g = 1;
						cc.b = 1;
					}
					else{
						cc.r = 0;
						cc.g = 0;
						cc.b = 0;
					}
					cc -= _NoiseColor0.a;
					*/
					
					
					
					fragInput.color = cc;
					
					
					// position computation
					fragInput.position = mul (UNITY_MATRIX_MVP, float4(	particleBuffer[inst].position +
																	cubed_verts[id]*_generatedNoiseCubeSize,
																	1));
				}
				
				
				
				return fragInput;
			}
			
			// this just pass through the color computed in the vertex program
			float4 frag (FragInput fragInput) : COLOR
			{
				return fragInput.color - _colorFade;
			}
			
			ENDCG
		
		}
	}

	Fallback Off
}
