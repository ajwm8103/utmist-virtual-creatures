using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace StylizedWater2.UnderwaterRendering
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
            
            if(UnderwaterRenderer.EnableRendering == false) EditorGUILayout.HelpBox($"Rendering has been disabled by an external script, such as an Underwater Trigger component", MessageType.Warning);

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
            UI.DrawNotification(renderer.waterMaterial && renderer.waterMaterial.GetInt("_Cull") != (int)CullMode.Off, "The water material is not double-sided", "Make it so", () =>
            {
                StylizedWaterEditor.DisableCullingForMaterial(renderer.waterMaterial);
            }, MessageType.Error);
            
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
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    UnderwaterRenderer.VisualizeWaterLevel = GUILayout.Toggle(UnderwaterRenderer.VisualizeWaterLevel , new GUIContent("  Display Gizmo", EditorGUIUtility.IconContent((UnderwaterRenderer.VisualizeWaterLevel ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff")).image), "Button");
                }
                
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

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
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
            }

            if (EditorGUI.EndChangeCheck())
            {
                renderer.UpdateProperties();
                serializedObject.ApplyModifiedProperties();
            }
            
            UI.DrawFooter();
            #else
            EditorGUILayout.HelpBox("The Stylized Water 2 asset or the Universal Render Pipeline is not installed.", MessageType.Error);
            #endif
        }

        private void DrawNotifications()
        {
            #if URP
            UI.DrawNotification( !AssetInfo.MeetsMinimumVersion(UnderwaterRenderer.MinBaseVersion), "Version mismatch, requires Stylized Water 2 v" + UnderwaterRenderer.MinBaseVersion +".\n\nUpdate to avoid any issues or resolve (shader) errors", "Update", () => AssetInfo.OpenInPackageManager(), MessageType.Error);
            
            UI.DrawNotification(UniversalRenderPipeline.asset == null, "The Universal Render Pipeline is not active", MessageType.Error);
            #if !UNITY_2021_1_OR_NEWER //No MSAA on depth texture yet
            UI.DrawNotification(UniversalRenderPipeline.asset && UniversalRenderPipeline.asset.msaaSampleCount > 1, "MSAA is enabled, this causes artifacts in the fog", "Disable",
                () =>
                { 
                    UniversalRenderPipeline.asset.msaaSampleCount = 1;
                    EditorUtility.SetDirty(UniversalRenderPipeline.asset);
                }, 
                MessageType.Warning);
            #endif
            
            using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
            {
                UI.DrawNotification(!renderFeaturePresent, "The underwater render feature hasn't be added to the default renderer", "Add", () =>
                {
                    PipelineUtilities.AddRenderFeature<UnderwaterRenderFeature>();
                    renderFeaturePresent = true;
                    renderFeature = PipelineUtilities.GetRenderFeature<UnderwaterRenderFeature>() as UnderwaterRenderFeature;
                }, MessageType.Error);
            }
            if(Application.isPlaying && !renderFeaturePresent) EditorGUILayout.HelpBox("Exit play mode to perform this action", MessageType.Warning);
            
            UI.DrawNotification(renderFeaturePresent && !renderFeatureEnabled, "The underwater render feature is disabled", "Enable", () => 
            { 
                PipelineUtilities.ToggleRenderFeature<UnderwaterRenderFeature>(true);
                renderFeatureEnabled = true; 
            }, MessageType.Warning);
            
            #if UNITY_6000_0_OR_NEWER && URP
            if (GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode == false)
            {
                EditorGUILayout.HelpBox("Using Render Graph in Unity 6+ is not supported." +
                                        "\n\nBackwards compatibility mode must be enabled.", MessageType.Error);
                
                GUILayout.Space(-32);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Enable", EditorGUIUtility.IconContent("d_tab_next").image), GUILayout.Width(60)))
                    {
                        GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode = true;

                        EditorUtility.DisplayDialog($"Underwater Rendering v{UnderwaterRenderer.Version}", 
                            "Please note that this option will be removed in a future Unity version, this version of the asset will no longer be functional then." +
                            "\n\n" +
                            "A license upgrade for Unity 6+ support may be available on the asset store, please check the documentation for current information.", "OK");
                    }
                    GUILayout.Space(8);
                }
                GUILayout.Space(11);
            }
            #endif
            #endif
        }

        [MenuItem("GameObject/3D Object/Water/Underwater rendering", false, 2000)]
        [MenuItem("Window/Stylized Water 2/Set up underwater rendering", false, 3000)]
        private static void CreateUnderwaterRenderer()
        {
            #if UNITY_2023_1_OR_NEWER
            UnderwaterRenderer r = FindFirstObjectByType<UnderwaterRenderer>();
            #else
            UnderwaterRenderer r = FindObjectOfType<UnderwaterRenderer>();
            #endif

            if (r)
            {
                EditorUtility.DisplayDialog("Stylized Water 2", "An Underwater Renderer instance already exists. Only one can be created", "OK");
                
                return;
            }
            
            GameObject obj = new GameObject("Underwater Renderer", typeof(UnderwaterRenderer));
            Undo.RegisterCreatedObjectUndo(obj, "Created Underwater Renderer");

            r = obj.GetComponent<UnderwaterRenderer>();
            
            if (Selection.activeGameObject) obj.transform.parent = Selection.activeGameObject.transform;
            
            Selection.activeObject = obj;
        }
    }
}
