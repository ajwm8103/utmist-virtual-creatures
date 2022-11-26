//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

Shader "Hidden/StylizedWater2/UnderwaterMask"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off

        Pass
        {
            Name "Underwater Mask"
            HLSLPROGRAM

            #pragma vertex VertexWaterLine
            #pragma fragment frag

            #pragma multi_compile_local _ _WAVES
            //#pragma multi_compile _ MODIFIERS_ENABLED

            #define FULLSCREEN_QUAD

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "UnderwaterMask.hlsl"

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                //Perform some antialiasing so the render target can be of a much lower resolution
                float gradient = pow(abs(input.uv.y), 256);
                return 1-gradient;
            }
            
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}