using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    public class RenderPass : ScriptableRenderPass
    {
        protected UnderwaterResources resources;
        protected UnderwaterRenderFeature.Settings settings;
        protected UnderwaterRenderFeature renderFeature;
        
        protected RenderTargetIdentifier cameraColorSource;
        protected RenderTargetIdentifier cameraColorTarget;
        protected readonly int sourceTexID = Shader.PropertyToID("_SourceTex");
        #if UNITY_2023_1_OR_NEWER
        protected readonly int blitTexID = Shader.PropertyToID("_BlitTexture");
        #else
        protected readonly int blitTexID = Shader.PropertyToID("_SourceTex");
        #endif
        
        protected Material Material;
        protected Material m_BlitMaterial;

        protected bool requireColorCopy = true;
        private bool xrRendering;

        protected void Initialize(UnderwaterRenderFeature renderFeature, Shader shader)
        {
            this.renderFeature = renderFeature;
            this.settings = renderFeature.settings;
            this.resources = renderFeature.resources;

            Material = CoreUtils.CreateEngineMaterial(shader);
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/Blit"));
            
            cameraColorSource = new RenderTargetIdentifier(sourceTexID, 0, CubemapFace.Unknown, -1);
        }
        
#if UNITY_2020_1_OR_NEWER //URP 9+
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
#else
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
#endif
        {
#if UNITY_2020_1_OR_NEWER //URP 9+
            RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
#endif

            ConfigurePass(cmd, cameraTextureDescriptor);
        }

        public virtual void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(sourceTexID, cameraTextureDescriptor);
            cmd.SetGlobalTexture(sourceTexID, sourceTexID);
            
            //At this point, the target is unbound. At least for the first frame
            //ConfigureTarget(cameraColorTarget);
        }

        private void CheckVR(ref RenderingData renderingData)
        {
            #if UNITY_2020_1_OR_NEWER && ENABLE_VR
            xrRendering = renderingData.cameraData.xrRendering;
            #else
            xrRendering = false;
            #endif
        }

        public virtual void Setup(UnderwaterRenderFeature.Settings settings, ScriptableRenderer renderer)
        {
            #if !UNITY_2020_2_OR_NEWER //URP 10+
            //otherwise fetched in Execute function, no longer allowed from a ScriptableRenderFeature setup function (target may be not be created yet, or was disposed)
            this.cameraColorTarget = renderer.cameraColorTarget;
            #endif
        }

        protected void GetColorTarget(ref RenderingData renderingData)
        {
            #if UNITY_2020_2_OR_NEWER //URP 10+
            //Color target can now only be fetched inside a ScriptableRenderPass
            
            #if UNITY_2022_1_OR_NEWER
            this.cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            #else
            this.cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            #endif
            #endif
        }

        private static readonly int _BlitScaleBiasRt = Shader.PropertyToID("_BlitScaleBiasRt");
        private static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        private static readonly Vector4 ScaleBias = new Vector4(1, 1, 0, 0);

        protected void BlitToCamera(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //Required for vertex shader
            cmd.SetGlobalVector(_BlitScaleBiasRt, ScaleBias);
            cmd.SetGlobalVector(_BlitScaleBias, ScaleBias);
            
            if (requireColorCopy)
            {
                //Color copy
                cmd.SetGlobalTexture(blitTexID, cameraColorTarget);
                cmd.SetRenderTarget(cameraColorSource);

                if (xrRendering)
                {
                    cmd.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, 0, MeshTopology.Quads, 4, 1, null);
                }
                else
                {
                    Blit(cmd,cameraColorTarget, cameraColorSource, m_BlitMaterial, 0);
                }
                
                //Blit to camera color target
                cmd.SetGlobalTexture(sourceTexID, cameraColorSource);
                if (xrRendering)
                {
                    cmd.SetRenderTarget(new RenderTargetIdentifier(cameraColorTarget, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                    cmd.DrawProcedural(Matrix4x4.identity, Material, 0, MeshTopology.Quads, 4, 1, null);
                }
                else
                {
                    cmd.SetRenderTarget(cameraColorTarget);
                    Blit(cmd, cameraColorSource, cameraColorTarget, Material, 0);
                }
            }
            else
            {
                #if UNITY_2021_2_OR_NEWER //URP 12+
                cmd.SetGlobalTexture(sourceTexID, cameraColorTarget);
                this.Blit(cmd, ref renderingData, Material, 0);
                #endif
            }
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            GetColorTarget(ref renderingData);

            CheckVR(ref renderingData);
            
            #if !UNITY_2021_2_OR_NEWER || UNITY_2022_1_OR_NEWER //not URP 12
            //The swap buffer behaviour appears to be broken in 2022.1
            //Depth buffer seems to be missing, causing the water to render over all opaque geometry
            requireColorCopy = false;
            #endif
            
            if(xrRendering) requireColorCopy = true;
        }
        
#if UNITY_2020_1_OR_NEWER //URP 9+
        public override void OnCameraCleanup(CommandBuffer cmd)
#else
        public override void FrameCleanup(CommandBuffer cmd)
#endif
        {
            Cleanup(cmd);
        }
        
        protected virtual void Cleanup(CommandBuffer cmd)
        {
            if (requireColorCopy) cmd.ReleaseTemporaryRT(sourceTexID);
        }
    }
}
#endif