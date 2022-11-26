//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    public class UnderwaterLinePass : ScriptableRenderPass
    {
        private const string ProfilerTag = "Underwater line Rendering";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);

        private static readonly int _WaterLineWidth = Shader.PropertyToID("_WaterLineWidth");
        
        private UnderwaterRenderFeature renderFeature;
        private UnderwaterRenderFeature.Settings settings;

        private readonly Material Material;

        public UnderwaterLinePass(UnderwaterRenderFeature renderFeature)
        {
            this.renderFeature = renderFeature;
            this.settings = renderFeature.settings;
            Material = UnderwaterRenderFeature.CreateMaterial(ProfilerTag, renderFeature.resources.waterlineShader);
        }
        
#if UNITY_2020_1_OR_NEWER //URP 9+
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
#else
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
#endif
        {
            ConfigureClear(ClearFlag.None, Color.clear);
            
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.REFRACTION_KEYWORD, settings.waterlineRefraction);
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.TRANSLUCENCY_KEYWORD, renderFeature.keywordStates.translucency);
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.WAVES_KEYWORD, renderFeature.keywordStates.waves);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                UnderwaterLighting.PassAmbientLighting(this, cmd);
                UnderwaterLighting.PassMainLight(cmd, renderingData);

                cmd.SetGlobalFloat(_WaterLineWidth, UnderwaterRenderer.Instance.waterLineThickness * 0.1f);
                cmd.DrawMesh(UnderwaterUtilities.WaterLineMesh, Matrix4x4.identity, Material, 0, 0);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
#endif