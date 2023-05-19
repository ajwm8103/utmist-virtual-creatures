using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class SelectedCreaturePanel : MonoBehaviour
{
    public static SelectedCreaturePanel instance;
    private CreatureSpawner creatureSpawner;
    private Creature currentCreatureClone;
    public Transform selectedCreatureContainer;
    private int cloneLayer;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two managers active at once
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        creatureSpawner = CreatureSpawner.instance;
        cloneLayer = LayerMask.NameToLayer("CreatureClone");
    }

    public void SaveCreature()
    {
        if (currentCreatureClone == null) return;

        string path = Path.Combine(OptionsPersist.instance.VCCreatures, currentCreatureClone.cg.name + ".creature");
        currentCreatureClone.cg.SaveData(path, true);
        Debug.Log(string.Format("Saved {0} to {1}", currentCreatureClone.cg.name, path));
    }

    public void UpdateClonedCreature(CreatureGenotype creatureGenotypeToClone){
        if (currentCreatureClone == null || currentCreatureClone.cg != creatureGenotypeToClone) {
            // Destroy previously cloned creature
            if (currentCreatureClone != null)
            {
                Destroy(currentCreatureClone.gameObject);
            }

            // Instantiate new creature
            Creature creatureClone = creatureSpawner.SpawnCreature(creatureGenotypeToClone, Vector3.zero);
            creatureClone.transform.parent = selectedCreatureContainer;

            // Make segments non-colliding and non-rendering
            foreach (Segment s in creatureClone.segments)
            {
                s.gameObject.GetComponent<Rigidbody>().detectCollisions = false;
                s.gameObject.layer = cloneLayer;
                s.transform.Find("Graphic").gameObject.layer = cloneLayer;
            }

            // Disable creature neurons and motion
            creatureClone.SetAlive(false);
            currentCreatureClone = creatureClone;

            // Target camera to creature
            CreatureViewerController.instance.SetCreature(currentCreatureClone);
        }
    }
}
