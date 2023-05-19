using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;


public static class SaveContainer {
    public static string GetNextFilename(this string filename)
    {
        int i = 1;
        string dir = Path.GetDirectoryName(filename);
        string file = Path.GetFileNameWithoutExtension(filename) + "({0})";
        string extension = Path.GetExtension(filename);

        while (File.Exists(filename))
            filename = Path.Combine(dir, string.Format(file, i++) + extension);

        return filename;
    }
}
public class OptionsPersist : MonoBehaviour
{
    public static OptionsPersist instance;

    // Vars here
    [HideInInspector]
    public string appSavePath { get; private set; }
    public string VCPath {
        get {
            return Path.Combine(appSavePath, "Virtual Creatures");
        }
    }
    public string VCSaves
    {
        get
        {
            return Path.Combine(VCPath, "Saves");
        }
    }
    public string VCCreatures
    {
        get
        {
            return Path.Combine(VCPath, "Creatures");
        }
    }
    public string VCData
    {
        get
        {
            return Path.Combine(VCPath, "Data");
        }
    }

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

            string[] paths = new string[]{
                VCPath, VCSaves, VCCreatures, VCData
            };

            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                {
                    // Try to create the directory.
                    Directory.CreateDirectory(path);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                }
            }
            DontDestroyOnLoad(gameObject);
        }
    }

    public void UpdateAppSavePath(string newPath){
        if (File.Exists(newPath))
        {
            // This path is a file
            Debug.Log("Provided path is a file.");
        }
        else if (Directory.Exists(newPath))
        {
            // This path is a directory
            appSavePath = newPath;
        }
        else
        {
            Debug.Log("Not a file or a directory.");
        }
    }
}
