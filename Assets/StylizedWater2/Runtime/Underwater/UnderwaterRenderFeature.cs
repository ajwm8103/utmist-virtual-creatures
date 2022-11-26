//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System;
using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    #if URP_11_0_OR_NEWER
    [DisallowMultipleRendererFeature]
    #endif
    public class UnderwaterRenderFeature : ScriptableRendererFeature
    {
        //Shared resources, ensures they're included in a build when the render feature is in use
        [SerializeField]
        #if !SWS_DEV
        [HideInInspector]
        #endif
        public UnderwaterResources resources;

        [Serializable]
        public class Settings
        {
            [Header("Quality/Performance")]
            public bool allowBlur = true;
            public bool allowDistortion = true;
            
            public enum DistortionMode
            {
                [InspectorName("Screen-space (Fastest)")]
                ScreenSpace,
                [InspectorName("Camera-space (Nicest)")]
                CameraSpace
            }
            [Tooltip("Screen-space mode is faster, but distortion will appear to move with the camera\n\n" +
                     "Camera-space mode looks better, but requires more calculations")]
            public DistortionMode distortionMode = DistortionMode.CameraSpace;
            
            [Space]
            
            [Tooltip("Limit caustics only to parts of a surface where sun light hits it")]
            public bool directionalCaustics;
            [Tooltip("(Requires Unity 2020.2+) Use the depth normals texture created from the Depth Normals pre-pass." +
                     "\n\nThis can negatively impact performance if the game isn't already optimized for draw calls!" +
                     "\n\nIf disabled, normals will be reconstructed from the depth texture")]
            public bool accurateDirectionalCaustics = false;

            [Space]
            
            [Tooltip("Attempts to create a glass-like appearance by refracting the scene geometry behind the water line. Note this does not refract the water surface behind it")]
            public bool waterlineRefraction = true;
        }
        public Settings settings = new Settings();
        
        private UnderwaterMaskPass maskPass;
        private UnderwaterLinePass waterLinePass;
        private UnderwaterShadingPass underwaterShadingPass;
        private UnderwaterPost underwaterPostPass;
        
        public UnderwaterRenderer.KeywordStates keywordStates;
        
        private void Reset() //Note: editor-only
        {
            if (!resources) resources = UnderwaterResources.Find();
            
            //Recommended fastest settings
            #if UNITY_IOS || UNITY_TVOS || UNITY_ANDROID || UNITY_SWITCH
            settings.directionalCaustics = false;
            settings.accurateDirectionalCaustics = false;
            settings.allowBlur = false;
            settings.allowDistortion = false;
            settings.distortionMode = Settings.DistortionMode.ScreenSpace;
            settings.waterlineRefraction = false;
            #endif
        }

        public override void Create()
        {
            #if UNITY_EDITOR
            if (!resources) resources = UnderwaterResources.Find();
            #endif

            maskPass = new UnderwaterMaskPass(this);
            maskPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            
            underwaterShadingPass = new UnderwaterShadingPass(this);
            underwaterShadingPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            
            underwaterPostPass = new UnderwaterPost(this);
            underwaterPostPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            
            waterLinePass = new UnderwaterLinePass(this);
            waterLinePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        private bool cameraIntersecting;
        private bool cameraSubmerged;

        private bool RequiresPostProcessingPass(UnderwaterRenderer renderer)
        {
            return (renderer.enableBlur && settings.allowBlur) || (renderer.enableDistortion && settings.allowDistortion);
        }
        
        private bool InvalidRenderingContext(CameraData cameraData)
        {
            #if SWS_DEV
            //Debug.Log($"Name:{cameraData.camera.name} Type:{cameraData.cameraType} Enabled:{cameraData.camera.enabled}");
            #endif
            
            //Likely a planar reflections camera or otherwise
            if (cameraData.cameraType != CameraType.SceneView && cameraData.camera.enabled == false) return true;
            
            //Camera stacking and depth-based post processing is essentially non-functional.
            //All effects render twice to the screen, causing double brightness. Next to fog causing overlay objects to appear transparent
            //- Best option is to not render anything for overlay cameras
            //- Reflection probes do not capture the water line correctly
            //- Preview cameras end up rendering the effect into asset thumbnails
            if (cameraData.renderType == CameraRenderType.Overlay || cameraData.camera.cameraType == CameraType.Reflection || cameraData.camera.cameraType == CameraType.Preview) return true;

#if UNITY_EDITOR
            //Skip if post-processing is disabled in scene-view
            if (cameraData.cameraType == CameraType.SceneView && UnityEditor.SceneView.lastActiveSceneView && !UnityEditor.SceneView.lastActiveSceneView.sceneViewState.showImageEffects) return true;

#endif

            //Skip hidden or off-screen cameras. 
            if (cameraData.cameraType == CameraType.Game && cameraData.camera.hideFlags != HideFlags.None) return true;
            
            #if UNITY_EDITOR
            //Skip rendering if editing a prefab
                #if UNITY_2021_2_OR_NEWER
                if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return true;
                #else
                if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return true;
                #endif
            #endif

            return false;
        }
        
        private int _FullySubmerged = Shader.PropertyToID("_FullySubmerged");
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!UnderwaterRenderer.EnableRendering) return;
            
            if (UnderwaterRenderer.Instance == null) return;
            if (UnderwaterRenderer.Instance.waterMaterial == null) return;

            if (InvalidRenderingContext(renderingData.cameraData)) return;
            
            //Only render if the bottom of the screen touches the water or is below the water level
            cameraIntersecting = UnderwaterRenderer.Instance.CameraIntersectingWater(renderingData.cameraData.camera);

            if (cameraIntersecting)
            {
                keywordStates = UnderwaterRenderer.Instance.materialKeywordStates;
                cameraSubmerged = UnderwaterRenderer.Instance.CameraSubmerged(renderingData.cameraData.camera);

                //Once submerged, the pass stops executing. At which point the water mask buffer will be left entirely filled
                if (!cameraSubmerged)
                {
                    maskPass.Setup(settings, renderer);
                }
 
                //Note: Previously was assigning Texture2D.redTexture as the water mask, but this breaks in VR since the texture isn't a texture array
                //Instead return a full white value in the shader function
                Shader.SetGlobalInt(_FullySubmerged, cameraSubmerged ? 1 : 0);

                underwaterShadingPass.Setup(settings, renderer);

                if (RequiresPostProcessingPass(UnderwaterRenderer.Instance))
                {
                    underwaterPostPass.Setup(settings, renderer);
                }

                //No need to render this if the water line won't be visible
                if (!cameraSubmerged)
                {
                    renderer.EnqueuePass(waterLinePass);
                }
            }
        }

        public static Material CreateMaterial(string profilerTag, Shader shader)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!shader)
            {
                //Debug.LogError("[" + profilerTag + "] Shader could not be found, ensure all files are imported");
                return null;
            }
            #endif
            
            return CoreUtils.CreateEngineMaterial(shader);
        }
    }
}
#endif