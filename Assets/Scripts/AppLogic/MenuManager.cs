using System;
using System.IO;
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
    public GameObject saveTitle;
    public GameObject saveTitleInput;
    public InputField populationInput;
    public InputField generationInput;
    public InputField ratioNumeratorInput;
    public InputField ratioDenominatorInput;
    public Toggle lockNeuralMutationsToggle;
    public Toggle lockPhysicalMutationsToggle;

    public string saveName;

    // evolution settings values
    private CreatureGenotype initialGenotype;
    private int populationSize;
    private int totalGenerations;
    private float ratioNumerator;
    private float ratioDenominator;
    private bool lockNeuralMutations;
    private bool lockPhysicalMutations;

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

        string[] fileArray = Directory.GetFiles(OptionsPersist.instance.VCSaves, "*.save");

        foreach (string filePath in fileArray)
        {
            Debug.Log(filePath);
        }

        chooseEvolution.SetActive(true);
    }

    public void ShowEvolutionSettingsMenu()
    {
        menus.ForEach(o => o.SetActive(false));

        populationSize = int.Parse(populationInput.text);
        totalGenerations = int.Parse(generationInput.text);

        ratioNumerator = float.Parse(ratioNumeratorInput.text);

        ratioDenominator = float.Parse(ratioDenominatorInput.text);

        lockNeuralMutations = lockNeuralMutationsToggle.isOn;

        lockPhysicalMutations = lockPhysicalMutationsToggle.isOn;

        evolutionSettingsMenu.SetActive(true);
}


    public void EditSaveName()
    {
        saveTitle.SetActive(false);
        saveTitleInput.SetActive(true);
    }

    public void SetSaveName(string input)
    {
        saveName = input;
        saveTitle.SetActive(true);
        saveTitle.GetComponent<Text>().text = input;
        saveTitleInput.SetActive(false);
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
        optimizationSettings.num_envs = 75;
        optimizationSettings.mp = new MutateGenotype.MutationPreferenceSetting();
        optimizationSettings.mp.mutateNeural = !lockNeuralMutations;
        optimizationSettings.mp.mutateMorphology = !lockPhysicalMutations;
        optimizationSettings.initialGenotype = templateCGSO == null ? null : templateCGSO.cg;
        //optimizationSettings.initialGenotype = CreatureGenotype.LoadData("/Fish.creature", false); // null means start w/ random creatures. TODO: Non-null will mean spawn that with mutations!
        
        //TrainingSettings ts = new TrainingSettings(optimizationSettings, new OceanEnvSettings());
        TrainingSettings ts = new TrainingSettings(optimizationSettings, new FloorEnvSettings());


        KSSSave save = new KSSSave();
        save.isNew = true;
        save.ts = ts;
        save.saveName = saveName;

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
