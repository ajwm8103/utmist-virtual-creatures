using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

// code example by mxu
// REFERENCES:
// https://docs.unity3d.com/ScriptReference/HingeJoint-motor.html
// https://www2.gwu.edu/~phy21bio/Reading/Purcell_life_at_low_reynolds_number.pdf

public class RLSwimmer : Agent
{
    public GameObject leftSegment;
    public GameObject rightSegment;

    public float motorForce = 200f;
    public float speed = 90f;

    public float t_left_angle_ = 0f;
    public float t_right_angle_ = 0f;
    Vector3 lastpos = new Vector3();

    float timeout = 5f;
    Stopwatch SW = new Stopwatch();

    public override void OnEpisodeBegin() 
    {
        SW.Reset();
        SW.Start();
        return;
    }

    private void SetJointToTargetAngle(HingeJoint hingeJoint, float targetAngle)
    {
        float leftDirection = Mathf.Sign(targetAngle - hingeJoint.angle);
        JointMotor motor = hingeJoint.motor;
        motor.force = motorForce;
        motor.targetVelocity = speed * leftDirection;
        motor.freeSpin = false;
        hingeJoint.motor = motor;
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        HingeJoint leftJoint = leftSegment.GetComponent<HingeJoint>();
        HingeJoint rightJoint = rightSegment.GetComponent<HingeJoint>();
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        float mag_velocity = rb.velocity.magnitude;
        Vector3 input = new Vector3(leftJoint.angle,rightJoint.angle,mag_velocity);
        UnityEngine.Debug.Log(input);
        sensor.AddObservation(input);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float t_left_angle = actions.ContinuousActions[0];
        float t_right_angle = actions.ContinuousActions[1];

        UnityEngine.Debug.Log(t_left_angle);
        UnityEngine.Debug.Log(t_right_angle);

    }


    private void FixedUpdate()
    {
        HingeJoint leftJoint = leftSegment.GetComponent<HingeJoint>();
        HingeJoint rightJoint = rightSegment.GetComponent<HingeJoint>();
        SW.Stop();
        if (SW.ElapsedMilliseconds >= timeout*1000f)
        {

            EndEpisode();
        }
        SW.Start();

        SetJointToTargetAngle(leftJoint, t_left_angle_*360);
        SetJointToTargetAngle(rightJoint, t_right_angle_*360);
    }
}