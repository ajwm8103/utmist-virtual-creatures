using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowEvolve()
    {
        SceneManager.LoadScene("Evolution");
    }

    public void ShowEdit()
    {
        Debug.Log("show edit scene");
    }

    public void ShowView()
    {
        Debug.Log("show view scene");
    }

}
