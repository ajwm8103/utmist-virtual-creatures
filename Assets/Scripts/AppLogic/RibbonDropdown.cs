using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RibbonDropdown : MonoBehaviour
{
    public GameObject template;

    private void Start()
    {
        template.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            template.SetActive(false);
        }
    }

    public void OpenTemplate(){
        template.SetActive(true);
    }
}
