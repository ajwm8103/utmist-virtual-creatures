using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum CurrentMenu { MAIN, EVOLUTION_SETTINGS, EVOLUTION_LS, EVOLUTION_SERVER}
public class MenuManager : MonoBehaviour
{
    [Header("Info")]
    [SerializeField]
    private CurrentMenu currentMenu = CurrentMenu.MAIN;

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
        SceneManager.LoadScene("OceanEnv");
    }
}
