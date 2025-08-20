//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2.UnderwaterRendering
{
    public class UnderwaterMaskPass : ScriptableRenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Mask";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);

        //Perfectly fine to render this at a quarter resolution
        private const int DOWNSAMPLES = 4;

        private Material Material;

        private RTHandle waterMaskRT;
        private readonly int waterMaskID = Shader.PropertyToID("_UnderwaterMask");

        private UnderwaterRenderFeature renderFeature;

        public UnderwaterMaskPass(UnderwaterRenderFeature renderFeature)
        {
            this.renderFeature = renderFeature;
            Material = UnderwaterRenderFeature.CreateMaterial(ProfilerTag, renderFeature.resources.watermaskShader);
        }

        public void Setup(UnderwaterRenderFeature.Settings settings, ScriptableRenderer renderer)
        {
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.WAVES_KEYWORD, renderFeature.keywordStates.waves);
            
            renderer.EnqueuePass(this);
        }
        
        #if UNITY_6000_0_OR_NEWER //Silence warning spam
        public override void RecordRenderGraph(UnityEngine.Rendering.RenderGraphModule.RenderGraph renderGraph, ContextContainer frameData) { }
        #endif


        #pragma warning disable CS0672
        #pragma warning disable CS0618
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.width /= DOWNSAMPLES;
            cameraTextureDescriptor.height /= DOWNSAMPLES;
            cameraTextureDescriptor.msaaSamples = 1;
            cameraTextureDescriptor.graphicsFormat = GraphicsFormat.R8_UNorm;
            cameraTextureDescriptor.dimension = TextureDimension.Tex2D;
            
            if (RenderPass.RTHandleNeedsReAlloc(waterMaskRT, cameraTextureDescriptor, "_UnderwaterMask"))
            {
                if(waterMaskRT != null) RTHandles.Release(waterMaskRT);
                waterMaskRT = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height, cameraTextureDescriptor.volumeDepth, DepthBits.None, cameraTextureDescriptor.graphicsFormat, FilterMode.Bilinear, TextureWrapMode.Clamp, TextureDimension.Tex2D, name: "_UnderwaterMask");
            }
            
            cmd.SetGlobalTexture(waterMaskID, waterMaskRT);
            
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
        
        public void Dispose()
        {
            RTHandles.Release(waterMaskRT);
        }
    }
}
#endif