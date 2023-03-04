using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GraphicsLevel { LOW, MEDIUM, HIGH };
public class Environment : MonoBehaviour
{
    [Header("Settings")]
    public GraphicsLevel graphicsLevel = GraphicsLevel.LOW;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
