using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class TrainingSave {
    public string saveName = "Unnamed Save";
    public string savePath;
    public bool isNew;
    public TrainingSettings ts;

    public void SaveData(string path, bool isFullPath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string fullPath = isFullPath ? path : Application.persistentDataPath + path;

        FileStream stream = new FileStream(fullPath, FileMode.Create);

        formatter.Serialize(stream, this);
        stream.Close();
    }

    public static TrainingSave LoadData(string path, bool isFullPath)
    {
        string fullPath = isFullPath ? path : Application.persistentDataPath + path;

        if (File.Exists(fullPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(fullPath, FileMode.Open);

            TrainingSave data = formatter.Deserialize(stream) as TrainingSave;

            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Error: Save file not found in " + fullPath);
            return null;
        }
    }
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
    public int populationSize = 300;
    public int totalGenerations = 200;
    public float survivalRatio = 1f / 5f;
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
    public TrainingSettings ts { get; private set; }
    [SerializeField]
    private TrainingStage stage;

    // test
    public CreatureGenotype creatureGenotype;

    // References to components
    public Text statsText;
    private TrainingAlgorithm algo;
    private Transform envHolder;

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
        creatureGenotype = ts.optimizationSettings.initialGenotype;
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
                instantiatedEnv.gameObject.name += i.ToString();
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

    public void SaveTraining(){
        algo.SaveTraining();
    }

    public Creature GetBestLivingCreature()
    {
        try
        {
            return environments.OrderByDescending(x => x.currentCreature.totalReward).First().currentCreature;
        }
        catch (Exception)
        {

            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
