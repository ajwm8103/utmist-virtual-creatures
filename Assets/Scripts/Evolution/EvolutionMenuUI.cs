using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EvolutionMenuUI : MonoBehaviour
{
    public GameObject evolutionMenu;
    public GameObject loadServerMenu;

    // Start is called before the first frame update
    void Start()
    {
        ShowEvolutionMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowEvolutionMenu()
    {
        evolutionMenu.SetActive(true);
        loadServerMenu.SetActive(false);
    }

    public void ShowLoadServerMenu()
    {
        Debug.Log("click");
        loadServerMenu.SetActive(true);
        evolutionMenu.SetActive(false);
    }

    public void Load()
    {
        SceneManager.LoadScene("OceanEnv");
    }
}
