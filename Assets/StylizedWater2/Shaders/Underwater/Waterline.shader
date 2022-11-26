//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

Shader "Hidden/StylizedWater2/Waterline"
{
	SubShader
	{
		Tags { "RenderQueue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

		//ZWrite should be disabled for post-processing pass
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			Name "Waterline"
			HLSLPROGRAM
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex VertexWaterLine
			#pragma fragment frag

			#define WATERLINE

			#pragma multi_compile_local _ _REFRACTION
			#pragma multi_compile_local_fragment _ _TRANSLUCENCY
			#pragma multi_compile_local _ _WAVES
			//#pragma multi_compile _ MODIFIERS_ENABLED

			#include "UnderwaterMask.hlsl"
			#include "UnderwaterEffects.hlsl"
			#include "UnderwaterLighting.hlsl"
			#include "../Libraries/Lighting.hlsl"

			float4 _TranslucencyParams;
			float4 _WaterShallowColor;
			float4 _WaterDeepColor;
			float _UnderwaterFogBrightness;
			float _UnderwaterSubsurfaceStrength;
			
			#if _REFRACTION
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
			#endif

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				
				float2 uv = input.uv;

				float gradient = saturate(min(uv.y, 1-uv.y) * 2.0);

				float3 color = lerp(_WaterDeepColor.rgb, _WaterShallowColor.rgb, gradient * 0.25);

				//View direction can be planar, since the mesh is flat on the frustrum anyway
				ApplyUnderwaterLighting(color, 1, float3(0,1,0), CAM_FWD);

				#if _TRANSLUCENCY
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - input.positionWS);
				
				TranslucencyData translucencyData = PopulateTranslucencyData(_WaterShallowColor.rgb, _MainLightPosition.xyz, _MainLightColor.rgb, viewDir, float3(0,1,0), float3(0,1,0), 0, _TranslucencyParams);
				translucencyData.strength *= _UnderwaterFogBrightness * _UnderwaterSubsurfaceStrength;
				ApplyTranslucency(translucencyData, color);
				#endif
				
				float2 screenPos = input.screenPos.xy / input.screenPos.w;
				
			#if _REFRACTION
				float2 screenPosRef = screenPos;
				screenPosRef.y = 1-screenPosRef.y;
				screenPosRef.y += (gradient * 0.1);
				
				float3 sceneColor = SampleSceneColor(screenPosRef);
				color.rgb = lerp(sceneColor, color.rgb, 0.85);
				//color.rgb = sceneColor.rgb;
			#endif

				float sceneDepth = SampleSceneDepth(screenPos );
				
				#if !UNITY_REVERSED_Z //OpenGL + Vulkan
				sceneDepth = 1.0 - sceneDepth;
				#endif
				
				float dist = 1-saturate(sceneDepth / 0.5);
				//return float4(dist.xxx, 1.0);
				
				return float4(color.rgb, gradient * dist);
			}
			ENDHLSL
		}
	}
	FallBack "Hidden/InternalErrorShader"
}