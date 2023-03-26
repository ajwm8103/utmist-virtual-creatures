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
    public GameObject evolutionSettingsMenu;
    public GameObject evolutionLSMenu;

    private List<GameObject> menus;

    // Start is called before the first frame update
    void Start()
    {
        menus = new List<GameObject>() { mainMenu, evolutionSettingsMenu, evolutionLSMenu };
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

    public void ShowEvolutionSettingsMenu()
    {
        menus.ForEach(o => o.SetActive(false));
        evolutionSettingsMenu.SetActive(true);
    }

    public void ShowEvolutionLSMenu()
    {
        menus.ForEach(o => o.SetActive(false));
        evolutionLSMenu.SetActive(true);
    }

    public void LoadLocal()
    {
        // Compile data from settings window into TrainingSave
        KSSSettings optimizationSettings = new KSSSettings();
        optimizationSettings.num_envs = 25;
        optimizationSettings.mp = new MutateGenotype.MutationPreferenceSetting();
        optimizationSettings.initialGenotype = CreatureGenotype.LoadData("/Fish.creature", false); // null means start w/ random creatures. TODO: Non-null will mean spawn that with mutations!
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
