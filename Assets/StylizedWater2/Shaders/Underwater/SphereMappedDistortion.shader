Shader "Hidden/StylizedWater2/SphereMappedDistortionOffset"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZTest Always
        ZWrite Off
        Cull Off //Mesh already has flipped normals
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "../Libraries/URP.hlsl" //Required to find DecodeHDREnvironment down the line
            #include "UnderwaterEffects.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
	            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                //Mesh is rendered at Matrix4x4.identity, hence vertices are transformed manually

                //Scale to the projection's field of view
                //Additional scale increases noise frequency, but also offers more variety
                output.positionOS = input.positionOS.xyz *  unity_CameraInvProjection._m11 * 4.0;
                
                //Position to camera origin
                output.positionWS.xyz = _WorldSpaceCameraPos.xyz + output.positionOS;

                output.positionCS = TransformWorldToHClip(output.positionWS);

                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float offset = MapWorldSpaceDistortionOffsets(input.positionOS);
                
                return float4(offset.xxx, 1.0);
            }
            ENDHLSL
        }
    }
}
