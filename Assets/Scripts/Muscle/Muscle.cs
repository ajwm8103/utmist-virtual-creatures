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
    public float refAngle = 110 * Mathf.PI / 180; // phi_ref
    public float maxForce = 22000f; // F_max
    public float optLength = 0.1f; // l_opt
    public float width; // w
    public float maxVelocity; //v_max
    public float eccentricForceEnhancement = 1.5f; // N
    public float curvatureConst = 5f; // K
    public float restLength = 0.4f; // l_ref
    public float refStrain; // epsilon_ref
    public float couplingConst = 0.01f; // tau
    public float timeDelay = 0.015f; // delta_P
    public float activationRate = 100; // c_a

    public MusclePreferenceSetting(){
        width = 0.4f * optLength;
        maxVelocity = -12 * optLength;
        refStrain = 0.04f * restLength;
    }
}

public class Muscle
{   
    public MusclePreferenceSetting mps;

    public float excitationSignal; // u
    public float contractileLength = 0.1f; // initial value (l_opt)
    public float contractileVelocity = 0; // initial value
    public float muscleActivation;

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
        muscleActivation = UpdateMuscleActivation(muscleActivation, excitationSignal, mps);
    }


    public float ForceLength(float contractileLength, MusclePreferenceSetting mps)
    {
        return Mathf.Exp(Mathf.Log(0.05f) * (Mathf.Abs((contractileLength - mps.optLength) / Mathf.Pow((mps.optLength * mps.width), 3f))));
    }
    
    public float ForceVelocity(float contractileVelocity, MusclePreferenceSetting mps)
    {
        if (contractileVelocity < 0)
        {
            return (mps.maxVelocity - contractileVelocity)/(mps.maxVelocity + mps.curvatureConst * contractileVelocity);
        }
        else
        {
            return mps.eccentricForceEnhancement + (mps.eccentricForceEnhancement - 1) * (mps.maxVelocity + contractileVelocity) / (7.56f * mps.curvatureConst * contractileVelocity - mps.maxVelocity);
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
            return Mathf.Pow(epsilon / mps.refStrain, 2f);
        }
        return 0f;
    }

    public float ForcePEE(float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return mps.maxForce * ((contractileLength - mps.optLength) / Mathf.Pow(mps.optLength * 0.56f * mps.optLength, 2f)) * ForceVelocity(contractileVelocity, mps);
    }

    public float UpdateContractileVelocity(float muscleActivation, float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return muscleActivation * mps.maxForce * ForceLength(contractileLength, mps) / ForceContractileElement(muscleActivation, contractileLength, contractileVelocity, mps);
    }

    public float UpdateContractileLength(float muscleActivation, float contractileLength, float contractileVelocity, MusclePreferenceSetting mps)
    {
        return contractileLength + Time.fixedDeltaTime * (contractileVelocity + UpdateContractileVelocity(muscleActivation, contractileLength, contractileVelocity, mps)) / 2;
    }

    public float UpdateMuscleActivation(float muscleActivation, float excitationSignal, MusclePreferenceSetting mps)
    {
        return muscleActivation + Time.fixedDeltaTime * mps.activationRate * (excitationSignal - muscleActivation);
    }

}
