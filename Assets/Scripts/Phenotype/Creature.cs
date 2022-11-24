using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Neuron
{
    public Neuron neuronA, neuronB, neuronC;
    public float a, b, c;
    public float outValue;
    public float dummy1;
    public NeuronGenotype ng;
    public HingeJoint effectorJoint;
    public Segment segment;

    public void GetInputs()
    {
        // Debug.Log(neuronA);
        if (neuronA != null) a = neuronA.outValue * ng.weights[0];
        if (neuronB != null) b = neuronB.outValue * ng.weights[1];
        if (neuronC != null) c = neuronC.outValue * ng.weights[2];


        if (ng.nr.id == 12)
        {
            // I am an "effector" I must "effect"
            //Debug.Log("Effector output" + a);
            JointMotor motor = effectorJoint.motor;
            motor.targetVelocity = a;
            effectorJoint.motor = motor;
        }

        //Debug.Log($"Neuron {ng.nr.id} has inputs {a}, {b}, and {c}.");
    }

    public void SetSensorOutputs()
    {
        outValue = ng.nr.id switch
        {
            0 => segment.GetContact("Right"),
            1 => segment.GetContact("Left"),
            2 => segment.GetContact("Top"),
            3 => segment.GetContact("Bottom"),
            4 => segment.GetContact("Front"),
            5 => segment.GetContact("Back"),
            6 => segment.jointAxisX,
            7 => segment.jointAxisY,
            8 => segment.jointAxisZ,
            9 => segment.GetPhotosensor(0),
            10 => segment.GetPhotosensor(1),
            11 => segment.GetPhotosensor(2),
            _ => 0
        };
    }

    public void SetOutput()
    {
        outValue = ng.type switch
        {
            0 => a + b, // sum
            1 => a * b, // product
            2 => a / b, // divide
            3 => Mathf.Min(a + b, c), // sum-threshold
            4 => a > b ? 1 : -1, // greater-than
            5 => Mathf.Sign(a), //sign-of
            6 => Mathf.Min(a, b), // min
            7 => Mathf.Max(a, b), // max
            8 => Mathf.Abs(a), // abs
            9 => a > 0 ? b : c, // if
            10 => a + (b - a) * c, // interpolate
            11 => Mathf.Sin(a), // sin
            12 => Mathf.Cos(a), // cos
            13 => Mathf.Atan(a), // atan
            14 => Mathf.Log10(a), // log
            15 => Mathf.Exp(a), // expt
            16 => 1 / (1 + Mathf.Exp(-a)), // sigmoid
            17 => outValue + Time.deltaTime * ((a + dummy1) * 0.5f), // integrate
            18 => (a - dummy1) / Time.deltaTime, // differentiate
            19 => outValue + (a - outValue) * 0.5f, // smooth
            20 => a, // memory
            21 => b * Mathf.Sin(Time.time * a) + c, // oscillate-wave
            22 => b * (Time.time * a - Mathf.Floor(Time.time * a)) + c, // oscillate-saw
            _ => 0
        };
        if (ng.type == 17 || ng.type == 18)
        {
            dummy1 = a;
        }
    }

    public void SetInputNeurons(List<Neuron> inputNeurons)
    {
        if (inputNeurons.Count >= 1)
        {
            neuronA = inputNeurons[0];
        }

        if (inputNeurons.Count >= 2)
        {
            neuronB = inputNeurons[1];
        }

        if (inputNeurons.Count >= 3)
        {
            neuronC = inputNeurons[2];
        }
        if (inputNeurons.Count > 3)
        {
            Debug.Log("??????");
            return;
        }
    }

    public Neuron(NeuronGenotype ng)
    {
        this.ng = ng;
    }
}

public class Creature : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
