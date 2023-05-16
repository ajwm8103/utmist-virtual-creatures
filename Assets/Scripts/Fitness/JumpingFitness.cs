using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingFitness : Fitness
{
    public float pushThreshold = 2f;
    public float pushPenaltyDiscount = 0.9f;
    float currSpeed = 0f;
    float distance, prevSpeed;
    Vector3 initialCom, currCom, prevCom;
    float maxAchievedHeight = 0f;
    Creature creature;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    public override float UpdateFrameReward()
    {
        //Creature creature = myEnvironment.currentCreature;
        float reward = 0f;
	
	    prevCom = currCom;
        currCom = creature.GetCentreOfMass();
        if (firstFrame){
            firstFrame = false;
            return 0f;
        }

	    prevSpeed = currSpeed;
        //distance = Vector3.Distance(currCom,prevCom);
        distance = Vector3.Dot(currCom - prevCom, Vector3.up);

        float currHeight = Vector3.Dot(currCom - initialCom, Vector3.up);

        currSpeed = distance / Time.fixedDeltaTime;
        
        if (currHeight > maxAchievedHeight){
            maxAchievedHeight = currHeight;
            reward += distance;
        }

        return reward;
    }

    public override void Reset()
    {
        creature = myEnvironment.currentCreature;
        if (creature == null) return;
        currCom = creature.GetCentreOfMass();
        initialCom = currCom - 1.08f * Vector3.up;
    }
}
