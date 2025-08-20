using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StylizedWater2.UnderwaterRendering
{
	[CustomEditor(typeof(UnderwaterTrigger))]
	class UnderwaterTriggerEditor : Editor
	{
		private SerializedProperty triggerTag;
		
		private SerializedProperty toggleRendering;
		private SerializedProperty waterMaterial;
		private SerializedProperty changeWaterLevel;
		private SerializedProperty useTransformForWaterlevel;
		private SerializedProperty waterLevel;

		private void OnEnable()
		{
			triggerTag = serializedObject.FindProperty("triggerTag");
			
			toggleRendering = serializedObject.FindProperty("toggleRendering");
			waterMaterial = serializedObject.FindProperty("waterMaterial");
			changeWaterLevel = serializedObject.FindProperty("changeWaterLevel");
			useTransformForWaterlevel = serializedObject.FindProperty("useTransformForWaterlevel");
			waterLevel = serializedObject.FindProperty("waterLevel");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();

			Rect rect = EditorGUILayout.GetControlRect();
			triggerTag.stringValue = EditorGUI.TagField(rect, new GUIContent(triggerTag.displayName, triggerTag.tooltip), triggerTag.stringValue);

			GameObject trigger = GameObject.FindGameObjectWithTag(triggerTag.stringValue);

			if (!trigger)
			{
				EditorGUILayout.HelpBox($"No GameObject found with the tag \"{triggerTag.stringValue}\"", MessageType.Error);
			}
			else
			{
				EditorGUILayout.HelpBox($"GameObject found with tag: {trigger.name}", MessageType.Info);
				
				Collider collider = trigger.GetComponent<Collider>();
				if (collider == null)
				{
					EditorGUILayout.HelpBox("Trigger object does not have a collider component added to it", MessageType.Error);
				}
				else
				{
					if (!collider.isTrigger) EditorGUILayout.HelpBox("Trigger object collider has the \"Is Trigger\" checkbox disabled", MessageType.Error);
				}

				Rigidbody rb = trigger.GetComponent<Rigidbody>();
				if (!rb)
				{
					EditorGUILayout.HelpBox("Trigger object does not have a Rigidbody component added to it", MessageType.Error);
				}
				else
				{
					if (!rb.isKinematic) EditorGUILayout.HelpBox("Trigger object Rigidbody has the \"Is Kinematic\" checkbox disabled, expect it to fall.", MessageType.Error);
				}
			}
			
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(toggleRendering);
			if(toggleRendering.boolValue) EditorGUILayout.HelpBox($"Current state. Rendering enabled: {UnderwaterRenderer.EnableRendering}", MessageType.None);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(waterMaterial);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(changeWaterLevel);
			if (changeWaterLevel.boolValue)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(useTransformForWaterlevel);
				if (useTransformForWaterlevel.boolValue == false) EditorGUILayout.PropertyField(waterLevel);
				EditorGUI.indentLevel--;
			}

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}