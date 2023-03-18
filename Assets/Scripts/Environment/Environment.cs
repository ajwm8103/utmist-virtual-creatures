using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public enum EnvArrangeType { LINEAR, PLANAR };
public enum GraphicsLevel { LOW, MEDIUM, HIGH };
public enum EnvCode { OCEAN };

[System.Serializable]
public abstract class EnvironmentSettings {
    public abstract EnvCode envCode { get; }
    public readonly static Dictionary<EnvCode, string> envString = new Dictionary<EnvCode, string>() { { EnvCode.OCEAN, "OceanEnv" } };
    public abstract EnvArrangeType envArrangeType { get; }
    public GraphicsLevel graphicsLevel { get; set;  }

    public virtual float sizeX { get { return 5; } }
    public virtual float sizeZ { get { return 5; } }
}

/// <summary>
/// Base class to control a single environment
/// </summary>
public abstract class Environment : MonoBehaviour
{
    [Header("Stats")]
    public EnvCode envCode;
    public float totalReward;
    public bool busy;

    // References to other Components
    [SerializeField]
    private Creature currentCreature;
    [SerializeField]
    private Fitness fitness;
    [SerializeField]
    private TrainingManager tm;
    [SerializeField]
    private CreatureSpawner cs;
    [SerializeField]
    private Transform spawnTransform;

    private EnvironmentSettings es;

    public void Setup(EnvironmentSettings es){
        this.es = es;
        fitness = GetComponent<Fitness>();
        tm = TrainingManager.instance;
        spawnTransform = transform.Find("Spawn Transform");
        ResetEnv();
    }

    // Spawn creature by passing transform params to Scene CreatureSpawner
    public virtual void SpawnCreature(CreatureGenotype cg){
        cs.SpawnCreature(cg, spawnTransform.position);
    }
    public virtual void ResetEnv() {
        totalReward = 0;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow cube at the transform position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(es.sizeX, 1, es.sizeZ));
    }
}
