//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    class UnderwaterPost : RenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Post Processing";
        private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);
        
        private RenderTargetIdentifier distortionSphereRT;
        
        private readonly int _DistortionNoise = Shader.PropertyToID("_DistortionNoise");
        private readonly int _DistortionSphere = Shader.PropertyToID("_DistortionSphere");

        private const string DistortionSSKeyword = "_SCREENSPACE_DISTORTION";
        private const string DistortionWSKeyword = "_CAMERASPACE_DISTORTION";
        private const string BlurKeyword = "BLUR";

        private Material DistortionSphereMaterial;

        public UnderwaterPost(UnderwaterRenderFeature renderFeature)
        {
            base.Initialize(renderFeature, renderFeature.resources.postProcessShader);
            
            DistortionSphereMaterial = CoreUtils.CreateEngineMaterial(resources.distortionShader);
            distortionSphereRT = new RenderTargetIdentifier(_DistortionSphere, 0, CubemapFace.Unknown, -1);
        }

        public override void Setup(UnderwaterRenderFeature.Settings settings, ScriptableRenderer renderer)
        {
            base.Setup(settings, renderer);
            
            renderer.EnqueuePass(this);
        }
        
        public override void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.ConfigurePass(cmd, cameraTextureDescriptor);
            
            CoreUtils.SetKeyword(Material, BlurKeyword, UnderwaterRenderer.Instance.enableBlur && settings.allowBlur);
            CoreUtils.SetKeyword(Material, DistortionSSKeyword, UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion && settings.distortionMode == UnderwaterRenderFeature.Settings.DistortionMode.ScreenSpace);
            CoreUtils.SetKeyword(Material, DistortionWSKeyword, UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion && settings.distortionMode == UnderwaterRenderFeature.Settings.DistortionMode.CameraSpace);

            if (UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion && settings.distortionMode == UnderwaterRenderFeature.Settings.DistortionMode.CameraSpace)
            {
                cameraTextureDescriptor.colorFormat = RenderTextureFormat.R8;
                cameraTextureDescriptor.msaaSamples = 1;
                cameraTextureDescriptor.width /= 4;
                cameraTextureDescriptor.height /= 4;
                
                cmd.GetTemporaryRT(_DistortionSphere, cameraTextureDescriptor, FilterMode.Bilinear);
                cmd.SetGlobalTexture(_DistortionSphere, _DistortionSphere);
            }
        }

        private void RenderDistortionSphere(CommandBuffer cmd)
        {
            if (UnderwaterRenderer.Instance.enableDistortion && settings.allowDistortion)
            {
                cmd.SetGlobalTexture(_DistortionNoise, resources.distortionNoise);

                if (settings.distortionMode == UnderwaterRenderFeature.Settings.DistortionMode.CameraSpace)
                {
                    cmd.SetRenderTarget(distortionSphereRT);
                    cmd.ClearRenderTarget(false, true, Color.clear);

                    cmd.DrawMesh(resources.geoSphere, Matrix4x4.identity, DistortionSphereMaterial, 0);
                }
            }
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                base.Execute(context, ref renderingData);
                
                RenderDistortionSphere(cmd);

                BlitToCamera(cmd, ref renderingData);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        protected override void Cleanup(CommandBuffer cmd)
        {
            base.Cleanup(cmd);
            cmd.ReleaseTemporaryRT(_DistortionSphere);
        }
    }

}
#endif