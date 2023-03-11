using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class OceanEnvSettings : EnvironmentSettings {
    public override EnvCode envCode { get {return EnvCode.OCEAN; }}
    public override EnvArrangeType envArrangeType { get {return EnvArrangeType.LINEAR; } }
    public override float sizeX { get { return 5; } }
    public override float sizeZ { get { return 5; } }
}

public class OceanEnv : Environment
{
    // Start is called before the first frame update
    public new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ResetEnv(){
        base.ResetEnv();
    }
    
    public override void SpawnCreature(){
        // Edit spawn transform if needed

        base.SpawnCreature();

    }
}
