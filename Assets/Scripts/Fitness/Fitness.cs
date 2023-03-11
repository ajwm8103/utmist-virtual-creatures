using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Fitness : MonoBehaviour
{
    public Environment myEnvironment;
    public abstract float GetFrameReward();
}
