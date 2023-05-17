using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public enum EnvArrangeType { LINEAR, PLANAR };
public enum GraphicsLevel { LOW, MEDIUM, HIGH };
public enum EnvCode { OCEAN };

[Serializable]
public abstract class EnvironmentSettings {
    public abstract EnvCode envCode { get; }
    public readonly static Dictionary<EnvCode, string> envString = new Dictionary<EnvCode, string>() { { EnvCode.OCEAN, "OceanEnv" } };
    public abstract EnvArrangeType envArrangeType { get; }
    public GraphicsLevel graphicsLevel { get; set;  }

    public virtual float sizeX { get { return 5; } }
    public virtual float sizeZ { get { return 5; } }
    public virtual float maxTime { get { return 3; } }
    public static EnvironmentSettings GetDefault(EnvCode code){
        if (code == EnvCode.OCEAN) {
            return new OceanEnvSettings();
        }
        return null;
    }
}

/// <summary>
/// Base class to control a single environment
/// </summary>
public abstract class Environment : MonoBehaviour
{

    [Header("Stats")]
    public float timePassed;
    public abstract EnvCode envCode { get; }
    public bool busy { get { return currentCreature != null; } }

    private bool updatedFrameReward;
    private float frameReward;
    private bool isDQ = false;
    private bool hasDoneKillCheck = false;
    private bool isStandalone; // true when just testing one
    private Vector3 lastCom;

    // References to other Components
    public Creature currentCreature;
    [SerializeField]
    protected Fitness fitness;
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

    public void Start()
    {
        tm = TrainingManager.instance;
        isStandalone = tm == null;
        if (isStandalone) {
            Setup(EnvironmentSettings.GetDefault(envCode));
        }
    }

    public virtual void Setup(EnvironmentSettings es)
    {
        this.es = es;
        fitness = GetComponent<Fitness>();
        //fitness.firstFrame = true;
        tm = TrainingManager.instance;
        cs = CreatureSpawner.instance;
        hasDoneKillCheck = false;
        spawnTransform = transform.Find("SpawnTransform");
        creatureHolder = transform.Find("CreatureHolder");
    }

    public virtual void FixedUpdate()
    {
        if (!busy) return;
        if (tm == null) tm = TrainingManager.instance;

        timePassed += Time.fixedDeltaTime;
        bool isOutOfTime;
        try
        {
            isOutOfTime = timePassed >= es.maxTime && es.maxTime > 0;
        }
        catch (Exception)
        {
            if (isStandalone) {
                es = EnvironmentSettings.GetDefault(envCode);
            } else {
                es = tm.ts.envSettings;
            }

            isOutOfTime = timePassed >= es.maxTime && es.maxTime > 0;
        }
        Vector3 currentCom = currentCreature.GetCentreOfMass();
        bool isExtremelyFar = (transform.position - currentCom).sqrMagnitude >= 1000;
        if (lastCom == null) {
            lastCom = currentCom;
        }



        bool isTooFast = ((currentCom - lastCom).magnitude / Time.fixedDeltaTime) > 10f && timePassed > 0.2f;
        bool isNan = !float.IsNaN(currentCom.x) || !float.IsNaN(currentCom.y) || !float.IsNaN(currentCom.z);
        bool isDQActivate = isExtremelyFar || isTooFast;
        bool isTooSlow = false;
        if (timePassed > 0.8f && !hasDoneKillCheck){
            hasDoneKillCheck = true;
            isTooSlow = Mathf.Abs(currentCreature.totalReward) < 0.00005f;
        }
        
        if (isOutOfTime || isDQActivate || isTooSlow)
        {
            if (isDQActivate)
            {
                isDQ = true;
            }
            //m_BlueAgentGroup.GroupEpisodeInterrupted();
            //m_RedAgentGroup.GroupEpisodeInterrupted();

            //m_blueAgent.agent.EpisodeInterrupted();
            //m_redAgent.agent.EpisodeInterrupted();
            ResetEnv();
        }
        //Debug.Log(currentCom + " " + lastCom);
        lastCom = currentCom;
    }

    // Spawn creature by passing transform params to Scene CreatureSpawner
    public virtual void StartEnv(CreatureGenotype cg)
    {
        if (busy)
        {
            ResetEnv();
        }
        currentCreature = cs.SpawnCreature(cg, spawnTransform.position, fitness);
        currentCreature.transform.parent = creatureHolder;

        fitness.Reset();
    }
    public virtual void ResetEnv()
    {
        if (tas != null){
            foreach (TrainingAlgorithm ta in tas)
            {
                //Debug.Log("Pinging training algorithm.");
                float totalReward = busy ? currentCreature.totalReward : 0;
                ta.ResetPing(this, totalReward, isDQ);
            }
        }

        tas = new List<TrainingAlgorithm>();

        if (busy) {
            Destroy(currentCreature.gameObject);
            currentCreature = null;
        }

        isDQ = false;
        hasDoneKillCheck = false;
        timePassed = 0;
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
