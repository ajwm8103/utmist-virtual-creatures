using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreatureSpawner))]
public class CreatureSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CreatureSpawner spawner = target as CreatureSpawner;

        if (GUILayout.Button("Save Current Creature"))
        {
            Debug.Log("Saving Current Creature");
            string path = EditorUtility.SaveFilePanel("Save Creature As", "C:", "Creature.creature", "creature");
            CreatureGenotype cg = spawner.creatureGenotype;
            cg.SaveData(path, true);
            Debug.Log(Application.persistentDataPath);
        }

        if (GUILayout.Button("Load Creature Genotype"))
        {
            Debug.Log("Loading Creature");
            string path = EditorUtility.OpenFilePanel("Creature.creature", "C:", "creature");
            CreatureGenotype cg = CreatureGenotype.LoadData(path, true);
            spawner.creatureGenotype = cg;
        }

        if (GUILayout.Button("Spawn Creature"))
        {
            Debug.Log("Spawning Creature");
            spawner.SpawnCreature(spawner.creatureGenotype, Vector3.up + Vector3.right, null);
        }
    }
}
