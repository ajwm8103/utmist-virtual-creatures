using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using KSS;

public class EvolutionChooseItem : MonoBehaviour
{
    public Text nameText;
    public Button viewButton;
    public string filePath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string filePath){
        this.filePath = filePath;
        nameText.text = Path.GetFileNameWithoutExtension(filePath);
    }

    public void OnPressView(){
        MenuManager.instance.ShowViewEvolutionMenu(filePath);
    }
}
