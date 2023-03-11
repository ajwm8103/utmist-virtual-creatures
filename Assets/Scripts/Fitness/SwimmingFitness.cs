using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingFitness : Fitness
{
    public float
    // Start is called before the first frame update
    void Start()
    {
        Vector3 curr_com = creature.GetCentreOfMass();
	Vector3 prev_com;
	float distance, prev_speed;
	float curr_speed = 0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override float GetFrameReward()
    {
        Creature creature = myEnvironment.currentCreature;
        float reward = 0f;
	
	prev_com = curr_com;
       	curr_com = creature.GetCentreOfMass();
	prev_speed = curr_speed;
       	distance = Vector3.Distance(curr_com,prev_com);

	curr_speed = distance/Time.deltaTime;
	reward += curr_speed;

	if(2*curr_speed < prev_speed){
		reward *= (1-
        // figure out where the origin of the environemnt is (likely myEnvironment.transform.position)
	// Straight swimming is rewarded over circling by using the maximum distance from the initial center of mass
	// Continuing movement is rewarded over that from a single initial push, by giving the velocities during the final phase of the test period a stronger relative weight in the total fitness value.

        return reward;
    }
}
