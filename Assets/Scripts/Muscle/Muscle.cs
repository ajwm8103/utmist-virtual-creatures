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
    public float refAngle = 110*Math.PI/180; // phi_ref
    public float maxForce = 22000f; // F_max
    public float optLength 0.1f; // l_opt
    public float width = 0.4*optLength; // w
    public float maxVelocity = -12*optLength; //v_max
    public float eccentricForceEnhancement = 1.5f; // N
    public float curvatureConst = 5f; // K
    public float restLength = 0.4f; // l_ref
    public float refStrain = 0.04f; // epsilon_ref
    public float couplingConst = 0.01f; // tau
    public float timeDelay = 0.015f; // delta_P
}

public class Muscle
{
    public MusclePreferenceSetting mps;

    public float ForceLength(contractileLength, mps)
    {
        return Math.Exp(Math.Log(0.05) * (Math.Abs((contractileLength - mps.optLength) / (mps.optLength * width))^3));
    }
    
    public float ForceVelocity(contractileVelocity, mps)
    {
        if (contractileVelocity < 0)
        {
            return (mps.maxVelocity - contractileVelocity)/(mps.maxVelocity + contractileVelocity);
        }
        else
        {
            return eccentricForceEnhancement + (mps.eccentricForceEnhancement - 1) * (mps.maxVelocity + contractileVelocity) / (7.56 * mps.curvatureConst * contractileVelocity - mps.maxVelocity);
        }
    }

    public float ForceContractileElement(muscleActivation, contractileLength, contractileVelocity, mps)
    {
        return muscleActivation * mps.maxForce * ForceLength(contractileLength, mps) * ForceVelocity(contractileVelocity, mps);
    }

    public float ForceSEE(lengthSEE, mps) // Serial Elastic Element
    {
        float epsilon = (lengthSEE - mps.restLength)/mps.restLength;
        float epsilonRef = 0.04f;
        if (epsilon > 0)
        {
            return (epsilon/epsilonRef)^2;
        }
        return 0f;
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
