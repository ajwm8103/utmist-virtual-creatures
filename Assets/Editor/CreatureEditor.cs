using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Creature))]
public class CreatureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Creature creature = target as Creature;

        if (GUILayout.Button("Save Current Creature"))
        {
            Debug.Log("Saving Current Creature");
            CreatureGenotype cg = creature.cg;
            string path = EditorUtility.SaveFilePanel("Save Your Creature", "C:", "Creature.creature", "creature");
            if (!string.IsNullOrEmpty(path))
            {
                cg.SaveData(path, true, true);
                Debug.Log("Saved to " + Application.persistentDataPath);
            }
        }
    }
}
