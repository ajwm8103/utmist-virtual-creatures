using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace StylizedWater2
{
    [CustomEditor(typeof(UnderwaterRenderer))]
    public class UnderwaterRendererInspector : Editor
    {
        private UnderwaterRenderer renderer;

        private SerializedProperty waterLevelSource;
        private SerializedProperty waterLevel;
        private SerializedProperty waterLevelTransform;
        private SerializedProperty waterLevelPadding;
        
        private SerializedProperty waterMaterial;
        private SerializedProperty dynamicMaterial;
        
        private SerializedProperty useVolumeBlending;
        
        private SerializedProperty startDistance;
        private SerializedProperty fogDensity;
        
        private SerializedProperty heightFogDepth;
        private SerializedProperty heightFogDensity;
        private SerializedProperty heightFogBrightness;
        
        private SerializedProperty fogBrightness;
        private SerializedProperty subsurfaceStrength;
        private SerializedProperty causticsStrength;
        
        private SerializedProperty distortionStrength;
        private SerializedProperty distortionFrequency;
        private SerializedProperty distortionSpeed;

        private SerializedProperty offset;
        private SerializedProperty waterLineThickness;
        
        private SerializedProperty enableBlur;
        private SerializedProperty enableDistortion;

        #if URP
        private bool renderFeaturePresent;
        private bool renderFeatureEnabled;
        private UnderwaterRenderFeature renderFeature;
        private Editor renderFeatureEditor;
        #endif

        private void OnEnable()
        {
            renderer = (UnderwaterRenderer)target;
            
            waterLevelSource = serializedObject.FindProperty("waterLevelSource");
            waterLevel = serializedObject.FindProperty("waterLevel");
            waterLevelTransform = serializedObject.FindProperty("waterLevelTransform");
            waterLevelPadding = serializedObject.FindProperty("waterLevelPadding");
            
            waterMaterial = serializedObject.FindProperty("waterMaterial");
            dynamicMaterial = serializedObject.FindProperty("dynamicMaterial");
            
            useVolumeBlending = serializedObject.FindProperty("useVolumeBlending");
            
            startDistance = serializedObject.FindProperty("startDistance");
            fogDensity = serializedObject.FindProperty("fogDensity");
            
            heightFogDepth = serializedObject.FindProperty("heightFogDepth");
            heightFogDensity = serializedObject.FindProperty("heightFogDensity");
            heightFogBrightness = serializedObject.FindProperty("heightFogBrightness");
            
            fogBrightness = serializedObject.FindProperty("fogBrightness");
            subsurfaceStrength = serializedObject.FindProperty("subsurfaceStrength");
            causticsStrength = serializedObject.FindProperty("causticsStrength");
            
            distortionStrength = serializedObject.FindProperty("distortionStrength");
            distortionFrequency = serializedObject.FindProperty("distortionFrequency");
            distortionSpeed = serializedObject.FindProperty("distortionSpeed");
            
            offset = serializedObject.FindProperty("offset");
            waterLineThickness = serializedObject.FindProperty("waterLineThickness");
            
            enableBlur = serializedObject.FindProperty("enableBlur");
            enableDistortion = serializedObject.FindProperty("enableDistortion");

            #if URP
            renderFeaturePresent = PipelineUtilities.RenderFeatureAdded<UnderwaterRenderFeature>();
            renderFeatureEnabled = PipelineUtilities.IsRenderFeatureEnabled<UnderwaterRenderFeature>();
            renderFeature = PipelineUtilities.GetRenderFeature<UnderwaterRenderFeature>() as UnderwaterRenderFeature;
            #endif
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Version " + UnderwaterRenderer.Version, EditorStyles.centeredGreyMiniLabel);
            
            #if URP
            DrawNotifications();
            
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.LabelField("Material", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(waterMaterial);

                EditorGUI.BeginDisabledGroup(waterMaterial.objectReferenceValue == null);
                if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(50f)))
                {
                    Selection.activeObject = waterMaterial.objectReferenceValue;
                }
                EditorGUI.EndDisabledGroup();
            }

            UI.DrawNotification(waterMaterial.objectReferenceValue == null, "The water material used by the water plane must be assigned", MessageType.Error);
            UI.DrawNotification(renderer.waterMaterial && renderer.waterMaterial.GetInt("_Cull") != (int)CullMode.Off, "The water material is not double-sided", "Make it so", () => SetMaterialDoubleSided(), MessageType.Error);
            
            if (waterMaterial.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(dynamicMaterial);
                
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Water level", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(waterLevelSource, new GUIContent("Source", waterLevelSource.tooltip));

                if (waterLevelSource.intValue == (int)UnderwaterRenderer.WaterLevelSource.FixedValue)
                {
                    EditorGUILayout.PropertyField(waterLevel);
                }
                else
                {
                    EditorGUILayout.PropertyField(waterLevelTransform);
                }
                EditorGUILayout.PropertyField(waterLevelPadding);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(useVolumeBlending);
                if (EditorGUI.EndChangeCheck())
                {
                     renderer.GetVolumeSettings();
                }

                if (!useVolumeBlending.boolValue)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.LabelField("Fog (distance from camera)", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(startDistance);
                    EditorGUILayout.PropertyField(fogDensity);
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField("Fog (distance from water)", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(heightFogDepth);
                    EditorGUILayout.PropertyField(heightFogDensity);
                    EditorGUILayout.PropertyField(heightFogBrightness);
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField("Multipliers", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(fogBrightness);
                    EditorGUILayout.PropertyField(subsurfaceStrength);
                    EditorGUILayout.PropertyField(causticsStrength);
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField("Distortion", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(distortionStrength);
                    EditorGUILayout.PropertyField(distortionFrequency);
                    EditorGUILayout.PropertyField(distortionSpeed);
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Lens", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(offset);
                EditorGUILayout.PropertyField(waterLineThickness);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(enableBlur);
                EditorGUILayout.PropertyField(enableDistortion);
                
                if (renderFeature)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Render feature settings", EditorStyles.boldLabel);

                    if (!renderFeatureEditor) renderFeatureEditor = Editor.CreateEditor(renderFeature);
                    SerializedObject serializedRendererFeaturesEditor = renderFeatureEditor.serializedObject;
                    serializedRendererFeaturesEditor.Update();
                
                    EditorGUI.BeginChangeCheck();

                    renderFeatureEditor.OnInspectorGUI();

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedRendererFeaturesEditor.ApplyModifiedProperties();
                        EditorUtility.SetDirty(renderFeature);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                renderer.UpdateProperties();
                serializedObject.ApplyModifiedProperties();
            }
            
            UI.DrawFooter();
            #else
            EditorGUILayout.HelpBox("The Stylized Water2 asset or the Universal Render Pipeline is not installed.", MessageType.Error);
            #endif
        }

        private void DrawNotifications()
        {
            #if URP
            UI.DrawNotification( !AssetInfo.MeetsMinimumVersion(UnderwaterRenderer.MinBaseVersion), "Version mismatch, requires Stylized Water 2 v" + UnderwaterRenderer.MinBaseVersion +".\n\nUpdate to avoid any issues or resolve (shader) errors", "Update", () => AssetInfo.OpenStorePage(), MessageType.Error);
            
            UI.DrawNotification(UniversalRenderPipeline.asset == null, "The Universal Render Pipeline is not active", MessageType.Error);
            UI.DrawNotification(UniversalRenderPipeline.asset && UniversalRenderPipeline.asset.msaaSampleCount > 1, "MSAA is enabled, this causes artifacts in the fog","Disable", () => DisableMSSA(), MessageType.Warning);
            UI.DrawNotification(UniversalRenderPipeline.asset && !UniversalRenderPipeline.asset.supportsCameraOpaqueTexture, "Opaque texture rendering is disabled, this is required for correct shading","Enable", StylizedWaterEditor.EnableOpaqueTexture, MessageType.Error);

            using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
            {
                UI.DrawNotification(!renderFeaturePresent, "The underwater render feature hasn't be added to the default renderer", "Add", () => AddRenderFeature(), MessageType.Error);
            }
            if(Application.isPlaying && !renderFeaturePresent) EditorGUILayout.HelpBox("Exit play mode to perform this action", MessageType.Warning);
            
            UI.DrawNotification(renderFeaturePresent && !renderFeatureEnabled, "The underwater render feature is disabled", "Enable", () => EnableRenderFeature(), MessageType.Warning);
            #endif
        }
        
        #if URP
        private void AddRenderFeature()
        {
            PipelineUtilities.AddRenderFeature<UnderwaterRenderFeature>();
            renderFeaturePresent = true;
            renderFeature = PipelineUtilities.GetRenderFeature<UnderwaterRenderFeature>() as UnderwaterRenderFeature;
        }

        private void EnableRenderFeature()
        {
            PipelineUtilities.ToggleRenderFeature<UnderwaterRenderFeature>(true);
            renderFeatureEnabled = true;
        }

        private void DisableMSSA()
        {
            UniversalRenderPipeline.asset.msaaSampleCount = 1;
            EditorUtility.SetDirty(UniversalRenderPipeline.asset);
        }

        private void SetMaterialDoubleSided()
        {
            StylizedWaterEditor.DisableCullingForMaterial(renderer.waterMaterial);
        }
        #endif
        
        [MenuItem("Window/Stylized Water 2/Set up underwater rendering", false, 2000)]
        private static void CreateUnderwaterRenderer()
        {
            UnderwaterRenderer r = FindObjectOfType<UnderwaterRenderer>();

            if (r)
            {
                EditorUtility.DisplayDialog("Stylized Water 2", "An Underwater Renderer instance already exists. Only one can be created", "OK");
                
                return;
            }
            
            GameObject obj = new GameObject("Underwater Renderer", typeof(UnderwaterRenderer));
            r = obj.GetComponent<UnderwaterRenderer>();
            
            Selection.activeObject = obj;
        }
    }

/* Disabled for now, used to strip Surface Modifiers functionality which isn't available atm
    //Currently in place to strip Surface Modifiers functionality, if not installed
    partial class KeywordStripper
    {
        partial void AddUnderwaterShaders(ref List<Shader> shaders)
        {
            shaders.Add(Shader.Find("Hidden/StylizedWater2/UnderwaterMask"));
            shaders.Add(Shader.Find("Hidden/StylizedWater2/Waterline"));
        }
    }
    */
}
