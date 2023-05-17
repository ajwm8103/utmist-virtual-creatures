using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;


[System.Serializable]
public class FloorEnvSettings : EnvironmentSettings {
    public override EnvCode envCode { get {return EnvCode.FLOOR; }}
    public override EnvArrangeType envArrangeType { get {return EnvArrangeType.LINEAR; } }
    public override float sizeX { get { return 5; } }
    public override float sizeZ { get { return 5; } }
    public override float maxTime { get { return 8; } }
}

public class FloorEnv : Environment
{
    public override EnvCode envCode { get { return EnvCode.FLOOR; } }
    public override void Setup(EnvironmentSettings es)
    {
        base.Setup(es);

        // likely change this later to a general fitness selecting option from the save
        // this change should be in the Environment class TODO
        fitness = gameObject.AddComponent<JumpingFitness>();
        fitness.myEnvironment = this;

        ResetEnv();
    }

    // Update is called once per frame
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void ResetEnv(){
        base.ResetEnv();
    }
    
    public override void StartEnv(CreatureGenotype cg)
    {
        // Edit spawn transform if needed

        base.StartEnv(cg);

    }
}
