using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusclePreferenceSetting
{
    public float weight = 80f; // m
    public float length = 0.99f; // l_f
    public float segmentLength = 0.5f; // l_s
    public float momentArm = 0.04f; // d
    public float refLength = 0.5f; // l_ref
    public float refAngle = 110 * Math.PI / 180; // phi_ref
    public float maxForce = 22000f; // F_max
    public float optLength 0.1f; // l_opt
    public float width = 0.4 * optLength; // w
    public float maxVelocity = -12 * optLength; //v_max
    public float eccentricForceEnhancement = 1.5f; // N
    public float curvatureConst = 5f; // K
    public float restLength = 0.4f; // l_ref
    public float refStrain = 0.04 * restLength; // epsilon_ref
    public float couplingConst = 0.01f; // tau
    public float timeDelay = 0.015f; // delta_P
    public float contractileVelocity = 0; // initial value
    public float contractileLength = optLength;
    public float activationRate = 100; // c_a
    public float excitationSignal; // u
}

public class Muscle
{   
    public MusclePreferenceSetting mps;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float tempVelocity = UpdateContractileVelocity(muscleActivation, contractileLength, contractileVelocity, mps);
        float tempLength = UpdateContractileLength(muscleActivation, contractileLength, contractileVelocity, mps);
        contractileVelocity = tempVelocity;
        contractileLength = tempLength;
        muscleActivation = UpdateMuscleActivation(muscleActivation, mps);
    }


    public float ForceLength(float contractileLength, MusclePreferenceSetting mps)
    {
        return Math.Exp(Math.Log(0.05) * (Math.Abs((contractileLength - mps.optLength) / (mps.optLength * width))^3));
    }
    
    public float ForceVelocity(float contractileVelocity, MusclePreferenceSetting mps)
    {
        if (contractileVelocity < 0)
        {
            return (mps.maxVelocity - contractileVelocity)/(mps.maxVelocity + mps.curvatureConst * contractileVelocity);
        }
        else
        {
            return mps.eccentricForceEnhancement + (mps.eccentricForceEnhancement - 1) * (mps.maxVelocity + contractileVelocity) / (7.56 * mps.curvatureConst * contractileVelocity - mps.maxVelocity);
        }
    }

    public float ForceContractileElement(float muscleActivation, float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return muscleActivation * mps.maxForce * ForceLength(contractileLength, mps) * ForceVelocity(contractileVelocity, mps);
    }

    public float ForceSEE(float lengthSEE, MusclePreferenceSetting mps) // Serial Elastic Element
    {
        float epsilon = (lengthSEE - mps.restLength)/mps.restLength;
        if (epsilon > 0)
        {
            return (epsilon/mps.refStrain)^2;
        }
        return 0f;
    }

    public float ForcePEE(float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return (maxForce * ((contractileLength - optLength) / (optLength * 0.56 * optLength))^2) * ForceVelocity(contractileVelocity, mps);
    }

    public float UpdateContractileVelocity(float muscleActivation, float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return muscleActivation * maxForce * ForceLength(contractileLength, mps) / ForceContractileElement(muscleActivation, contractileLength, contractileVelocity, mps);
    }

    public float UpdateContractileLength(float muscleActivation, float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return contractileLength + Time.fixedDeltaTime * (contractileVelocity + UpdateContractileVelocity(muscleActivation, contractileLength, contractileVelocity, mps)) / 2;
    }

    public float UpdateMuscleActivation(float muscleActivation, MusclePreferenceSetting mps)
    {
        return muscleActivation + Time.fixedDeltaTime * mps.activationRate * (mps.excitationSignal - muscleActivation);
    }

}
