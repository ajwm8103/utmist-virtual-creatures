//Stylized Water 2: Underwater Rendering extension
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedWater2.UnderwaterRendering
{
    [AddComponentMenu("Stylized Water 2/Underwater Renderer")]
    [ExecuteInEditMode]
    public class UnderwaterRenderer : MonoBehaviour
    {
        public const string Version = "1.2.8";
        public const string MinBaseVersion = "1.5.2";
        
#if URP
        public static UnderwaterRenderer Instance;
        /// <summary>
        /// If false, underwater rendering is disabled entirely
        /// </summary>
        public static bool EnableRendering = true;

        [HideInInspector]
        //If this is throwing an error either the Stylized Water 2 asset is not installed, or an outdated version is used (see MinBaseVersion)
        public WaveParameters waveParameters = new WaveParameters();

        public enum WaterLevelSource
        {
            FixedValue,
            Transform
        }
        
        [Tooltip("Configure what should be used to set the base water level. Either a fixed value, or based on a transform's world-space Y-position")]
        public WaterLevelSource waterLevelSource;
        [Tooltip("The base water level, this value is important and required for correct rendering. As such, underwater rendering does not work with rivers or other non-flat water")]
        public float waterLevel;
        [Tooltip("This transform's Y-position is used as the base water level, this value is important and required for correct rendering. As such, underwater rendering does not work with rivers or other non-flat water")]
        public Transform waterLevelTransform;
        public float CurrentWaterLevel
        {
            get
            {
                if (waterLevelSource == WaterLevelSource.Transform && waterLevelTransform) return waterLevelTransform.position.y;

                return waterLevel;
            }
        }
        
        [Tooltip("Rendering is triggered once the bottom of the screen touches the water.\n\nIf the water surface is artificially raised (eg. vertex shader), use this padding value to trigger the effects early." +
                 "\n\nSet this to at least 0.5 when using Dynamic Effects with displacement")]
        public float waterLevelPadding = 0.5f;
        
        [Tooltip("The water material used in the environment. This is used to copy its colors and wave settings, so everything is in sync")]
        public Material waterMaterial;
        [Tooltip("Only enable if the material's wave parameters are being changed in realtime, this has some performance overhead.\n\nIn edit-mode, the wave parameters are always fetched, so changes are directly visible")]
        public bool dynamicMaterial;
        
        [SerializeField]
        private UnderwaterSettings settings;

        //[Header("Fog")]
        [Tooltip("Control the fog settings through a Volume component. \"Underwater Settings\" must be present on any volume profile")]
        public bool useVolumeBlending;
        
        [Tooltip("Pushes the fog this many units away from the camera, resulting in clear water")]
        public float startDistance = 0f;
        [Min(0f)]
        public float fogDensity = 8f;

        [Min(0f)]
        [Tooltip("Distance from the water surface, where height fog will start")]
        public float heightFogDepth = 25f;
        [Min(0f)]
        [Tooltip("This essentially controls how harsh the start transition of the height fog is")]
        public float heightFogDensity = 1f;
        [Range(0f, 1f)]
        [Tooltip("Within the height fog, the fog color is multiplied by this value")]
        public float heightFogBrightness = 0.5f;

        //Multipliers
        [Min(0f)]
        public float fogBrightness = 1f;
        [Min(0f)]
        [Tooltip("This value acts as a multiplier for the translucency strength value on the water material")]
        public float subsurfaceStrength = 0.5f;
        [Min(0f)]
        [Tooltip("This value acts as a multiplier for the caustics strength value on the water material")]
        public float causticsStrength = 1f;
        
        [Range(0f, 2f)]
        public float distortionStrength = 0.25f;
        [Range(0f, 5f)]
        public float distortionFrequency = 0.75f;
        [Range(0f, 1f)]
        public float distortionSpeed = 0.5f;
        
        //[Header("Waterline")]
        [Tooltip("Pushes the lens effect this many units away from the camera. The camera's Near Clip value is added to this.")]
        [Min(0f)]
        public float offset = 0.25f;
        [Range(0.1f, 0.7f)]
        public float waterLineThickness = 0.4f;
        
        //[Header("Effects")]
        [Tooltip("Enables blurring based on fog density. This emulates light scattering in murky water")]
        public bool enableBlur;
        [Tooltip("Distorts the underwater image using an animated noise texture")]
        public bool enableDistortion;

        public const string Keyword = "UNDERWATER_ENABLED"; //multi_compile (global)
        
        private void Update()
        {
            if (!EnableRendering) return;
            
            if (dynamicMaterial || Application.isPlaying == false) UpdateMaterialParameters();
            
            if(useVolumeBlending && !settings) GetVolumeSettings();
            
            UpdateProperties();
        }
        
        private static int _WaterLevel = Shader.PropertyToID("_WaterLevel");
        private static int _ClipOffset = Shader.PropertyToID("_ClipOffset");
        
        private static int _StartDistance = Shader.PropertyToID("_StartDistance");
        private static int _UnderwaterFogDensity = Shader.PropertyToID("_UnderwaterFogDensity");
        
        private static int _HeightFogDepth = Shader.PropertyToID("_HeightFogDepth");
        private static int _HeightFogDensity = Shader.PropertyToID("_HeightFogDensity");
        private static int _HeightFogBrightness = Shader.PropertyToID("_HeightFogBrightness");
        
        private static int _UnderwaterFogBrightness = Shader.PropertyToID("_UnderwaterFogBrightness");
        private static int _UnderwaterSubsurfaceStrength = Shader.PropertyToID("_UnderwaterSubsurfaceStrength");
        private static int _UnderwaterCausticsStrength = Shader.PropertyToID("_UnderwaterCausticsStrength");
        
        private static int _DistortionStrength = Shader.PropertyToID("_DistortionStrength");
        private static int _DistortionFreq = Shader.PropertyToID("_DistortionFreq");
        private static int _DistortionSpeed = Shader.PropertyToID("_DistortionSpeed");

        /// <summary>
        /// Passes the fog parameters, water level and offset value to shader land. Call this whenever changing these values through script!
        /// </summary>
        public void UpdateProperties()
        {
            Shader.SetGlobalFloat(_WaterLevel, CurrentWaterLevel);
            Shader.SetGlobalFloat(_ClipOffset, offset);

            Shader.SetGlobalFloat(_StartDistance, useVolumeBlending && settings ? settings.startDistance.value : startDistance);
            Shader.SetGlobalFloat(_UnderwaterFogDensity, (useVolumeBlending && settings ? settings.fogDensity.value : fogDensity) * 0.01f);
            
            Shader.SetGlobalFloat(_HeightFogDepth, (useVolumeBlending && settings ? settings.heightFogDepth.value : heightFogDepth));
            Shader.SetGlobalFloat(_HeightFogDensity, (useVolumeBlending && settings ? settings.heightFogDensity.value : heightFogDensity) * 0.01f);
            Shader.SetGlobalFloat(_HeightFogBrightness, (useVolumeBlending && settings ? settings.heightFogBrightness.value : heightFogBrightness));
            
            Shader.SetGlobalFloat(_UnderwaterFogBrightness, (useVolumeBlending && settings ? settings.fogBrightness.value : fogBrightness));
            Shader.SetGlobalFloat(_UnderwaterSubsurfaceStrength, (useVolumeBlending && settings ? settings.subsurfaceStrength.value : subsurfaceStrength));
            Shader.SetGlobalFloat(_UnderwaterCausticsStrength, (useVolumeBlending && settings ? settings.causticsStrength.value : causticsStrength));
            
            Shader.SetGlobalFloat(_DistortionStrength, (useVolumeBlending && settings ? settings.distortionStrength.value  : distortionStrength) * 0.025f);
            Shader.SetGlobalFloat(_DistortionFreq, (useVolumeBlending && settings ? settings.distortionFrequency.value  : distortionFrequency));
            Shader.SetGlobalFloat(_DistortionSpeed, (useVolumeBlending && settings ? settings.distortionSpeed.value  : distortionSpeed) * 0.1f);
        }

        private static readonly int SourceShallowColorID = Shader.PropertyToID("_ShallowColor");
        private static readonly int SourceDeepColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int DestShallowColorID = Shader.PropertyToID("_WaterShallowColor");
        private static readonly int DestDeepColorID = Shader.PropertyToID("_WaterDeepColor");
        
        private static readonly int CausticsTexID = Shader.PropertyToID("_CausticsTex");
        private static readonly int CausticsTilingID = Shader.PropertyToID("_CausticsTiling");
        private static readonly int CausticsBrightnessID = Shader.PropertyToID("_CausticsBrightness");
        private static readonly int CausticsSpeedID = Shader.PropertyToID("_CausticsSpeed");
        private static readonly int SpeedID = Shader.PropertyToID("_Speed");
        
        private static readonly int _TranslucencyStrength = Shader.PropertyToID("_TranslucencyStrength");
        private static readonly int _TranslucencyExp = Shader.PropertyToID("_TranslucencyExp");
        
        public struct KeywordStates
        {
            public bool translucency;
            public bool caustics;
            public bool waves;
        }
        public KeywordStates materialKeywordStates;
        
        public const string TRANSLUCENCY_KEYWORD = "_TRANSLUCENCY";
        public const string REFRACTION_KEYWORD = "_REFRACTION";
        public const string WAVES_KEYWORD = "_WAVES";
        public const string CAUSTICS_KEYWORD = "_CAUSTICS";

        /// <summary>
        /// Fetches the water material's wave parameters and sends this to the underwater effects. if "dynamicMaterial" is enabled, this is performed every frame
        /// Call this function when changing the material
        /// </summary>
        public void UpdateMaterialParameters()
        {
            if (waterMaterial)
            {
                materialKeywordStates.translucency = waterMaterial.IsKeywordEnabled(TRANSLUCENCY_KEYWORD);
                materialKeywordStates.waves = waterMaterial.IsKeywordEnabled(WAVES_KEYWORD);
                materialKeywordStates.caustics = waterMaterial.IsKeywordEnabled(CAUSTICS_KEYWORD);
                
                Shader.SetGlobalColor(DestShallowColorID, waterMaterial.GetColor(SourceShallowColorID));
                Shader.SetGlobalColor(DestDeepColorID, waterMaterial.GetColor(SourceDeepColorID));

                Shader.SetGlobalTexture(CausticsTexID, waterMaterial.GetTexture(CausticsTexID));
                Shader.SetGlobalFloat(CausticsTilingID, waterMaterial.GetFloat(CausticsTilingID));
                Shader.SetGlobalFloat(CausticsBrightnessID, waterMaterial.GetFloat(CausticsBrightnessID));
                Shader.SetGlobalFloat(CausticsSpeedID, waterMaterial.GetFloat(CausticsSpeedID) * waterMaterial.GetFloat(SpeedID));
                
                Shader.SetGlobalFloat(_TranslucencyStrength, waterMaterial.GetFloat(_TranslucencyStrength));
                Shader.SetGlobalFloat(_TranslucencyExp, waterMaterial.GetFloat(_TranslucencyExp));
                
                //Debug.Log("Updating water material parameters");
                waveParameters.Update(waterMaterial);
                waveParameters.SetAsGlobal();
            }
        }
        
        private void Reset()
        {
            gameObject.name = "Underwater Renderer";
            
            //Component first added, fetch the water level for easy setup
            if (WaterObject.Instances.Count == 1)
            {
                waterLevel = WaterObject.Instances[0].transform.position.y;
                waterMaterial = WaterObject.Instances[0].material;
            }
        }

        private void OnEnable()
        {
            Instance = this;
            
            #if UNITY_EDITOR && URP
            if (Application.isPlaying == false)
            {
                if (PipelineUtilities.RenderFeatureMissing<UnderwaterRenderFeature>(out UnityEngine.Rendering.Universal.ScriptableRendererData[] renderers))
                {
                    string[] rendererNames = new string[renderers.Length];
                    for (int i = 0; i < rendererNames.Length; i++)
                    {
                        rendererNames[i] = "• " + renderers[i].name;
                    }

                    if (EditorUtility.DisplayDialog("Underwater Rendering", $"The Underwater Rendering render feature hasn't been added to the following renderers:\n\n" +
                                                                       System.String.Join(System.Environment.NewLine, rendererNames) +
                                                                       $"\n\nThis is required for rendering to take effect.", "Setup", "Ignore"))
                    {
                        PipelineUtilities.SetupRenderFeature<UnderwaterRenderFeature>(name:"Stylized Water 2: Underwater Rendering");
                    }
                }
            }
            #endif

            GetVolumeSettings();
            
            UpdateProperties();
            UpdateMaterialParameters();
            
#if URP
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
#endif

#if UNITY_EDITOR
            if(!Application.isPlaying) EditorApplication.update += Update;
#endif
        }

        public void GetVolumeSettings()
        {
            settings = VolumeManager.instance?.stack?.GetComponent<UnderwaterSettings>();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= Update;
#endif
            
#if URP
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
#endif
            
            Instance = null;
            
            UnderwaterUtilities.ToggleUnderwaterKeyword(false);
        }

        private void OnDestroy()
        {
            UnderwaterUtilities.ToggleUnderwaterKeyword(false);
        }
        
        /// <summary>
        /// If no Underwater Renderer instance is present, this does nothing. The waterLevelSource parameter will be set to "FixedValue"
        /// </summary>
        /// <param name="transform">The world-space position's Y-value will be used</param>
        public static void SetCurrentWaterLevel(Transform transform)
        {
            if (!Instance) return;

            Instance.waterLevelSource = WaterLevelSource.Transform;
           
            Instance.waterLevelTransform = transform;
            Instance.UpdateProperties();
        }

        /// <summary>
        /// If no Underwater Renderer instance is present, this does nothing. The waterLevelSource will be set to "FixedValue"
        /// </summary>
        /// <param name="height">Water level height in world-space</param>
        public static void SetCurrentWaterLevel(float height)
        {
            if (!Instance) return;

            Instance.waterLevelSource = WaterLevelSource.FixedValue;

            Instance.waterLevel = height;
            Instance.UpdateProperties();
        }

        /// <summary>
        /// Configure the water material used for underwater rendering. This affects the water line behaviour and overall appearance.
        /// </summary>
        /// <param name="material"></param>
        public static void SetCurrentWaterMaterial(Material material)
        {
            if (!Instance) return;

            if (material.GetFloat("_Cull") > 0)
            {
                throw new Exception($"[Underwater Renderer] Assigned material ({material.name}) is not double-sided! Set the Cull Faces option to \"Off\"");
            }
            
            Instance.waterMaterial = material;
            Instance.UpdateMaterialParameters();
            Instance.UpdateProperties();
        }

        /// <summary>
        /// Checks if the bottom of the camera's near-clip plane is below the maximum possible water level
        /// Does not account for rotation on the Z-axis!
        /// </summary>
        /// <param name="targetCamera"></param>
        /// <returns></returns>
        public bool CameraIntersectingWater(Camera targetCamera)
        {
            #if URP
            //Note: Does not account for rotation on Z-axis, should check for both left/right corners of the plane

            //Check if bottom of near plane touches water level.
            return (UnderwaterUtilities.GetNearPlaneBottomPosition(targetCamera, offset).y - (waveParameters.height)) <= (CurrentWaterLevel + waterLevelPadding);
            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// Checks if the top of the camera's near-clip plane is below the maximum possible water level
        /// Does not account for rotation on the Z-axis!
        /// </summary>
        /// <param name="targetCamera"></param>
        /// <returns></returns>
        public bool CameraSubmerged(Camera targetCamera)
        {
            #if URP
            //Check if top of near plane is below the water level.
            return (UnderwaterUtilities.GetNearPlaneTopPosition(targetCamera, offset).y + (waveParameters.height)) < (CurrentWaterLevel - waterLevelPadding);
            #else
            return false;
            #endif
        }

        private void OnBeginCameraRendering(ScriptableRenderContext content, Camera currentCamera)
        {
            //Little caveat, this must be done on a per-camera basis. The render passes are shared by all cameras using the same renderer.
            //Set the keyword before rendering, before any passes execute (including underwater rendering)
            UnderwaterUtilities.ToggleUnderwaterKeyword(CameraIntersectingWater(currentCamera) && EnableRendering);
        }

        private void OnEndCameraRendering(ScriptableRenderContext content, Camera currentCamera)
        {
            //Disable if necessary for whatever camera comes next, it may not be underwater
            UnderwaterUtilities.ToggleUnderwaterKeyword(false);
        }

        public static void DrawWaterLevelGizmo(Vector3 center, Vector3 scale, float waterLevel)
        {
            Gizmos.color = new Color(0, 1f, 0f, 0.5f);
            Vector3 pos = center;
            pos.y = waterLevel;
				
            Vector3 size = new Vector3(scale.x, 0f, scale.z);
            Gizmos.DrawCube(pos, size);
				
            Gizmos.color = new Color(0, 1f, 0f, 1f);
            Gizmos.DrawWireCube(pos, size);
        }

        public static bool VisualizeWaterLevel;
        
        private void OnDrawGizmosSelected()
        {
            if(VisualizeWaterLevel) DrawWaterLevelGizmo(this.transform.position, Vector3.one * 3000f, CurrentWaterLevel);
        }
#else
#error Underwater Rendering extension is imported without either the "Stylized Water 2" asset or the "Universal Render Pipeline" installed. Will not be functional until these are both installed and set up.
#endif
    }
}