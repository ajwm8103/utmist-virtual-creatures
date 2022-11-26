//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    public class UnderwaterMaskPass : ScriptableRenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Mask";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);

        //Perfectly fine to render this at a quarter resolution
        private const int DOWNSAMPLES = 4;

        private Material Material;

        private RenderTargetIdentifier waterMaskRT;
        private readonly int waterMaskID = Shader.PropertyToID("_UnderwaterMask");

        private UnderwaterRenderFeature renderFeature;

        public UnderwaterMaskPass(UnderwaterRenderFeature renderFeature)
        {
            this.renderFeature = renderFeature;
            Material = UnderwaterRenderFeature.CreateMaterial(ProfilerTag, renderFeature.resources.watermaskShader);
            waterMaskRT = new RenderTargetIdentifier(waterMaskID, 0, CubemapFace.Unknown, -1);
        }

        public void Setup(UnderwaterRenderFeature.Settings settings, ScriptableRenderer renderer)
        {
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.WAVES_KEYWORD, renderFeature.keywordStates.waves);
            
            renderer.EnqueuePass(this);
        }
        
#if UNITY_2020_1_OR_NEWER //URP 9+
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
#else
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
#endif
        {
            #if UNITY_2020_1_OR_NEWER //URP 9+
            var cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            #endif
            
            cameraTextureDescriptor.width /= DOWNSAMPLES;
            cameraTextureDescriptor.height /= DOWNSAMPLES;
            cameraTextureDescriptor.msaaSamples = 1;
            cameraTextureDescriptor.graphicsFormat = GraphicsFormat.R8_UNorm;
            cmd.GetTemporaryRT(waterMaskID, cameraTextureDescriptor, FilterMode.Bilinear);
            
            cmd.SetGlobalTexture(waterMaskID, waterMaskID);
            
            ConfigureTarget(waterMaskRT);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.DrawMesh(UnderwaterUtilities.WaterLineMesh, Matrix4x4.identity, Material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

#if URP_9_0_OR_NEWER
        public override void OnCameraCleanup(CommandBuffer cmd)
#else
        public override void FrameCleanup(CommandBuffer cmd)
#endif
        {
            cmd.ReleaseTemporaryRT(waterMaskID);
        }
    }
}
#endif