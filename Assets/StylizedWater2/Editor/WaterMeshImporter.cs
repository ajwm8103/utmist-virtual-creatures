//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

using System.IO;
using UnityEngine;
using UnityEditor;

namespace StylizedWater2
{
    [ScriptedImporter(1, "watermesh")]
    public class WaterMeshImporter : ScriptedImporter
    {
		[UnityEngine.Serialization.FormerlySerializedAs("plane")]
        [SerializeField] public WaterMesh waterMesh;
        [HideInInspector] private Mesh mesh;

        public override void OnImportAsset(AssetImportContext context)
        {
            if (waterMesh == null)
            {
                waterMesh = new WaterMesh();
                mesh = waterMesh.Rebuild();
            }
            else
            {
                mesh = waterMesh.Rebuild();
            }

            context.AddObjectToAsset("mesh", mesh);
            context.SetMainObject(mesh);
        }
        
        //Handles correct behaviour when double-clicking a .watermesh asset assigned to a field
        //Otherwise the OS prompts to open it
        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Object target = EditorUtility.InstanceIDToObject(instanceID);

            if (target is Mesh)
            {
                var path = AssetDatabase.GetAssetPath(instanceID);
                
                if (Path.GetExtension(path) != ".watermesh") return false;

                Selection.activeObject = target;
                return true;
            }
            
            return false; // we did not handle the open
        }

    }
	
	[CustomEditor(typeof(WaterMeshImporter))]
    public class WaterMeshImporterEditor: ScriptedImporterEditor
    {
        private WaterMeshImporter importer;
		private bool autoApplyChanges;

        public override void OnEnable()
        {
			base.OnEnable();
			
            importer = (WaterMeshImporter)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
			
			autoApplyChanges = EditorGUILayout.Toggle("Auto-apply changes", autoApplyChanges);
			
			int vertexCount = Mathf.FloorToInt(importer.waterMesh.subdivisions * importer.waterMesh.subdivisions);
            if(vertexCount > 65535)
            {
                EditorGUILayout.HelpBox("Vertex count (" + vertexCount + ") is too high. Decrease the amount of subdivisions", MessageType.Error);
            }

            if (autoApplyChanges && HasModified())
            {
				this.Apply();     
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.assetTarget));
            }
        }
    }
}
