using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrainingAlgorithm : MonoBehaviour
{
    [SerializeField]
    protected TrainingManager tm;
    [SerializeField]
    protected TrainingSave save;
    
    // Setup is called after all vars are updated and sent to the TrainingAlgorithm
    public virtual void Setup(TrainingManager tm){
        this.tm = tm;
        save = tm.save;
    }

    public abstract void ResetPing(Environment env, float fitness, bool isDQ);
}
