//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Enabled when inspecting the mesh on a renderer for a specific camera
//#define DEBUG

#include "../Libraries/URP.hlsl"

#if MODIFIERS_ENABLED
//#include "../SurfaceModifiers/SurfaceModifiers.hlsl"
#endif

float _WaterLevel;
float _ClipOffset;
float _WaterLineWidth;
		   
float4 _AnimationParams;
float _WaveHeight;
float _WaveNormalStr; //Unused
float _WaveDistance;
float4 _WaveDirection;
float _WaveSteepness;
float _WaveSpeed;
uint _WaveCount;

//Additional wave height to avoid any cracks
#define PADDING 0.0

#ifdef DEBUG
	float _NearPlane;
	float _FarPlane;
	float _CamFov;
	float3 _CamForward;
	float3 _CampUp;
	float3 _CamRight;
	float3 _CamPos;
	float _CamAspect;

	#define NEAR_PLANE _NearPlane
	#define ASPECT _CamAspect
	#define CAM_FOV _CamFov
	#define CAM_POS _CamPos
	#define CAM_RIGHT _CamRight
	#define CAM_UP _CampUp
	#define CAM_FWD _CamForward
#else
	//Current camera
	#define NEAR_PLANE _ProjectionParams.y
	#define ASPECT _ScreenParams.x / _ScreenParams.y
	#define CAM_FOV unity_CameraInvProjection._m11
	#define CAM_POS _WorldSpaceCameraPos
	#define CAM_RIGHT unity_WorldToCamera[0].xyz //Possibly flipped as well, but doesn't matter
	#define CAM_UP unity_WorldToCamera[1].xyz
	//The array variant is flipped when stereo rendering is in use. Using the camera center forward vector also works for VR
	#define CAM_FWD -GetWorldToViewMatrix()[2].xyz
#endif   

#define DEGREES_2_RAD PI / 180.0
#define FOV_SCALAR 2.0

#include "../Libraries/Waves.hlsl"
 
struct Attributes
{
	float4 positionOS : POSITION;
	float2 uv         : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float4 positionCS 	: SV_POSITION;
	float2 uv 			: TEXCOORD0;
	#ifdef WATERLINE
	float3 positionWS	: TEXCOORD1;
	float4 screenPos 	: TEXCOORD2;
	#endif
	UNITY_VERTEX_OUTPUT_STEREO
};

float GetWaveAmplitude(float3 position, float3 wavePosition)
{
	//Distance from near plane top to wave position
	float waveAmp = length(wavePosition - position);

	//If above water, amplitude should be negative
	if (position.y > wavePosition.y) waveAmp = -waveAmp;

	return waveAmp + PADDING;
}

float GetWaveAmplitudeXZ(float2 position, float2 wavePosition)
{
	//Distance from near plane top to wave position
	float waveAmp = length(wavePosition - position);

	return waveAmp + PADDING;
}

Varyings VertexWaterLine(Attributes input)
{
	Varyings output = (Varyings)0;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.uv.xy = input.uv.xy;

	float clipPlane = (NEAR_PLANE * 2.0) + _ClipOffset;
	
	#ifdef WATERLINE
	//Draw just in front of the mask mesh
	clipPlane -= 0.001;

	//Scale unit rectangle by width
	input.positionOS.y *= _WaterLineWidth;
	#endif

	//Near-clip plane position
	float3 nearPlaneCenter = CAM_POS + (CAM_FWD * clipPlane);

	const float fovScalar = FOV_SCALAR * CAM_FOV;
	const float aspectRatio = ASPECT;
	
	//Position vertices on the near-clip plane and scale to fit
	float3 positionWS = (nearPlaneCenter + (CAM_RIGHT * (input.positionOS.x * aspectRatio * fovScalar) * clipPlane)) + (CAM_UP * input.positionOS.y * clipPlane * fovScalar);
	float3 bottom = (nearPlaneCenter + (CAM_RIGHT * (input.positionOS.x * aspectRatio * fovScalar) * clipPlane)) - (CAM_UP * clipPlane * fovScalar);
	float3 top = (nearPlaneCenter + (CAM_RIGHT * (input.positionOS.x * aspectRatio * fovScalar) * clipPlane)) + (CAM_UP * clipPlane * fovScalar);
	float planeLength = distance(bottom, top);

	//Distance from near-clip bottom to water level (straight up)
	float depth = _WaterLevel - bottom.y;

	//Camera's X-angle
	float upFactor = dot(CAM_UP, float3(0.0, 1.0, 0.0));
	float angle = (acos(upFactor) * 180.0) / PI;

	//Distance from center to water level along tangent (from known opposite length and angle)
	float hypotenuse = (depth / cos(DEGREES_2_RAD * angle));

	//Intersection point with water level when traveling along the plane's tangent
	float3 samplePos = bottom + (CAM_UP * hypotenuse);

	#if MODIFIERS_ENABLED
	samplePos += SampleDisplacementModifiersNear(samplePos);
	#endif
	
	float waveAmp = 0;
	#if _WAVES
	WaveInfo waves = GetWaveInfo(samplePos.xz, ((_TimeParameters.x * _AnimationParams.z) * _AnimationParams.xy) * _WaveSpeed, 1000, 1001);
	waves.position.xz = samplePos.xz;
	//Wave height is relative to 0, convert to absolute world-space height and scale
	waves.position.y = _WaterLevel + (waves.position.y * _WaveHeight);

	//Distance from near plane bottom to wave position
	waveAmp = GetWaveAmplitude(bottom, waves.position);

	//If below the lowest possible wave height, fix to top of plane
	if (top.y + _WaveHeight < _WaterLevel) waveAmp = planeLength;

	#else
	//Simply snap to water level
	waveAmp = length(samplePos - bottom);
	#endif
	
#if defined(FULLSCREEN_QUAD)
	//Only affect top row of vertices
	if(input.positionOS.y >= 0.5)
	{	   				
		positionWS = bottom + (CAM_UP * (waveAmp));

		//float2 horizontalOffset = GetWaveAmplitudeXZ(positionWS, waves.position);
		//positionWS.xz += horizontalOffset;
	}
#endif

#ifdef WATERLINE
	waveAmp -= planeLength * 0.5;
	
	positionWS += (CAM_UP * (waveAmp));
#endif

	output.positionCS = TransformWorldToHClip(positionWS);

	#ifdef WATERLINE
	output.positionWS = positionWS;
	output.screenPos = ComputeScreenPos(output.positionCS);
	#endif

   return output;
}