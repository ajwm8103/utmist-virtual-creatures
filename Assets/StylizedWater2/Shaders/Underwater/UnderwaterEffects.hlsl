//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Victim of changes in early URP versions. Technically applies to 7.5.2 and older, but can't check for sub-versions
#if (SHADER_LIBRARY_VERSION_MAJOR == 7 && SHADER_LIBRARY_VERSION_MINOR < 5) || defined(STEREO_INSTANCING_ON) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
#define LEFT_HANDED_VIEW_SPACE
#endif

float3 GetWorldPosition(float2 uv, float deviceDepth)
{
	//This unrolls to an array using [unity_StereoEyeIndex] when VR is enabled
	float4x4 invViewProjMatrix = unity_CameraInvProjection;
	
	#if UNITY_REVERSED_Z //Anything other than OpenGL + Vulkan
	deviceDepth = (1.0 - deviceDepth) * 2.0 - 1.0;
	
	//https://issuetracker.unity3d.com/issues/shadergraph-inverse-view-projection-transformation-matrix-is-not-the-inverse-of-view-projection-transformation-matrix
	invViewProjMatrix._12_22_32_42 = -invViewProjMatrix._12_22_32_42;
	
	real rawDepth = deviceDepth;
	#else
	//Adjust z to match NDC for OpenGL + Vulkan
	real rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, deviceDepth);
	#endif

	//Unrolled from ComputeWorldSpacePosition and ComputeViewSpacePosition functions. Since ComputeWorldSpacePosition actually returns the view-space position
	float4 positionCS  = ComputeClipSpacePosition(uv.xy, rawDepth);
	float4 hpositionWS = mul(invViewProjMatrix, positionCS);
	
	//The view space uses a right-handed coordinate system.
	#ifndef LEFT_HANDED_VIEW_SPACE
	hpositionWS.z = -hpositionWS.z;
	#endif
	
	float3 positionVS = hpositionWS.xyz / max(0, hpositionWS.w);
	float3 positionWS = mul(unity_CameraToWorld, float4(positionVS, 1.0)).xyz;

	return positionWS;
}

float3 ViewSpacePosition(float2 uv)
{
	float rawDepth = SampleSceneDepth(uv);

	#if UNITY_REVERSED_Z //Anything other than OpenGL + Vulkan
	rawDepth = (1.0 - rawDepth) * 2.0 - 1.0;
	#else
	rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, rawDepth);
	#endif

	return ComputeViewSpacePosition(uv, rawDepth, unity_CameraInvProjection);
}

#if _SOURCE_DEPTH_NORMALS && SHADER_LIBRARY_VERSION_MAJOR >= 10
#define DEPTH_NORMALS_PREPASS_AVAILABLE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#else
#undef DEPTH_NORMALS_PREPASS_AVAILABLE
#endif

float3 GetWorldNormal(float2 screenPos)
{
	half3 viewNormal = 0;
	
	#ifdef DEPTH_NORMALS_PREPASS_AVAILABLE
	viewNormal = SampleSceneNormals(screenPos);
	#else

	//https://wickedengine.net/2019/09/22/improved-normal-reconstruction-from-depth/
	
	// get current pixel's view space position
	const half3 center = ViewSpacePosition(screenPos);

	// get view space position at 1 pixel offsets in each major direction
	const half3 left = ViewSpacePosition(screenPos - float2(_ScreenParams.z - 1.0, 0.0));
	const half3 up = ViewSpacePosition(screenPos + float2(0.0, _ScreenParams.w - 1.0));
	const half3 right = ViewSpacePosition(screenPos + float2(_ScreenParams.z - 1.0, 0.0));
	const half3 down = ViewSpacePosition(screenPos - float2(0.0, _ScreenParams.w - 1.0));

	// get the difference between the current and each offset position
	half3 l = center - left;
	half3 r = right - center;
	half3 d = center - down;
	half3 u = up - center;

	// pick horizontal and vertical diff with the smallest z difference
	const half3 H = abs(l.z) < abs(r.z) ? l : r;
	const half3 V = abs(d.z) < abs(u.z) ? d : u;

	// get view space normal from the cross product of the diffs
	viewNormal = normalize(cross(H, V));

	#if !UNITY_REVERSED_Z //OpenGL + Vulkan
	viewNormal.xz = -viewNormal.xz;
	#endif
	
	viewNormal.y = -viewNormal.y;
	#endif

	float3 worldNormal = mul((float3x3)unity_CameraToWorld, viewNormal);
	
	return worldNormal;
}

TEXTURE2D(_DistortionNoise); SAMPLER(sampler_DistortionNoise);
TEXTURE2D_X(_DistortionSphere); SAMPLER(sampler_DistortionSphere);

#define HQ_WORLDSPACE_DISTORTION

#if SHADER_API_MOBILE
#undef HQ_WORLDSPACE_DISTORTION
#endif

float _DistortionFreq;
float _DistortionStrength;
float _DistortionSpeed;

#define HALF_FREQUENCY 0.5
#define STRENGTH_SCALAR 4.0

float MapWorldSpaceDistortionOffsets(float3 wPos)
{
	wPos *= _DistortionFreq;
	float distortionOffset = _TimeParameters.x * _DistortionSpeed;
	
	float x1 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.y + distortionOffset, wPos.z + distortionOffset)).r;
	#ifdef HQ_WORLDSPACE_DISTORTION
	float x2 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.y - distortionOffset * HALF_FREQUENCY, wPos.z + distortionOffset)).r;
	#endif

	//Note: okay to skip Y-axis projection
	
	float z1 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.x + distortionOffset, wPos.y + distortionOffset)).r;
	#ifdef HQ_WORLDSPACE_DISTORTION
	float z2 =  SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(wPos.x + distortionOffset * HALF_FREQUENCY, wPos.y + distortionOffset)).r;
	#endif

	#ifdef HQ_WORLDSPACE_DISTORTION
	float offset = (x1 * x2 * z1 * z2) * 2.0;
	#else
	float offset = (x1 * z1);
	#endif

	return offset;
}

half DistortUV(float2 uv, inout float2 distortedUV)
{
	half offset = 0;
	
#if _SCREENSPACE_DISTORTION
	float2 distortionFreq = uv * _DistortionFreq;
	float distortionOffset = _TimeParameters.x * _DistortionSpeed;
				
	float n1 = SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(distortionFreq.x + distortionOffset, distortionFreq.y + distortionOffset)).r ;
	float n2 = SAMPLE_TEXTURE2D(_DistortionNoise, sampler_DistortionNoise, float2(distortionFreq.x - (distortionOffset * HALF_FREQUENCY), distortionFreq.y)).r;

	offset = (n1 * n2);
#endif

#if _CAMERASPACE_DISTORTION
	offset = SAMPLE_TEXTURE2D_X(_DistortionSphere, sampler_DistortionSphere, uv).r;
#endif

#if _SCREENSPACE_DISTORTION || _CAMERASPACE_DISTORTION
	offset *= _DistortionStrength * STRENGTH_SCALAR;
	
	#ifdef UNITY_REVERSED_Z
	//Offset always has to push up, otherwise creates a seam where the water meets the shore
	distortedUV = uv.xy - offset;
	#else
	distortedUV = uv.xy + offset;
	#endif
	
#endif

	return offset;
}