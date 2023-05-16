using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using KSS;

//public enum CurrentMenu { MAIN, EVOLUTION_SETTINGS, EVOLUTION_LS, EVOLUTION_SERVER}
public class MenuManager : MonoBehaviour
{
    //[SerializeField]
    //private CurrentMenu currentMenu = CurrentMenu.MAIN;

    [Header("References")]
    public GameObject mainMenu;
    public GameObject chooseEvolution;
    public GameObject evolutionSettingsMenu;
    // TEMP
    public CreatureGenotypeScriptableObject templateCGSO;

    private List<GameObject> menus;

    // settings ui
    public InputField populationField;
    public InputField generationField;
    public InputField ratioNumeratorField;
    public InputField ratioDenominatorField;
    public Toggle lockNeuralMutationsToggle;
    public Toggle lockPhysicalMutationsToggle;

    // evolution settings values
    private CreatureGenotype initialGenotype;
    //private int populationSize;
    //private int totalGenerations;
    //private float survivalRatioNumerator;
    //private float survivalRatioDenominator;
    //private bool lockNeuralMutations;
    //private bool lockPhysicalMutations;
    public int populationSize;
    public int totalGenerations;
    public float ratioNumerator;
    public float ratioDenominator;
    public bool lockNeuralMutations;
    public bool lockPhysicalMutations;

    // Start is called before the first frame update
    void Start()
    {
        menus = new List<GameObject>() { mainMenu, chooseEvolution, evolutionSettingsMenu};
        ShowMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowEdit()
    {
        Debug.Log("Show edit scene");
    }

    public void ShowView()
    {
        Debug.Log("Show view scene");
    }

    public void ShowMainMenu()
    {
        menus.ForEach(o => o.SetActive(false));
        mainMenu.SetActive(true);
    }

    public void ShowChooseEvolutionMenu()
    {
        menus.ForEach(o => o.SetActive(false));
        chooseEvolution.SetActive(true);
    }

    public void ShowEvolutionSettingsMenu()
    {
        menus.ForEach(o => o.SetActive(false));
        evolutionSettingsMenu.SetActive(true);
    }


    public void SetPopulationSize(string input)
    {
        populationSize = int.Parse(input);
    }

    public void SetTotalGenerations(string input)
    {
        totalGenerations = int.Parse(input);
    }

    public void SetSurvivalRatioNumerator(string input)
    {
        ratioNumerator = float.Parse(input);
    }

    public void SetSurvivalRatioDenominator(string input)
    {
        ratioDenominator = float.Parse(input);
    }

    public void LockNeuralMutation(bool input)
    {
        lockNeuralMutations = input;
    }

    public void LockPhysicalMutation(bool input)
    {
        lockPhysicalMutations = input;
    }


    public void LoadLocal()
    {
        // Compile data from settings window into TrainingSave
        KSSSettings optimizationSettings = new KSSSettings(populationSize, totalGenerations, ratioNumerator / ratioDenominator);
        optimizationSettings.num_envs = 50;
        optimizationSettings.mp = new MutateGenotype.MutationPreferenceSetting();
        optimizationSettings.mp.mutateNeural = !lockNeuralMutations;
        optimizationSettings.mp.mutateMorphology = !lockPhysicalMutations;
        optimizationSettings.initialGenotype = templateCGSO == null ? null : templateCGSO.cg;
        //optimizationSettings.initialGenotype = CreatureGenotype.LoadData("/Fish.creature", false); // null means start w/ random creatures. TODO: Non-null will mean spawn that with mutations!
        TrainingSettings ts = new TrainingSettings(optimizationSettings, new OceanEnvSettings());
        KSSSave save = new KSSSave();
        save.isNew = true;
        save.ts = ts;

        // Send to EvolutionSettingsPersist
        EvolutionSettingsPersist esp = EvolutionSettingsPersist.instance;
        if (esp == null)
        {
            throw new Exception("No EvolutionSettingsPersist instance found.");
        }
        esp.save = save;

        // Load env runner
        SceneManager.LoadScene("LocalEnvRunner");
    }
}
