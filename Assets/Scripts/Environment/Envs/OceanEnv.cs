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
    public override void Setup(EnvironmentSettings es)
    {
        base.Setup(es);

        ResetEnv();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ResetEnv(){
        base.ResetEnv();
    }
    
    public override void SpawnCreature(CreatureGenotype cg)
    {
        // Edit spawn transform if needed

        base.SpawnCreature(cg);

    }
}
