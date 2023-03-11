using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingFitness : Fitness
{
    public float pushThreshold = 2f;
    public float pushPenaltyDiscount = 0.9f;
    float currSpeed = 0f;
    float distance, prevSpeed;
    Vector3 currCom, prevCom;
    Creature creature;

    // Start is called before the first frame update
    void Start()
    {
        creature = myEnvironment.currentCreature;
        currCom = creature.GetCentreOfMass();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override float GetFrameReward()
    {
        //Creature creature = myEnvironment.currentCreature;
        float reward = 0f;
	
	    prevCom = currCom;
        currCom = creature.GetCentreOfMass();
	    prevSpeed = currSpeed;
        distance = Vector3.Distance(currCom,prevCom);

	    currSpeed = distance/Time.deltaTime;
	    reward += currSpeed;
	
	    // Continuing movement is rewarded over that from a single initial push, by giving the velocities during the final phase of the test period a stronger relative weight in the total fitness value
	    // We do not implement this because I am lazy
	    // Initial push <=> curr speed would be way slower than prev speed => apply discount to reward

	    if(2 * currSpeed < prevSpeed)
	    {
		    reward *= pushPenaltyDiscount;
	    }


        // figure out where the origin of the environemnt is (likely myEnvironment.transform.position)
        // Straight swimming is rewarded over circling by using the maximum distance from the initial center of mass
        // Continuing movement is rewarded over that from a single initial push, by giving the velocities during the final phase of the test period a stronger relative weight in the total fitness value.
        
        return reward;
    }
}
