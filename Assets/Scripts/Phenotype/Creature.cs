using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public List<Neuron> sensors = new List<Neuron>();
    public List<Neuron> neurons = new List<Neuron>();
    public List<Neuron> effectors = new List<Neuron>();

    // Update is called once per frame
    void Update()
    {
        FeedForward();
    }

    public void InitializeCreature()
    {
        Debug.Log("----INITIALIZING CREATURE----");
        ConnectNeurons(neurons);
        ConnectNeurons(effectors);
    }

    void FeedForward()
    {
        foreach (Neuron n in sensors)
        {
            // Get sensor STUFF
            n.SetSensorOutputs();
        }

        foreach (Neuron n in neurons)
        {
            n.GetInputs();
        }

        foreach (Neuron n in neurons)
        {
            n.SetOutput();
        }

        foreach (Neuron n in effectors)
        {
            n.GetInputs();
        }
    }

    void ConnectNeurons(List<Neuron> neuronList)
    {
        foreach (Neuron n in neuronList)
        {
            List<Neuron> inputNeuronsToAdd = new List<Neuron>();
            foreach (NeuronReference nr in n.ng.inputs)
            {
                if (nr.id >= 13)
                {
                    //Debug.Log($"Finding a neuron ({nr.id}) for neuron {n.ng.nr.id}.");
                    Neuron foundNeuron = GetNeuron(neurons, nr, n.ng.nr);
                    if (foundNeuron == null)
                    {
                        // Nay!
                        //Debug.Log("No neuron found.");
                    }
                    else
                    {
                        // Yay!
                        inputNeuronsToAdd.Add(foundNeuron);
                    }
                }
                else if (nr.id == 12)
                {
                    //Debug.Log($"Finding an effector ({nr.id}) for neuron {n.ng.nr.id}.");
                    Neuron foundNeuron = GetNeuron(effectors, nr, n.ng.nr);
                    if (foundNeuron == null)
                    {
                        // Nay!
                        //Debug.Log("No effector found.");
                    }
                    else
                    {
                        // Yay!
                        inputNeuronsToAdd.Add(foundNeuron);
                    }
                }
                else
                {
                    //Debug.Log($"Finding/generating a sensor ({nr.id}) for neuron {n.ng.nr.id}.");
                    Neuron foundNeuron = GetNeuron(sensors, nr, n.ng.nr);
                    if (foundNeuron == null)
                    {
                        //Debug.Log("No sensor found.");
                        /*// Create new sensor if no sensor available
                        Neurongap ng = new Neurongap(nr);
                        foundNeuron = new Neuron(ng);
                        sensors.Add(foundNeuron);*/
                    }
                    inputNeuronsToAdd.Add(foundNeuron);
                }
            }
            n.SetInputNeurons(inputNeuronsToAdd);
        }
    }

    private Neuron GetNeuron(List<Neuron> neuronList, NeuronReference nr, NeuronReference requestingNeuronsNr)
    {
        // requestingNeuronsNr is the nr of the neuron requesting this neuron
        //Debug.Log("Getting Neuron");
        //Debug.Log(nr.id);
        if (nr.isGhost)
        {
            // Check all neurons in ghost and match the id
            foreach (Neuron n in neuronList)
            {
                //Debug.Log(n.ng.nr.id);
                if (n.ng.nr.isGhost && n.ng.nr.id == nr.id)
                {
                    return n;
                }

            }
        }
        else if (nr.isParent)
        {
            // this id will be in the Parent of the requesting neuron, so find all neurons there and match ids
            foreach (Neuron n in neuronList)
            {
                //Debug.Log(n.ng.nr.id);
                NeuronReference potentialNr = n.ng.nr;
                if (potentialNr.id == nr.id && potentialNr.connectionPath.Count + 1 == requestingNeuronsNr.connectionPath.Count)
                {
                    // Checks if the path to the parent of the requesting neuron and the path to the potential neuron are the same.
                    // This means that they are both in the same correct instance of the parent, so it's the right neuron.
                    bool isGood = true;
                    for (int i = 0; i < potentialNr.connectionPath.Count; i++)
                    {
                        if (potentialNr.connectionPath[i] != requestingNeuronsNr.connectionPath[i])
                        {
                            isGood = false;
                            break;
                        }
                    }
                    if (isGood) return n;
                }

            }
        }
        else if (nr.isSelf)
        {
            // this id will be in the same neuron as the requesting neuron, so find all neurons there and match ids
            foreach (Neuron n in neuronList)
            {
                //Debug.Log(n.ng.nr.id);
                NeuronReference potentialNr = n.ng.nr;
                if (potentialNr.id == nr.id)
                {
                    // Checks if the path to the requesting neuron segment and the path to the potential neuron segment are the same.
                    // This means that they are both in the same correct instance of the segment, so it's the right neuron.
                    bool isGood = true;
                    for (int i = 0; i < potentialNr.connectionPath.Count; i++)
                    {
                        //Debug.Log(potentialNr.connectionPath[i] != requestingNeuronsNr.connectionPath[i]);
                        if (potentialNr.connectionPath[i] != requestingNeuronsNr.connectionPath[i])
                        {
                            isGood = false;
                            break;
                        }
                    }
                    if (isGood) return n;
                } // thank you sir (right here joshua helped me fix a curly bracket)
            }
        }
        else
        {
            foreach (Neuron n in neuronList)
            {
                //Debug.Log(n.ng.nr.id);
                if (n.ng.nr.id == nr.id && n.ng.nr.connectionPath.SequenceEqual(nr.connectionPath))
                {
                    return n;
                }

            }
        }


        return null;
    }


    public void AddNeuron(NeuronGenotype ng, HingeJoint effectorJoint, Segment segment)
    {
        //Debug.Log("Adding neuron" + ng.nr.id);
        Neuron n = new Neuron(ng);
        if (n.ng.nr.id >= 13)
        {
            //Debug.Log("Neuron");
            neurons.Add(n);
        }
        else if (n.ng.nr.id == 12)
        {
            //Debug.Log("Effector");
            n.effectorJoint = effectorJoint;
            effectors.Add(n);
        }
        else
        {
            //Debug.Log("Sensor");
            n.segment = segment;
            sensors.Add(n);
        }
    }
}
