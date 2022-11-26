using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace StylizedWater2
{
    [CustomEditor(typeof(PlanarReflectionRenderer))]
    public class PlanarReflectionRendererInspector : Editor
    {
        private PlanarReflectionRenderer renderer;
        
        //Rendering
        private SerializedProperty cullingMask;
        private SerializedProperty rendererIndex;
        private SerializedProperty offset;
        private SerializedProperty includeSkybox;
        
        //Quality
        private SerializedProperty renderShadows;
        private SerializedProperty renderRange;
        private SerializedProperty renderScale;
        private SerializedProperty maximumLODLevel;
        
        private SerializedProperty waterObjects;
        private SerializedProperty moveWithTransform;

        private Bounds curBounds;
        private bool waterLayerError;

        private void OnEnable()
        {
#if URP
            PipelineUtilities.RefreshRendererList();
            
            renderer = (PlanarReflectionRenderer)target;

            cullingMask = serializedObject.FindProperty("cullingMask");
            rendererIndex = serializedObject.FindProperty("rendererIndex");
            offset = serializedObject.FindProperty("offset");
            includeSkybox = serializedObject.FindProperty("includeSkybox");
            renderShadows = serializedObject.FindProperty("renderShadows");
            renderRange = serializedObject.FindProperty("renderRange");
            renderScale = serializedObject.FindProperty("renderScale");
            maximumLODLevel = serializedObject.FindProperty("maximumLODLevel");
            waterObjects = serializedObject.FindProperty("waterObjects");
            moveWithTransform = serializedObject.FindProperty("moveWithTransform");
            
            if (renderer.waterObjects.Count == 0 && WaterObject.Instances.Count == 1)
            {
                renderer.waterObjects.Add(WaterObject.Instances[0]);
                renderer.RecalculateBounds();
                renderer.EnableMaterialReflectionSampling();
                
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            ValidateWaterObjectLayer();

            curBounds = renderer.CalculateBounds();
#endif
        }

        public override void OnInspectorGUI()
        {
#if !URP
            UI.DrawNotification("The Universal Render Pipeline package v" + AssetInfo.MIN_URP_VERSION + " or newer is not installed", MessageType.Error);
#else
            UI.DrawNotification(UnityEngine.Rendering.XRGraphics.enabled, "Not supported with VR rendering", MessageType.Error);
            
            UI.DrawNotification(PlanarReflectionRenderer.AllowReflections == false, "Reflections have been globally disabled by an external script", MessageType.Warning);
            
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status: " + (renderer.isRendering ? "Rendering (water in view)" : "Not rendering (no water in view)"), EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(cullingMask);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.BeginChangeCheck();
            UI.DrawRendererProperty(rendererIndex);
            if (EditorGUI.EndChangeCheck())
            {
                renderer.SetRendererIndex(rendererIndex.intValue);
            }
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(includeSkybox);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Quality", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(renderShadows);
            if (EditorGUI.EndChangeCheck())
            {
                renderer.ToggleShadows(renderShadows.boolValue);
            }
            EditorGUILayout.PropertyField(renderRange);
            EditorGUILayout.PropertyField(renderScale);
            EditorGUILayout.PropertyField(maximumLODLevel);
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Target water objects", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(moveWithTransform, new GUIContent("Move bounds with transform", moveWithTransform.tooltip));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(waterObjects);
            if (EditorGUI.EndChangeCheck())
            {
                curBounds = renderer.CalculateBounds();
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(new GUIContent("Auto-find", "Assigns all active water objects currently in the scene"), EditorStyles.miniButton))
                {
                    renderer.waterObjects = new List<WaterObject>(WaterObject.Instances);
 
                    renderer.RecalculateBounds();
                    curBounds = renderer.bounds;
                    renderer.EnableMaterialReflectionSampling();

                    ValidateWaterObjectLayer();
                    
                    EditorUtility.SetDirty(target);
                }
                if(GUILayout.Button("Clear", EditorStyles.miniButton))
                {
                    renderer.ToggleMaterialReflectionSampling(false);
                    renderer.waterObjects.Clear();
                    renderer.RecalculateBounds();
                    
                    EditorUtility.SetDirty(target);
                }
            }
            
            if (renderer.waterObjects != null)
            {
                UI.DrawNotification(renderer.waterObjects.Count == 0, "Assign at least one Water Object", MessageType.Info);
                
                if (renderer.waterObjects.Count > 0)
                {
                    UI.DrawNotification(curBounds.size != renderer.bounds.size || (moveWithTransform.boolValue == false && curBounds.center != renderer.bounds.center), "Water objects have changed or moved, bounds needs to be recalculated", "Recalculate",() => RecalculateBounds(), MessageType.Error);
                }
                
                UI.DrawNotification(waterLayerError, "One or more Water Objects aren't on the \"Water\" layer.\n\nThis causes recursive reflections", "Fix", () => SetObjectsOnWaterLayer(), MessageType.Error);
            }

#endif
            
            UI.DrawFooter();
        }
        
#if URP
        private void ValidateWaterObjectLayer()
        {
            if (renderer.waterObjects == null) return;

            waterLayerError = false;
            int layerID = LayerMask.NameToLayer("Water");

            foreach (WaterObject obj in renderer.waterObjects)
            {
                //Is not on "Water" layer?
                if (obj.gameObject.layer != layerID)
                {
                    waterLayerError = true;
                    return;
                }
            }
        }

        private void SetObjectsOnWaterLayer()
        {
            int layerID = LayerMask.NameToLayer("Water");

            foreach (WaterObject obj in renderer.waterObjects)
            {
                //Is not on "Water" layer?
                if (obj.gameObject.layer != layerID)
                {
                    obj.gameObject.layer = layerID;
                    EditorUtility.SetDirty(obj);
                }
            }
            
            waterLayerError = false;
        }
    #endif

        private void RecalculateBounds()
        {
#if URP
            renderer.RecalculateBounds();
            curBounds = renderer.bounds;
            EditorUtility.SetDirty(target);
#endif
        }
    }
}
