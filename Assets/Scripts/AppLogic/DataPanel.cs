using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPanel : MonoBehaviour
{
    public GameObject dataPanelHolderActive;
    public GameObject dataPanelHolderInactive;
    public GameObject[] dataPages;
    public int selectedPageIdx = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPanel()
    {
        dataPanelHolderActive.SetActive(true);
        dataPanelHolderInactive.SetActive(false);
    }

    public void HidePanel(){
        dataPanelHolderActive.SetActive(false);
        dataPanelHolderInactive.SetActive(true);
    }

    public void ShiftLeft(){
        selectedPageIdx = (selectedPageIdx + dataPages.Length - 1) % dataPages.Length;
        foreach (GameObject page in dataPages)
        {
            page.SetActive(false);
        }
        dataPages[selectedPageIdx].SetActive(true);
    }

    public void ShiftRight()
    {
        selectedPageIdx = (selectedPageIdx + 1) % dataPages.Length;
        foreach (GameObject page in dataPages)
        {
            page.SetActive(false);
        }
        dataPages[selectedPageIdx].SetActive(true);
    }
}
