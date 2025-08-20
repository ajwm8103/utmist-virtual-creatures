using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

#if UNITY_2022_1_OR_NEWER
using RenderTarget = UnityEngine.Rendering.RTHandle;
#else
using RenderTarget = UnityEngine.Rendering.RenderTargetIdentifier;
#endif

namespace StylizedWater2.UnderwaterRendering
{
    public class RenderPass : ScriptableRenderPass
    {
        protected UnderwaterResources resources;
        protected UnderwaterRenderFeature.Settings settings;
        protected UnderwaterRenderFeature renderFeature;

        private RTHandle cameraColorSource;
        private RenderTarget cameraColorTarget;
        private RenderTarget cameraDepthTarget;

        private readonly int sourceTexID = Shader.PropertyToID("_SourceTex");

        //In the interest of consistency, some one at Unity thinks its a good idea to change parameter names
        #if UNITY_2022_2_OR_NEWER
        private const string BlitInputTexName = "_BlitTexture";
        #else
        private const string BlitInputTexName = "_SourceTex";
        #endif

        private readonly int blitInputID = Shader.PropertyToID(BlitInputTexName);
        
        protected Material Material;
        private static Material _BlitMaterial;
        private static Material BlitMaterial
        {
            get
            {
                if (!_BlitMaterial)
                {
                    _BlitMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Universal Render Pipeline/Blit"));
                }
                
                return _BlitMaterial;
            }
        }
        
        private bool xrRendering;
        
        private const string CopyProfilerTag = "Copy camera color";
        private static readonly ProfilingSampler m_CopyProfilingSampler = new ProfilingSampler(CopyProfilerTag);

        protected void Initialize(UnderwaterRenderFeature renderFeature, Shader shader)
        {
            this.renderFeature = renderFeature;
            this.settings = renderFeature.settings;
            this.resources = renderFeature.resources;

            if(shader) Material = CoreUtils.CreateEngineMaterial(shader);
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
            //At this point, the target is unbound. At least for the first frame
            //ConfigureTarget(cameraColorTarget, cameraDepthTarget);
        }

        protected void AllocateColorCopy(RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (RTHandleNeedsReAlloc(cameraColorSource, cameraTextureDescriptor, "_SourceTex"))
            {
                //Note: function does a null check, needed for the first allocation
                if(cameraColorSource != null) RTHandles.Release(cameraColorSource);
                cameraColorSource = RTHandles.Alloc(cameraTextureDescriptor.width, cameraTextureDescriptor.height, cameraTextureDescriptor.volumeDepth, 
                    DepthBits.None, cameraTextureDescriptor.graphicsFormat, FilterMode.Point, TextureWrapMode.Clamp, cameraTextureDescriptor.dimension, 
                    cameraTextureDescriptor.enableRandomWrite, useMipMap:false, msaaSamples:(MSAASamples)cameraTextureDescriptor.msaaSamples, useDynamicScale:cameraTextureDescriptor.useDynamicScale
                    #if UNITY_2022_3_OR_NEWER
                    , vrUsage:cameraTextureDescriptor.vrUsage
                    #endif
                    );
            }
        }

        public static bool RTHandleNeedsReAlloc(RTHandle handle, in RenderTextureDescriptor descriptor, in string name)
        {
            //#if UNITY_2022_1_OR_NEWER
            //Using this results in a depth texture constantly being allocated?!
            //return RenderingUtils.ReAllocateIfNeeded(ref handle, descriptor);
            //#else
            //If not ever being allocated
            if (handle == null || handle.rt == null)
            {
                #if SWS_DEV
                //Debug.Log($"RTHandle {name} null, allocating");
                #endif
                return true;
            }
            
            //Resolution changes
            if ((handle.rt.width != descriptor.width || handle.rt.height != descriptor.height))
            {
                #if SWS_DEV
                //Debug.Log($"{name} resolution changed. Source:{descriptor.width}x{descriptor.height}. Current:{handle.rt.width}x{handle.rt.height}");
                #endif
                return true;
            }

            //In case XR is initialized at some point
            if (handle.rt.descriptor.dimension != descriptor.dimension)
            {
                #if SWS_DEV
                //Debug.Log($"{name} dimensions changed. Source:{descriptor.dimension}. Current:{handle.rt.descriptor.dimension}");
                #endif
                return true;
            }

            return false;
        }

        private void CheckVR(ref RenderingData renderingData)
        {
            #if UNITY_2020_1_OR_NEWER && ENABLE_VR && ENABLE_XR_MODULE
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
            this.cameraDepthTarget = renderer.cameraDepthTarget;
            #endif
        }

        private void GetColorTarget(ref RenderingData renderingData)
        {
            #if UNITY_2020_2_OR_NEWER //URP 10+
            //Color target can now only be fetched inside a ScriptableRenderPass
            
            #if UNITY_2022_1_OR_NEWER
            this.cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            this.cameraDepthTarget = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            #else
            this.cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            this.cameraDepthTarget = renderingData.cameraData.renderer.cameraDepthTarget;
            #endif
            #endif
        }

        private static readonly int _BlitScaleBiasRt = Shader.PropertyToID("_BlitScaleBiasRt");
        private static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
        private static readonly Vector4 ScaleBias = new Vector4(1, 1, 0, 0);

        protected void BlitToCamera(CommandBuffer cmd, ref RenderingData renderingData, bool copyColor)
        {
            //Required for vertex shader
            cmd.SetGlobalVector(_BlitScaleBiasRt, ScaleBias);
            cmd.SetGlobalVector(_BlitScaleBias, ScaleBias);

            RenderTarget source = copyColor ? cameraColorSource : cameraDepthTarget;
            
            if (copyColor)
            {
                //Copy camera color source
                //Seemingly always needed when rendering before transparent materials, otherwise breaks the depth buffer
                //+ Always needed for VR. Swap buffer fails to work there.
                using (new ProfilingScope(cmd, m_CopyProfilingSampler))
                {
                    //Color copy
                    cmd.SetGlobalTexture(blitInputID, cameraColorTarget);
                    cmd.SetRenderTarget(cameraColorSource, cameraDepthTarget, 0, CubemapFace.Unknown, -1);

                    if (xrRendering)
                    {
                        cmd.DrawProcedural(Matrix4x4.identity, BlitMaterial, 0, MeshTopology.Quads, 4, 1, null);
                    }
                    else
                    {
                        Blit(cmd, cameraColorTarget, cameraColorSource, BlitMaterial, 0);
                    }
                    
                    cmd.SetGlobalTexture(sourceTexID, cameraColorSource);
                }
            }

            //Blit to camera color target
            cmd.SetRenderTarget(cameraColorTarget, cameraDepthTarget, 0, CubemapFace.Unknown, -1);
            
            if (xrRendering)
            {
                cmd.DrawProcedural(Matrix4x4.identity, Material, 0, MeshTopology.Quads, 4, 1, null);
            }
            else
            {
                #if UNITY_2022_3_OR_NEWER
                //This API respects the current depth/stencil buffer!
                Blitter.BlitTexture(cmd, source, ScaleBias, Material, 0);
                #else
                Blit(cmd, source, cameraColorTarget, Material, 0);
                #endif
            }
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            GetColorTarget(ref renderingData);

            CheckVR(ref renderingData);
        }
        
#if UNITY_2020_1_OR_NEWER //URP 9+
        public override void OnCameraCleanup(CommandBuffer cmd)
#else
        public override void FrameCleanup(CommandBuffer cmd)
#endif
        {
            Cleanup(cmd);
        }

        public virtual void Dispose()
        {
            RTHandles.Release(cameraColorSource);
        }
        
        protected virtual void Cleanup(CommandBuffer cmd)
        {
        }
    }
}
#endif