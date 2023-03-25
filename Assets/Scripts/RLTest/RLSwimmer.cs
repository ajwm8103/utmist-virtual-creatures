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

    public Vector3 start_loc;

    public float motorForce = 200f;
    public float speed = 90f;

    public float t_left_angle_ = 0f;
    public float t_right_angle_ = 0f;
    Vector3 lastpos = new Vector3();

    float timeout = 20f;
    Stopwatch SW = new Stopwatch();

    void Start()
    {
        lastpos = transform.parent.transform.position;
        UnityEngine.Debug.Log(lastpos);
    }

    public override void OnEpisodeBegin() 
    {
        
        transform.parent.transform.position = new Vector3(lastpos.x,lastpos.y,lastpos.z);
        UnityEngine.Debug.Log(transform.parent.transform.position);

        Rigidbody rb = transform.GetComponent<Rigidbody>();
        float mag_velocity = rb.velocity.magnitude;
        UnityEngine.Debug.Log(mag_velocity);
        // HingeJoint leftJoint = leftSegment.GetComponent<HingeJoint>();
        // HingeJoint rightJoint = rightSegment.GetComponent<HingeJoint>();
        // motorForce = 2000f;
        // SetJointToTargetAngle(leftJoint, 0);
        // SetJointToTargetAngle(rightJoint, 0);
        // motorForce = 200f;

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
        // UnityEngine.Debug.Log(input);
        sensor.AddObservation(input);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        t_left_angle_ = actions.ContinuousActions[0] + 1;
        t_right_angle_ = actions.ContinuousActions[1] + 1;
        HingeJoint leftJoint = leftSegment.GetComponent<HingeJoint>();
        HingeJoint rightJoint = rightSegment.GetComponent<HingeJoint>();
        // UnityEngine.Debug.Log(t_left_angle_);
        // UnityEngine.Debug.Log(t_right_angle_);
        SetJointToTargetAngle(leftJoint, t_left_angle_*180);
        SetJointToTargetAngle(rightJoint, t_right_angle_*180);
        
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
    //     continuousActions[0] = Random.Range(-1f,1f);
    //     continuousActions[1] = Random.Range(-1f,1f);
    // }


    private void FixedUpdate()
    {
        // UnityEngine.Debug.Log(SW.ElapsedMilliseconds);
        SW.Stop();
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        float mag_velocity = rb.velocity.magnitude;
        AddReward(mag_velocity);
        if (SW.ElapsedMilliseconds >= timeout*1000f) EndEpisode();
        SW.Start();

        
    }
}