using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionSettingsPersist : MonoBehaviour
{
    public static EvolutionSettingsPersist instance;

    // Vars here
    public TrainingSettings ts;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two persists active at once
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
