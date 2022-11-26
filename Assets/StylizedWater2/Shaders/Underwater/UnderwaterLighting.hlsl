#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float4 _AmbientParams; //Skybox multiplier
//X: Intensity (skybox)
//Y: (bool) Skybox shading
float4 _UnderwaterAmbientColor;

TEXTURECUBE(skyboxCubemap);
SAMPLER(sampler_skyboxCubemap);
float4 skyboxCubemap_HDR;

#define AMBIENT_SKY_INTENSITY _AmbientParams.x
#define AMBIENT_SKYBOX _AmbientParams.y == 1
#define AMBIENT_SKYBOX_MIP 4 //Really don't need super detailed directional color accuracy, only sampling one texel

float3 SampleUnderwaterGI(float3 normalWS, float3 viewDir)
{
	float3 ambientColor = _UnderwaterAmbientColor.rgb;
	
	if(AMBIENT_SKYBOX)
	{
		float3 reflectVec = reflect(-viewDir, normalWS);
		float4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(skyboxCubemap, sampler_skyboxCubemap, reflectVec, AMBIENT_SKYBOX_MIP).rgba * AMBIENT_SKY_INTENSITY;

		#if !defined(UNITY_USE_NATIVE_HDR)
		ambientColor.rgb = DecodeHDREnvironment(encodedIrradiance , skyboxCubemap_HDR) ;
		#else
		ambientColor.rgb = encodedIrradiance.rgb;
		#endif
	}

	return ambientColor;
}

void ApplyUnderwaterLighting(inout float3 color, float shadowMask, float3 normalWS, float3 viewDir)
{
	float3 ambientColor = SampleUnderwaterGI(normalWS, viewDir);
	
	//Mirror lambert shading
	float diffuseTerm = saturate(dot(_MainLightPosition.xyz, normalWS));

	float3 directColor = _MainLightColor.rgb * diffuseTerm;
	float3 bakedGI = ambientColor * shadowMask;

	float3 diffuseColor = (bakedGI + directColor);

	color = color * diffuseColor;
}