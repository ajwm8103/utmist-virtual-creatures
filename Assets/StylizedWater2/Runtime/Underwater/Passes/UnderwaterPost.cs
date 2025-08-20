//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2.UnderwaterRendering
{
    class UnderwaterPost : RenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Post Processing";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);

        private const string BlurKeyword = "BLUR";

        private readonly int _DistortionNoise = Shader.PropertyToID("_DistortionNoise");

        private const string DistortionSSKeyword = "_SCREENSPACE_DISTORTION";
        private const string DistortionWSKeyword = "_CAMERASPACE_DISTORTION";

        public UnderwaterPost(UnderwaterRenderFeature renderFeature)
        {
            base.Initialize(renderFeature, renderFeature.resources.postProcessShader);
        }

        public override void Setup(UnderwaterRenderFeature.Settings settings, ScriptableRenderer renderer)
        {
            base.Setup(settings, renderer);
            
            renderer.EnqueuePass(this);
        }
        
        #if UNITY_6000_0_OR_NEWER
        #pragma warning disable CS0672
        #pragma warning disable CS0618
        #endif
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            
            AllocateColorCopy(cameraTextureDescriptor);
            
            if (UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion)
            {
                cmd.SetGlobalTexture(_DistortionNoise, resources.distortionNoise);
            }
            
            CoreUtils.SetKeyword(Material, BlurKeyword, UnderwaterRenderer.Instance.enableBlur && settings.allowBlur);
            CoreUtils.SetKeyword(Material, DistortionSSKeyword, UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion && settings.distortionMode == UnderwaterRenderFeature.Settings.DistortionMode.ScreenSpace);
            CoreUtils.SetKeyword(Material, DistortionWSKeyword, UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion && settings.distortionMode == UnderwaterRenderFeature.Settings.DistortionMode.CameraSpace);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                base.Execute(context, ref renderingData);

                BlitToCamera(cmd, ref renderingData, true);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
    
    class DistortionSpherePass : ScriptableRenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Post Processing (Distortion)";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);
        
        private RTHandle distortionSphereRT;

        private const string _DistortionSphere = "_DistortionSphere";
        private readonly int _DistortionSphereID = Shader.PropertyToID("_DistortionSphere");
        
        private readonly Material DistortionSphereMaterial;
        private readonly Mesh geoSphere;

        public DistortionSpherePass(UnderwaterResources resources)
        {
            if(resources.distortionShader) DistortionSphereMaterial = CoreUtils.CreateEngineMaterial(resources.distortionShader);
            this.geoSphere = resources.geoSphere;
        }

        #if UNITY_6000_0_OR_NEWER //Silence warning spam
        public override void RecordRenderGraph(UnityEngine.Rendering.RenderGraphModule.RenderGraph renderGraph, ContextContainer frameData) { }
        #endif
        
        #if UNITY_6000_0_OR_NEWER
        #pragma warning disable CS0672
        #pragma warning disable CS0618
        #endif
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.R8;
            cameraTextureDescriptor.msaaSamples = 1;
            cameraTextureDescriptor.width /= 4;
            cameraTextureDescriptor.height /= 4;
            //cameraTextureDescriptor.dimension = TextureDimension.Tex2D;

            if (RenderPass.RTHandleNeedsReAlloc(distortionSphereRT, cameraTextureDescriptor, _DistortionSphere))
            {
                //Note: function does a null check, needed for the first allocation
                if(distortionSphereRT != null) RTHandles.Release(distortionSphereRT);
                distortionSphereRT = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height, cameraTextureDescriptor.volumeDepth, DepthBits.None, cameraTextureDescriptor.graphicsFormat, FilterMode.Bilinear, TextureWrapMode.Clamp, cameraTextureDescriptor.dimension, name: _DistortionSphere);

            }
            cmd.SetGlobalTexture(_DistortionSphereID, distortionSphereRT);
            
            ConfigureTarget(distortionSphereRT);
            ConfigureClear(ClearFlag.Color, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                //cmd.SetRenderTarget(distortionSphereRT);
                //cmd.ClearRenderTarget(false, true, Color.clear);

                cmd.DrawMesh(geoSphere, Matrix4x4.identity, DistortionSphereMaterial, 0);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        public void Dispose()
        {
            RTHandles.Release(distortionSphereRT);
        }
    }

}
#endif