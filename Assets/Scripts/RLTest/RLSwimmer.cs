using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code example by mxu
// REFERENCES:
// https://docs.unity3d.com/ScriptReference/HingeJoint-motor.html
// https://www2.gwu.edu/~phy21bio/Reading/Purcell_life_at_low_reynolds_number.pdf

public class RLSwimmer : MonoBehaviour
{
    public GameObject leftSegment;
    public GameObject rightSegment;

    public float motorForce = 200f;
    public float speed = 90f;
    public float eps = 2f;
    int state = 0;

    float[] targetLeftAngles = { 45f, 45f, -45f, -45f };
    float[] targetRightAngles = { -45f, 45f, 45f, -45f };

    private void SetJointToTargetAngle(HingeJoint hingeJoint, float targetAngle)
    {
        float leftDirection = Mathf.Sign(targetAngle - hingeJoint.angle);
        JointMotor motor = hingeJoint.motor;
        motor.force = motorForce;
        motor.targetVelocity = speed * leftDirection;
        motor.freeSpin = false;
        hingeJoint.motor = motor;
    }

    private bool IsCloseEnough(float currentAngle, float targetAngle)
    {
        if (Mathf.Abs(currentAngle - targetAngle) < eps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void FixedUpdate()
    {
        /* 4 states
         * 
         * 0. Both Up
         * 1. Left up Right down
         * 2. Both down
         * 3. Left down Right up
         * 
         */


        HingeJoint leftJoint = leftSegment.GetComponent<HingeJoint>();
        HingeJoint rightJoint = rightSegment.GetComponent<HingeJoint>();

        SetJointToTargetAngle(leftJoint, targetLeftAngles[state]);
        SetJointToTargetAngle(rightJoint, targetRightAngles[state]);

        if (IsCloseEnough(leftJoint.angle, targetLeftAngles[state]) && IsCloseEnough(rightJoint.angle, targetRightAngles[state]))
        {
            state += 1;
            if (state > 3)
            {
                state = 0;
            }
        }
    }
}