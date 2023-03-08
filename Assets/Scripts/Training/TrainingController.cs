using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingSettings {
    public OptimizationSettings optimizationSettings;
    public EnvironmentSettings
}

public class OptimizationSettings {

}

public class RLSettings : OptimizationSettings {

}

public class KSSSettings : OptimizationSettings {

}

/// <summary>
/// Generates Environments, tallies data, Starts and Stops training
/// </summary>
public class TrainingController : MonoBehaviour
{
    [SerializeField]
    private TrainingSettings ts;

    // Start is called before the first frame update
    void Start()
    {
        if (EvolutionSettingsPersist.instance != null)
        {
            ts = EvolutionSettingsPersist.instance.ts;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
