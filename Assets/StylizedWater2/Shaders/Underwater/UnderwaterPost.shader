//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

Shader "Hidden/StylizedWater2/UnderwaterPost"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Underwater Post Processing"

			HLSLPROGRAM
			
			#pragma vertex Vert
			#pragma fragment Fragment

			#pragma multi_compile_local _ BLUR
			#pragma multi_compile_local _ _SCREENSPACE_DISTORTION _CAMERASPACE_DISTORTION
			#pragma multi_compile _ _USE_DRAW_PROCEDURAL //Unity 2020.3 - 2021.2

			#include "../Libraries/URP.hlsl"
			#include "UnderwaterEffects.hlsl"
			#include "UnderwaterShading.hlsl"
			#include "UnderwaterFog.hlsl"

			#if VERSION_GREATER_EQUAL(12,0)
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			#elif VERSION_LOWER(10,0)
			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
			#else
			#include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
			#endif

			#ifdef UNITY_CORE_BLIT_INCLUDED
			#define UV input.texcoord.xy
			#else
			#define UV input.uv.xy
			#endif

			TEXTURE2D_X(_SourceTex); SAMPLER(sampler_SourceTex); float4 _SourceTex_TexelSize;

			#define BLUR_RADIUS 2.0

			#define GAUSSIAN_KERNEL_SIZE 8
			static const float GaussianWeights[GAUSSIAN_KERNEL_SIZE] = { 0.40, 0.15, 0.15, 0.10, 0.10, 0.05, 0.05, 0.025 };
			static const float2 GaussianKernel[GAUSSIAN_KERNEL_SIZE] = {				
				//Cross
				float2(-1,0),
				float2(1,0),
				float2(0,1),
				float2(0,-1),

				//Diagonal
				float2(-1,-1),
				float2(1,1),
				float2(-1,1),
				float2(1,-1)
			};
			
			float3 GaussianBlur(TEXTURE2D_X_PARAM(textureName, samplerTex), float2 uv)
			{
				const float2 radius = BLUR_RADIUS / _ScreenParams.xy;
		
				float3 color = SAMPLE_TEXTURE2D_X(textureName, samplerTex, uv.xy).rgb;

				UNITY_UNROLL
 				for(uint k = 0; k < GAUSSIAN_KERNEL_SIZE; k++)
				{
					color += SAMPLE_TEXTURE2D_X(textureName, samplerTex, uv.xy + (GaussianKernel[k].xy * radius.xy)).rgb * GaussianWeights[k];
				}
				
				return color * 0.5;
			}

			half4 Fragment(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				//return float4(0,1,0,1);
				float2 uv = UV;
				float2 uwUV = uv;
				
				DistortUV(uv, uwUV);
						
				float underwaterMask = SampleUnderwaterMask(uwUV);
				uwUV = lerp(uv, uwUV, underwaterMask);
				//return float4(uwUV.xy, 0, 1);
				
				half4 screenColor = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex, uwUV);
				
				float3 farColor = screenColor.rgb;
				float waterDensity = 1.0;
				
#if BLUR
				farColor = GaussianBlur(_SourceTex, sampler_SourceTex, uwUV);			

				float sceneDepth = SampleSceneDepth(uwUV);
				float3 positionWS = GetWorldPosition(uv, sceneDepth);

				float distanceDensity = ComputeDistanceXYZ(positionWS);	
				float heightDensity = ComputeHeight(positionWS);
				waterDensity = ComputeDensity(distanceDensity, heightDensity);
#endif

				float3 finalColor = lerp(screenColor.rgb, farColor.rgb, underwaterMask * waterDensity);
				
				return float4(finalColor.rgb, screenColor.a);
			}

			ENDHLSL
		}
	}
}
