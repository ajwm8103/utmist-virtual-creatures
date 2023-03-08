using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GraphicsLevel { LOW, MEDIUM, HIGH };
public class EnvironmentSettings {
    public GraphicsLevel graphicsLevel = GraphicsLevel.LOW;
}

/// <summary>
/// Base class to control a single environment
/// </summary>
public class Environment : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
