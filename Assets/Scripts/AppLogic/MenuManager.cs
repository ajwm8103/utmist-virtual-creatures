using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        // Compile data from settings window into TrainingSettings
        TrainingSettings ts = new TrainingSettings(new OptimizationSettings(), new OceanEnvSettings());
        ts.optimizationSettings.num_envs = 10;

        // Send to EvolutionSettingsPersist
        EvolutionSettingsPersist esp = EvolutionSettingsPersist.instance;
        if (esp == null)
        {
            throw new Exception("No EvolutionSettingsPersist instance found.");
        }
        esp.ts = ts;
        esp.cg = null; // null means start w/ random creatures. TODO: Non-null will mean spawn that with mutations!

        // Load env runner
        SceneManager.LoadScene("LocalEnvRunner");
    }
}
