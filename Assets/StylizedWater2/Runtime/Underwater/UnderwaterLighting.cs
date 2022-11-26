//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace StylizedWater2
{
    public static class UnderwaterLighting
    {
#if URP
        private static readonly int _AmbientParams = Shader.PropertyToID("_AmbientParams");
        private static readonly int _UnderwaterAmbientColor = Shader.PropertyToID("_UnderwaterAmbientColor");
        
        //Global values that needs to be set up again, won't survive opaque pass
        private static readonly int skyboxCubemap = Shader.PropertyToID("skyboxCubemap");
        private static readonly int skyboxCubemap_HDR = Shader.PropertyToID("skyboxCubemap_HDR");
        private static readonly int unity_WorldToLight = Shader.PropertyToID("unity_WorldToLight");

        private static Vector4 ambientParams;
        
        public static void PassAmbientLighting(ScriptableRenderPass pass, CommandBuffer cmd)
        {
            //URP uses spherical harmonics to store the ambient light color, even if it's flat. But this is done in native engine code
            //Normally set up on a per-renderer basis, emulate the behaviour for post-processing passes
            
            if (RenderSettings.ambientMode == AmbientMode.Skybox)
            {
                cmd.SetGlobalTexture(skyboxCubemap, ReflectionProbe.defaultTexture);
                cmd.SetGlobalVector(skyboxCubemap_HDR, ReflectionProbe.defaultTextureHDRDecodeValues);
            }
            else if (RenderSettings.ambientMode == AmbientMode.Flat)
            {
                cmd.SetGlobalColor(_UnderwaterAmbientColor, RenderSettings.ambientLight.linear);
            }
            else //Tri-light
            {
                cmd.SetGlobalColor(_UnderwaterAmbientColor, RenderSettings.ambientEquatorColor.linear);
            }

            ambientParams.x = Mathf.GammaToLinearSpace(RenderSettings.ambientIntensity);
            ambientParams.y = RenderSettings.ambientMode == AmbientMode.Skybox ? 1 : 0;
            cmd.SetGlobalVector(_AmbientParams, ambientParams);
        }

        private static VisibleLight mainLight;
        public static void PassMainLight(CommandBuffer cmd, RenderingData renderingData)
        {
            // When no lights are visible, main light will be set to -1.
            if (renderingData.lightData.mainLightIndex > -1)
            {
                mainLight = renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex];
    
                if (mainLight.lightType == LightType.Directional)
                {
                    cmd.SetGlobalMatrix(unity_WorldToLight, mainLight.light.transform.worldToLocalMatrix);
                }
            }
        }
#endif
    }
}