using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class OptionsPersist : MonoBehaviour
{
    public static OptionsPersist instance;

    // Vars here
    public string appSavePath;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two persists active at once
        }
        else
        {
            instance = this;
            appSavePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            DontDestroyOnLoad(gameObject);
        }
    }
}
