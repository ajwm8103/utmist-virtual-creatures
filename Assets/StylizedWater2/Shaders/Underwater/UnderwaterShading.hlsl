//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifndef UNDERWATER_SHADING_INCLUDED
#define UNDERWATER_SHADING_INCLUDED

float _ClipOffset; //Lens offset
bool _FullySubmerged;

#if !defined(SHADERGRAPH_PREVIEW)
TEXTURE2D_X(_UnderwaterMask);
SAMPLER(sampler_UnderwaterMask);

#include "UnderwaterFog.hlsl"
#include "UnderwaterLighting.hlsl"
#endif

#define REFLECTION_ROUGHNESS 0.0
half _UnderwaterSurfaceSmoothness;
#define NORMAL_SMOOTHNESS _UnderwaterSurfaceSmoothness
half _UnderwaterRefractionOffset;
#define REFRACTION_OFFSET _UnderwaterRefractionOffset

#define WATER_RI 1.333
#define AIR_RI 1.000293 
#define SCHLICK_EXPONENT 5.0
#define REFLECTION_ALPHA 1

//Fresnel factor between two refractive media. https://en.wikipedia.org/wiki/Schlick%27s_approximation
float FresnelReflection(float angle)
{
	//R = (n1 - n2) / (n1 + n2)^2
	//R(ϴ) = R + (1 - R)(1 - cosϴ)^5
	
	float r = (AIR_RI - WATER_RI) / (AIR_RI + WATER_RI);
	r = r * r;
	
	return saturate(r + (AIR_RI - r) * pow(max(0.0, AIR_RI - angle), SCHLICK_EXPONENT));
}

//Schlick's BRDF fresnel
float ReflectionViewFresnel(float3 worldNormal, float3 viewDir, float exponent)
{
	float cosTheta = saturate(dot(worldNormal, viewDir));
	return pow(max(0.0, 1.0 - cosTheta), exponent);
}

float3 UnderwaterReflectionVector(float3 normalWS, float3 worldTangentNormal, float3 viewDir)
{
	//Blend between the vertex/wave normal and tangent normal map
	float3 normal = lerp(worldTangentNormal, normalWS, NORMAL_SMOOTHNESS);

	float3 reflectionVector = reflect(-viewDir, normal);

	//Mirror on the horizontal plane. Since normals are never actually oriented downwards
	reflectionVector.y = -reflectionVector.y;

	return reflectionVector;
}

float UnderwaterReflectionFactor(float3 normalWS, float3 worldTangentNormal, float3 viewDir)
{
	float3 normal = lerp(worldTangentNormal, normalWS, NORMAL_SMOOTHNESS);

	const float viewAngle = max(0.0, dot(normal, -viewDir));

	//If given a spherical normal, behaves as a lensing effect.
	const float refractionAngle = WATER_RI * sin(acos(viewAngle + REFRACTION_OFFSET)) / AIR_RI;
	const float reflectionAngle = acos(clamp(refractionAngle, -1.0, 1.0)) ;
	
	const float reflectionFresnel = 1.0 - FresnelReflection(reflectionAngle) * REFLECTION_ALPHA;
	const float viewFresnel = ReflectionViewFresnel(normalWS, viewDir, SCHLICK_EXPONENT);

	return (reflectionFresnel * viewFresnel);
}

float3 SampleUnderwaterReflectionProbe(float3 reflectionVector, float smoothness, float3 positionWS, float3 normalWS, float3 worldTangentNormal, float3 viewDir)
{
	#if !defined(SHADERGRAPH_PREVIEW)
	#if VERSION_GREATER_EQUAL(12,0)
	float3 reflections = GlossyEnvironmentReflection(reflectionVector, positionWS, smoothness, 1.0).rgb;
	#else
	float3 reflections = GlossyEnvironmentReflection(reflectionVector, smoothness, 1.0).rgb;
	#endif

	return reflections;
	#else
	return 0;
	#endif
}

//Color parameters now obsolete!
void ApplyLitUnderwaterFog(inout float3 color, float3 positionWS, float3 normalWS, float3 viewDir, int vFace)
{
	#ifndef SHADERGRAPH_PREVIEW
	float distanceDensity = ComputeDistanceXYZ(positionWS);	
	float heightDensity = ComputeHeight(positionWS);
	float density = ComputeDensity(distanceDensity, heightDensity);

	float3 volumeColor = GetUnderwaterFogColor(distanceDensity, heightDensity);

	ApplyUnderwaterLighting(volumeColor, 1, normalWS, viewDir);

	color = lerp(color, volumeColor, density);
	#endif
}

void MixUnderwaterReflections(inout float3 color, in float3 sceneColor, float skyMask, float3 positionWS, float3 normalWS, float3 worldTangentNormal, float3 viewDir, int vFace, float density)
{
	const float3 reflectionVector = UnderwaterReflectionVector(normalWS, worldTangentNormal, viewDir);
	float reflectionCoefficient = UnderwaterReflectionFactor(normalWS, worldTangentNormal, viewDir);

	//Fade out by fog density. Ensuring the reflections fade out as the water surface gets further away
	reflectionCoefficient *= density;

	#ifndef _ENVIRONMENTREFLECTIONS_OFF
		float3 incomingReflections = SampleUnderwaterReflectionProbe(reflectionVector, REFLECTION_ROUGHNESS, positionWS, normalWS, worldTangentNormal, viewDir);

		//Fallback to opaque texture for pixels in front of opaque geometry
		sceneColor = lerp(sceneColor, incomingReflections, skyMask);
	#endif
	
	//Faux-reflection of underwater volume
	color = lerp(color, sceneColor.rgb, reflectionCoefficient * (1-vFace));
}

//Main function called at the end of ForwardPass.hlsl
float3 ShadeUnderwaterSurface(in float3 albedo, float3 emission, float3 specular, float3 sceneColor, float skyMask, float shadowMask, float3 positionWS, float3 normalWS, float3 worldTangentNormal, float3 viewDir, float3 shallowColor, float3 deepColor, int vFace)
{
	float3 color = albedo.rgb;

	#ifndef SHADERGRAPH_PREVIEW
	const float distanceDensity = ComputeDistanceXYZ(positionWS);
	const float heightDensity = ComputeHeight(positionWS);
	const float density = ComputeDensity(distanceDensity, heightDensity);

	//Not using distanceDensity here, so only the deep color is returned, which better represents the volume's color
	float3 volumeColor = GetUnderwaterFogColor(shallowColor, deepColor, 1.0, heightDensity);
	
	//Fade out into fog
	shadowMask = lerp(shadowMask, 1.0, density);
	
	//Apply lighting to the albedo fog color
	ApplyUnderwaterLighting(volumeColor, shadowMask, normalWS, viewDir);

	color = lerp(color, volumeColor, density);
	//Re-apply translucency
	color.rgb += emission.rgb * (1-heightDensity);
	//Specular reflection (unknown why point lights don't carry over)
	color.rgb += specular.rgb * (1-density);

	MixUnderwaterReflections(color.rgb, sceneColor.rgb, skyMask, positionWS, normalWS, worldTangentNormal, viewDir, vFace, 1-density);
	#endif
	
	return color;
}

float SampleUnderwaterMask(float2 screenPos)
{
	#ifndef SHADERGRAPH_PREVIEW //SAMPLE_TEXTURE2D_X is yet available
	if(_FullySubmerged)
	{
		return 1;
	}
	else
	{
		return SAMPLE_TEXTURE2D_X(_UnderwaterMask, sampler_UnderwaterMask, screenPos.xy).r;
	}
	#else
	return 0;
	#endif
}

#define CAM_FWD unity_CameraToWorld._13_23_33
#define NEAR_PLANE _ProjectionParams.y

//Clip the water using a fake near-clipping plane.
float ClipSurface(float4 screenPos, float3 positionWS, float3 positionCS, float vFace)
{
#if UNDERWATER_ENABLED && !defined(SHADERGRAPH_PREVIEW) && !defined(UNITY_GRAPHFUNCTIONS_LW_INCLUDED)
	const float clipStart = NEAR_PLANE + _ClipOffset;
	const float3 viewPos = TransformWorldToView(positionWS);

	//Distance based scalar
	float f = saturate(-viewPos.z / clipStart);
	float mask = floor(f);

	//Clip space depth is not enough since vertex density is likely lower than the underwater mask
	//Sample the per-pixel water mask
	const float underwaterMask = SampleUnderwaterMask(screenPos.xy / screenPos.w);
	mask *= lerp(underwaterMask, 1-underwaterMask, vFace);
	
	clip(mask - 0.5);

	return mask;
#else
	return 1.0;
#endif
}

//Shading for external transparent materials
void ApplyUnderwaterShading(inout float3 color, float3 positionWS, float3 normal, float3 viewDir, float bottomFace)
{
	#if UNDERWATER_ENABLED && !defined(SHADERGRAPH_PREVIEW)
	const float distanceDensity = ComputeDistanceXYZ(positionWS);
	const float heightDensity = ComputeHeight(positionWS);
	const float density = ComputeDensity(distanceDensity, heightDensity);
	
	float3 fogColor = GetUnderwaterFogColor(distanceDensity, heightDensity);

	//Apply lighting to the albedo fog color
	ApplyUnderwaterLighting(fogColor, 1.0, normal, viewDir);
	
	const float mask = (bottomFace * density);
	color = lerp(color, fogColor, mask);
	#endif
}

void ApplyUnderwaterShading(inout float3 color, float3 positionWS, float2 screenPos)
{
	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - positionWS);
	float mask = SampleUnderwaterMask(screenPos);
	
	ApplyUnderwaterShading(color, positionWS, float3(0,1,0), viewDir, mask);
}

//////////////////
// Shader Graph //
//////////////////
void SampleUnderwaterMask_float(float4 screenPos, out float mask)
{
	mask = SampleUnderwaterMask(screenPos.xy / screenPos.w);
}

//Shader Graph
void ApplyUnderwaterShading_float(in float3 inEmission, float3 positionWS, out float3 outEmission, inout float density)
{
	outEmission = inEmission;
	density = 1;
	
	#if UNDERWATER_ENABLED
	float3 viewDir = SafeNormalize(_WorldSpaceCameraPos - positionWS);
	ApplyUnderwaterShading(outEmission, positionWS, float3(0,1,0), viewDir, 1.0);

	density = GetUnderwaterFogDensity(positionWS);
	
	outEmission = lerp(inEmission, outEmission, density);
	#endif
}
#endif