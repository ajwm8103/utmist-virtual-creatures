using System.Collections;
using System;
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
    public virtual float maxTime { get { return 3; } }
}

/// <summary>
/// Base class to control a single environment
/// </summary>
public abstract class Environment : MonoBehaviour
{

    [Header("Stats")]
    public EnvCode envCode;
    public float totalReward;
    public float timePassed;
    public bool busy { get { return currentCreature != null; } }

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
    [SerializeField]
    private Transform creatureHolder;

    private EnvironmentSettings es;
    public List<TrainingAlgorithm> tas;

    public virtual void Setup(EnvironmentSettings es)
    {
        this.es = es;
        fitness = GetComponent<Fitness>();
        tm = TrainingManager.instance;
        cs = CreatureSpawner.instance;
        spawnTransform = transform.Find("SpawnTransform");
        creatureHolder = transform.Find("CreatureHolder");

        ResetEnv();
    }

    public virtual void FixedUpdate()
    {
        
        timePassed += Time.fixedDeltaTime;

        if (timePassed >= es.maxTime && es.maxTime > 0)
        {
            //m_BlueAgentGroup.GroupEpisodeInterrupted();
            //m_RedAgentGroup.GroupEpisodeInterrupted();

            //m_blueAgent.agent.EpisodeInterrupted();
            //m_redAgent.agent.EpisodeInterrupted();
            ResetEnv();
        }
    }

    // Spawn creature by passing transform params to Scene CreatureSpawner
    public virtual void StartEnv(CreatureGenotype cg)
    {
        if (busy)
        {
            ResetEnv();
        }
        currentCreature = cs.SpawnCreature(cg, spawnTransform.position);
        currentCreature.transform.parent = creatureHolder;
    }
    public virtual void ResetEnv()
    {
        if (tas != null){
            foreach (TrainingAlgorithm ta in tas)
            {
                Debug.Log("Pinging training algorithm.");
                ta.ResetPing(this, totalReward);
            }
        }

        tas = new List<TrainingAlgorithm>();

        if (busy) {
            Destroy(currentCreature.gameObject);
            currentCreature = null;
        }

        timePassed = 0;
        totalReward = 0;
    }

    public void PingReset(TrainingAlgorithm ta){
        tas.Add(ta);
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow cube at the transform position
        if (es == null)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(es.sizeX, 10, es.sizeZ));
    }
}
