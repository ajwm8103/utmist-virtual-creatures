using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureViewerController : MonoBehaviour
{
    [Header("Preferences")]
    public float rotatePeriod = 2f;
    public Creature currentCreature;

    public static CreatureViewerController instance;

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

    // Update is called once per frame
    void Update()
    {
        if (currentCreature == null) return;

        // TODO: Spawn replica creature, still, at some other location and track that
        Vector3 position = Vector3.zero;
        position.x = 2f * Mathf.Cos(Time.time * 0.5f * Mathf.PI / rotatePeriod);
        position.y = 1f;
        position.z = 2f * Mathf.Sin(Time.time * 0.5f * Mathf.PI / rotatePeriod);
        Vector3 creatureCOM = currentCreature.GetCentreOfMass();
        position += creatureCOM;
        transform.position = position;

        transform.LookAt(creatureCOM);
    }

    public void SetCreature(Creature creature){
        currentCreature = creature;
    }
}
