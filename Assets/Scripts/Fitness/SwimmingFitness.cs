using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingFitness : Fitness
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override float GetFrameReward()
    {
        Creature creature = myEnvironment.currentCreature;
        float reward = 0;

        // write code here
        // pls read Karl Sims paper, see if size adjustments need to happen (perhaps by a boolean)
        // figure out where the origin of the environemnt is (likely myEnvironment.transform.position)

        return reward;
    }
}
