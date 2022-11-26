//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifndef UNDERWATER_FOG_INCLUDED
#define UNDERWATER_FOG_INCLUDED

float _WaterLevel;

float4 _WaterShallowColor;
float4 _WaterDeepColor;

float _StartDistance;
float _FogDensity;

float _HeightFogDepth;
float _HeightFogDensity;
float _HeightFogBrightness;

float _UnderwaterFogBrightness;
float _UnderwaterSubsurfaceStrength;

/////////////
// DENSITY //
/////////////

//Radial distance
float ComputeDistanceXYZ(float3 positionWS)
{
	//Distance from camera (horizontally)
	float horizontal = length(_WorldSpaceCameraPos.xz - positionWS.xz);

	//Start distance
	horizontal -= _ProjectionParams.y + _StartDistance;
	horizontal *= _FogDensity;
	
	return saturate(1-(exp(-horizontal)));
}

float ComputeHeight(float3 positionWS)
{
	float start = (_WaterLevel - 1.0 - _HeightFogDepth) - _HeightFogDensity;
	
	float3 wsDir = _WorldSpaceCameraPos.xyz - positionWS;
	float FH = start; //Height
	float3 P = positionWS;
	float FdotC = _WorldSpaceCameraPos.y - start; //Camera/fog plane height difference
	float k = (FdotC <= 0.0f ? 1.0f : 0.0f); //Is camera below height fog
	float FdotP = P.y - FH;
	float FdotV = wsDir.y;
	float c1 = k * (FdotP + FdotC);
	float c2 = (1 - 2 * k) * FdotP;
	float g = min(c2, 0.0);
	g = -_HeightFogDensity * (c1 - g * g / abs(FdotV + 1.0e-5f));
	return 1-exp(-g);
}

float ComputeDensity(float distanceDepth, float heightDepth)
{
	//Blend of both factors
	return saturate(distanceDepth + heightDepth);
}

float GetUnderwaterFogDensity(float3 positionWS)
{
	const float distanceDensity = ComputeDistanceXYZ(positionWS);
	const float heightDensity = ComputeHeight(positionWS);
	const float density = ComputeDensity(distanceDensity, heightDensity);

	return density;
}

void GetWaterDensity_float(float4 positionWS, out float density)
{
	density = GetUnderwaterFogDensity(positionWS.xyz);
}

///////////
// COLOR //
///////////

//Questionable! The water colors used are always the same anyway.
//For the water shader itself, the material colors are passed it. For the water line, the same colors are passed in as global uniforms.
float3 GetUnderwaterFogColor(float3 shallow, float3 deep, float distanceDensity, float heightDensity)
{
	float3 waterColor = lerp(shallow.rgb, deep.rgb, distanceDensity) * _UnderwaterFogBrightness;
	
	waterColor = lerp(waterColor, deep.rgb * _HeightFogBrightness, heightDensity);
	
	return waterColor;
}

float3 GetUnderwaterFogColor(float distanceDensity, float heightDensity)
{
	return GetUnderwaterFogColor(_WaterShallowColor.rgb, _WaterDeepColor.rgb, distanceDensity, heightDensity);
}

float3 GetUnderwaterFogColor(float3 positionWS)
{
	const float distanceDensity = ComputeDistanceXYZ(positionWS);
	const float heightDensity = ComputeHeight(positionWS);

	return GetUnderwaterFogColor(distanceDensity, heightDensity);
}
#endif