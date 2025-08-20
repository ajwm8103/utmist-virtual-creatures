using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace StylizedWater2.UnderwaterRendering
{
    [CustomEditor(typeof(UnderwaterRenderFeature))]
    public class RenderFeatureEditor : Editor
    {
        private SerializedProperty resources;
        private SerializedProperty settings;
        
        private SerializedProperty allowBlur;
        private SerializedProperty allowDistortion;
        private SerializedProperty distortionMode;
        
        private SerializedProperty directionalCaustics;
        private SerializedProperty accurateDirectionalCaustics;
        
        private SerializedProperty waterlineRefraction;

        private void OnEnable()
        {
            resources = serializedObject.FindProperty("resources");
            
            settings = serializedObject.FindProperty("settings");
            
            allowBlur = settings.FindPropertyRelative("allowBlur");
            allowDistortion = settings.FindPropertyRelative("allowDistortion");
            distortionMode = settings.FindPropertyRelative("distortionMode");
            
            directionalCaustics = settings.FindPropertyRelative("directionalCaustics");
            accurateDirectionalCaustics = settings.FindPropertyRelative("accurateDirectionalCaustics");
            
            waterlineRefraction = settings.FindPropertyRelative("waterlineRefraction");
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Version {UnderwaterRenderer.Version}", EditorStyles.miniLabel);
            }
            EditorGUILayout.Space();
            
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
                            "Please note that this option will be removed in a future Unity version, this version will no longer be functional then." +
                            "\n\n" +
                            "A license upgrade for Unity 6+ support may be available, please check the documentation for information.", "OK");
                    }
                    GUILayout.Space(8);
                }
                GUILayout.Space(11);
            }
            #endif
            
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Quality/Performance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(allowBlur);
            EditorGUILayout.PropertyField(allowDistortion);

            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(distortionMode);

            EditorGUILayout.Space();
                
            EditorGUILayout.PropertyField(directionalCaustics);
            if (directionalCaustics.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(accurateDirectionalCaustics);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(waterlineRefraction);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (resources.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Internal shader resources object not referenced!", MessageType.Error);
                if (GUILayout.Button("Find & assign"))
                {
                    resources.objectReferenceValue = UnderwaterResources.Find();
                    serializedObject.ApplyModifiedProperties();
                }
            }
            
            UI.DrawFooter();
        }
    }
}