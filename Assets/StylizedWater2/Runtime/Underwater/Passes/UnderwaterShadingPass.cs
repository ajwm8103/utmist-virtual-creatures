//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    class UnderwaterShadingPass : RenderPass
    {
        private const string ProfilerTag = "Underwater Rendering: Shading";
        private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(ProfilerTag);

        public UnderwaterShadingPass(UnderwaterRenderFeature renderFeature)
        {
            base.Initialize(renderFeature, renderFeature.resources.underwaterShader);
        }

        public const string DEPTH_NORMALS_KEYWORD = "_REQUIRE_DEPTH_NORMALS";
        public const string SOURCE_DEPTH_NORMALS_KEYWORD = "_SOURCE_DEPTH_NORMALS";
        
        public override void Setup(UnderwaterRenderFeature.Settings settings, ScriptableRenderer renderer)
        {
            base.Setup(settings, renderer);
            
            renderer.EnqueuePass(this);
        }
        
        public override void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.ConfigurePass(cmd, cameraTextureDescriptor);
            
            #if URP_10_0_OR_NEWER
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
            #endif
            
            if (settings.directionalCaustics)
            {
                #if URP_10_0_OR_NEWER
                if(settings.accurateDirectionalCaustics) 
                {
                    ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);
                    CoreUtils.SetKeyword(Material, SOURCE_DEPTH_NORMALS_KEYWORD, true);
                }
                else
                {
                    CoreUtils.SetKeyword(Material, SOURCE_DEPTH_NORMALS_KEYWORD, false);
                }
                #else
                CoreUtils.SetKeyword(Material, SOURCE_DEPTH_NORMALS_KEYWORD, false);
                #endif
            }
            
            CoreUtils.SetKeyword(Material, DEPTH_NORMALS_KEYWORD, settings.directionalCaustics);
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.TRANSLUCENCY_KEYWORD, renderFeature.keywordStates.translucency);
            CoreUtils.SetKeyword(Material, UnderwaterRenderer.CAUSTICS_KEYWORD, renderFeature.keywordStates.caustics);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                base.Execute(context, ref renderingData);
                
                UnderwaterLighting.PassAmbientLighting(this, cmd);
                UnderwaterLighting.PassMainLight(cmd, renderingData);

                BlitToCamera(cmd, ref renderingData);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        protected override void Cleanup(CommandBuffer cmd)
        {
            base.Cleanup(cmd);
        }
    }

}
#endif