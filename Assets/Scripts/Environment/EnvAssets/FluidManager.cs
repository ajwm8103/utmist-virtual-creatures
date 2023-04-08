using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidManager : MonoBehaviour
{
    public static FluidManager instance;

    [Header("Fluid Properties")]
    public float fluidDensity = 1000f;
    public float viscosityDrag = 1f;
    public bool fluidEnabled = true;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // Can't have two fluid managers active at once
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
