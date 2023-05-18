using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatureGenotypeScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CreatureGenotypeScriptableObject so = target as CreatureGenotypeScriptableObject;

        if (GUILayout.Button("Save Current Creature Genotype"))
        {
            Debug.Log("Saving Current Creature");
            string path = EditorUtility.SaveFilePanel("Save Creature As", "C:", "Creature.creature", "creature");
            CreatureGenotype cg = so.cg;
            cg.SaveData(path, true);
            Debug.Log(Application.persistentDataPath);
        }

        if (GUILayout.Button("Load Creature Genotype"))
        {
            Debug.Log("Loading Creature");
            string path = EditorUtility.OpenFilePanel("Creature.creature", "C:", "creature");
            CreatureGenotype cg = CreatureGenotype.LoadData(path, true);
            so.cg = cg;
        }
        DrawDefaultInspector();
    }
}
