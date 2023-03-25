using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrainingSave {
    public string saveName = "Unnamed Save";
    public string savePath;
    public bool isNew;
    public TrainingSettings ts;
}

[System.Serializable]
public class TrainingSettings {
    public OptimizationSettings optimizationSettings;
    public EnvironmentSettings envSettings;

    public TrainingSettings(OptimizationSettings os, EnvironmentSettings es){
        optimizationSettings = os;
        envSettings = es;
    }
}

[System.Serializable]
public abstract class OptimizationSettings {
    public int num_envs = 1;
    public CreatureGenotype initialGenotype;
    public abstract TrainingStage stage { get; }
}

[System.Serializable]
public class RLSettings : OptimizationSettings {
    public override TrainingStage stage { get {return TrainingStage.RL; }}
}

[System.Serializable]
public class KSSSettings : OptimizationSettings {
    public override TrainingStage stage { get { return TrainingStage.KSS; } }
    public int populationSize = 50;
    public int totalGenerations = 10;
    public MutateGenotype.MutationPreferenceSetting mp;
}

/// <summary>
/// Generates Environments, tallies data, Starts and Stops training
/// </summary>
public class TrainingManager : MonoBehaviour
{
    public static TrainingManager instance;

    public List<Environment> environments { get; private set; }

    public TrainingSave save { get; private set; }
    [SerializeField]
    private TrainingSettings ts;
    [SerializeField]
    private TrainingStage stage;

    // References to components
    [SerializeField]
    private TrainingAlgorithm algo;
    private Transform envHolder;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two controllers active at once
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        EvolutionSettingsPersist esp = EvolutionSettingsPersist.instance;
        if (esp == null)
        {
            throw new Exception("No EvolutionSettingsPersist instance found. Try launching from the Menu Scene!");
        }

        save = esp.save;
        Debug.Log(save);
        ts = save.ts;
        stage = ts.optimizationSettings.stage;
        envHolder = transform.Find("EnvHolder");
        if (envHolder == null){
            envHolder = new GameObject().transform;
            envHolder.transform.parent = transform;
            envHolder.transform.localPosition = Vector3.zero;
            envHolder.name = "EnvHolder";
        }

        GameObject envPrefab = Resources.Load<GameObject>("Prefabs/Envs/" + EnvironmentSettings.envString[ts.envSettings.envCode]);
        environments = new List<Environment>();

        if (ts.envSettings.envArrangeType == EnvArrangeType.LINEAR){
            float sizeX = ts.envSettings.sizeX;
            int l = ts.optimizationSettings.num_envs;
            for (int i = 0; i < l; i++)
            {
                Environment instantiatedEnv = Instantiate(envPrefab, Vector3.right * i * sizeX, envPrefab.transform.rotation).GetComponent<Environment>();
                instantiatedEnv.Setup(ts.envSettings);
                instantiatedEnv.transform.parent = envHolder;
                environments.Add(instantiatedEnv);
                Transform oneOff = instantiatedEnv.transform.Find("OneOffHolder");
                if (oneOff != null && i != 0) {
                    Destroy(oneOff.gameObject);
                }
            }
        } else if (ts.envSettings.envArrangeType == EnvArrangeType.PLANAR){
            // TODO lol
        }

        if (stage == TrainingStage.KSS) {
            algo = (TrainingAlgorithm)gameObject.AddComponent(typeof(KSS.KSSAlgorithm));
            algo.Setup(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}