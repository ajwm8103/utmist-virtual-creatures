using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
 
public class MutateGenotype
{
    public static float NextGaussian()
    {
        float v1, v2, s;
        do
        {
            v1 = Random.Range(0f, 2f) - 1.0f;
            v2 = Random.Range(0f, 2f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }

    public static float NextGaussian(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }

    public static float NextGaussian(float mean, float standard_deviation, float min, float max)
    {
        float x;
        do
        {
            x = NextGaussian(mean, standard_deviation);
        } while (x < min || x > max);
        return x;
    }

    public static bool CoinFlip(float percent)
    {
        return Random.Range(0.0f, 1.0f) <= percent;
    }

    public static int RandomSign()
    {
        return Random.Range(0, 2) * 2 - 1;
    }

    public static bool RandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    public class MutationPreferenceSetting
    {
        public bool mutateMorphology = true;
        public bool mutateNeural = false;
        public float stdevSizeAdjustmentFactor = 0.3f;

        public struct MutationPreference
        {
            public float mutationChance;
            public float stdev;

            public MutationPreference(float mutationChance, float stdev)
            {
                this.mutationChance = mutationChance;
                this.stdev = stdev;
            }
        }

        float currentScaleFactor = 1f;
        public Dictionary<string, MutationPreference> mutationFrequencies = new Dictionary<string, MutationPreference>()
        {
            {"s_r", new MutationPreference(0.25f, 0.25f)}, // Red (byte)
            {"s_g", new MutationPreference(0.25f, 0.25f)}, // Green (byte)
            {"s_b", new MutationPreference(0.25f, 0.25f)}, // Blue (byte)
            {"s_rl", new MutationPreference(0.25f, 0.25f)}, // Recursive limit (byte, 0:15)
            {"s_dx", new MutationPreference(0.25f, 0.25f)}, // Dimension X (float, 0.05f:3f)
            {"s_dy", new MutationPreference(0.25f, 0.25f)}, // Dimension Y (float, 0.05f:3f)
            {"s_dz", new MutationPreference(0.25f, 0.25f)}, // Dimension Z (float, 0.05f:3f)
            {"s_jt", new MutationPreference(0.25f, 0.25f)}, // JointType (enum, 0:3)
            {"s_dest", new MutationPreference(0.25f, 0.25f)}, // Destination (byte, 1:255)
            {"s_a", new MutationPreference(0.25f, 0.25f)}, // Anchor (sadness)
            {"s_o", new MutationPreference(0.25f, 0.25f)}, // Orientation (float, 0f:360f)
            {"s_s", new MutationPreference(0.25f, 0.25f)}, // Scale (float, 0.2f:2f)
            {"s_reflected", new MutationPreference(0.25f, 0.25f)}, // Reflected (bool)
            {"s_t", new MutationPreference(0.25f, 0.25f)}, // Terminal-only (bool)
            {"s_addc", new MutationPreference(0.25f, 0.25f)}, // Add connection
            {"s_removec", new MutationPreference(0.25f, 0.25f)}, // Remove connection
            {"n_t", new MutationPreference(0.25f, 0.25f)}, // Type (byte, 0:22)
            {"n_w1", new MutationPreference(0.25f, 0.25f)}, // Weight 1 (float, -15:15)
            {"n_w2", new MutationPreference(0.25f, 0.25f)}, // Weight 2 (float, -15:15)
            {"n_w3", new MutationPreference(0.25f, 0.25f)}, // Weight 3 (float, -15:15)
            {"n_relocateinput", new MutationPreference(0.25f, 0.25f)}, // Relocate Input
        };

        public Dictionary<string, float[]> floatClamps = new Dictionary<string, float[]>() {
            {"s_dx", new float[2]{0.05f,3f}},
            {"s_dy", new float[2]{0.05f,3f}},
            {"s_dz", new float[2]{0.05f,3f}},
            {"s_o", new float[2]{0f,360f}},
            {"s_s", new float[2]{0.2f,2f}},
            {"n_w1", new float[2]{-15f,15f}},
            {"n_w2", new float[2]{-15f,15f}},
            {"n_w3", new float[2]{-15f,15f}},
        };

        public Dictionary<string, byte[]> byteClamps = new Dictionary<string, byte[]>() {
            {"s_r", new byte[2]{0,255}},
            {"s_g", new byte[2]{0,255}},
            {"s_b", new byte[2]{0,255}},
            {"s_rl", new byte[2]{0,15}},
            {"s_dest", new byte[2]{1,255}},
        };

        public bool CoinFlip(string parameter)
        {
            return MutateGenotype.CoinFlip(mutationFrequencies[parameter].mutationChance * currentScaleFactor);
        }

        public float ModifyFloat(float mean, string parameter)
        {
            return NextGaussian(mean, mutationFrequencies[parameter].stdev + Mathf.Abs(mean * stdevSizeAdjustmentFactor), floatClamps[parameter][0], floatClamps[parameter][1]);
        }

        public float ModifyFloatNoFactor(float mean, string parameter)
        {
            return NextGaussian(mean, mutationFrequencies[parameter].stdev, floatClamps[parameter][0], floatClamps[parameter][1]);
        }

        public byte ModifyByte(float mean, string parameter)
        {
            return (byte)Mathf.RoundToInt(NextGaussian(mean, mutationFrequencies[parameter].stdev + Mathf.Abs(mean * stdevSizeAdjustmentFactor), byteClamps[parameter][0], byteClamps[parameter][1]));
        }

        public byte ModifyByteNoFactor(float mean, string parameter)
        {
            return (byte)Mathf.RoundToInt(NextGaussian(mean, mutationFrequencies[parameter].stdev, byteClamps[parameter][0], byteClamps[parameter][1]));
        }

        public void SetFactor(float factor)
        {
            currentScaleFactor = factor;
        }
    }

    public void SimplifyCreatureGenotype(CreatureGenotype cg)
    {
        // Remove all unconnected segments

        // List all connected segments
        Dictionary<byte, List<SegmentConnectionGenotype>> segmentConnectionsByDest = new Dictionary<byte, List<SegmentConnectionGenotype>>();
        /*foreach (SegmentGenotype sm in cm.segments)
        {
            foreach (SegmentConnectionGenotype scm in sm.connections)
            {
                if (segmentConnectionsByDest.ContainsKey(scm.destination))
                {
                    segmentConnectionsByDest[scm.destination].Add(scm);
                } else {
                    segmentConnectionsByDest.Add(scm.destination, new List<SegmentConnectionGenotype>() { scm });
                }
                
            }
        }*/

        Queue<SegmentConnectionGenotype> connectionsToSearch = new Queue<SegmentConnectionGenotype>();

        segmentConnectionsByDest[0] = new List<SegmentConnectionGenotype>();
        segmentConnectionsByDest[1] = new List<SegmentConnectionGenotype>();

        cg.GetSegment(1).connections.ForEach(item => connectionsToSearch.Enqueue(item));
        while (connectionsToSearch.Count > 0)
        {
            SegmentConnectionGenotype scg = connectionsToSearch.Dequeue();
            bool destSearched = segmentConnectionsByDest.ContainsKey(scg.destination);
            //Debug.Log(scm.destination);
            if (destSearched)
            {
                segmentConnectionsByDest[scg.destination].Add(scg);
            }
            else
            {
                segmentConnectionsByDest.Add(scg.destination, new List<SegmentConnectionGenotype>() { scm });
            }


            SegmentGenotype sg = cg.GetSegment(scg.destination);
            if (sg != null && !destSearched) // If the segment was found and has not already been searched
            {
                foreach (SegmentConnectionGenotype scg2 in cg.GetSegment(scg.destination).connections)
                {
                    connectionsToSearch.Enqueue(scg2);
                }
            }
        }

        // Remove all unconnected segments
        cg.segments.RemoveAll(item => item.id != 0 && item.id != 1 && !segmentConnectionsByDest.ContainsKey(item.id));

        // Set each segment to lowest free id
        foreach (SegmentGenotype sg in cg.segments)
        {
            if (sg.id == 0 || sg.id == 1)
            {
                continue;
            }
            for (byte i = 2; i < Mathf.Min(sg.id, 255); i++)
            {
                if (cg.GetSegment(i) == null)
                {
                    Debug.Log($"Replacing old id {sg.id} with new id {i}.");
                    // Found lower value
                    byte oldId = sg.id;


                    // Update all connections with this as a destination
                    if (segmentConnectionsByDest.ContainsKey(oldId))
                    {
                        foreach (SegmentConnectionGenotype scg in segmentConnectionsByDest[oldId])
                        {
                            scg.destination = i;
                        }
                    }

                    sg.id = i;
                    break;
                }
            }
        }
    }

    public void TraceConnections(CreatureGenotype cg, Dictionary<byte, byte> recursiveLimitValues,
		    SegmentConnectionGenotype myConnection, List<byte> connectionPath, List<byte> segmentIds,
		    List<List<byte>> connectionPaths, List<NeuronReference> neuronReferences)
    {
        // Find SegmentGenotype
        byte id;
        if (myConnection == null)
        {
            // Do the stuff for Ghost
            id = 0;
            SegmentGenotype ghostSegmentGenotype = cg.GetSegment(id);

            if (ghostSegmentGenotype != null)
            {
                // Add neurons
                foreach (NeuronGenotype ng in ghostSegmentGenotype.neurons)
                {
                    ng.nr.connectionPath = connectionPath;
                    ng.nr.isGhost = true;
                    neuronReferences.Add(ng.nr);
                }

                connectionPaths.Add(null);
                segmentIds.Add(id);
            }
            // Do the stuff for Root
            id = 1;
            SegmentGenotype rootSegmentGenotype = cg.GetSegment(id);

            if (rootSegmentGenotype == null) return;

            // Change recursiveLimit stuff
            bool runTerminalOnly = false;
            recursiveLimitValues[id]--;
            if (recursiveLimitValues[id] == 0)
            {
                runTerminalOnly = true;
            }

            // Add neurons
            foreach (NeuronGenotype ng in rootSegmentGenotype.neurons)
            {
                ng.nr.connectionPath = connectionPath;
                neuronReferences.Add(ng.nr);
            }

            connectionPaths.Add(connectionPath);
            segmentIds.Add(id);

            foreach (SegmentConnectionGenotype connection in rootSegmentGenotype.connections)
            {

                if (recursiveLimitValues[connection.destination] > 0)
                {
                    if (!runTerminalOnly && connection.terminalOnly)
                    {
                        continue;
                    }
                    var recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                    var connectionPathClone = connectionPath.Select(item => (byte)item).ToList();
                    connectionPathClone.Add(connection.id);
                    TraceConnections(cg, recursiveLimitClone, connection, connectionPathClone, segmentIds, connectionPaths, neuronReferences);
                }
            }
        }
        else
        {
            id = myConnection.destination;
            SegmentGenotype currentSegmentGenotype = cg.GetSegment(id);

            if (currentSegmentGenotype == null) return;

            // Change recursiveLimit stuff
            bool runTerminalOnly = false;
            recursiveLimitValues[id]--;
            if (recursiveLimitValues[id] == 0)
            {
                runTerminalOnly = true;
            }

            // Add neurons
            foreach (NeuronGenotype ng in currentSegmentGenotype.neurons)
            {
                ng.nr.connectionPath = connectionPath;
                neuronReferences.Add(ng.nr);

            }

            connectionPaths.Add(connectionPath);
            segmentIds.Add(id);

            foreach (SegmentConnectionGenotype connection in currentSegmentGenotype.connections)
            {

                if (recursiveLimitValues[connection.destination] > 0)
                {
                    if (!runTerminalOnly && connection.terminalOnly)
                    {
                        continue;
                    }
                    var recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                    //var connectionPathClone = connectionPath.Select(item => (byte)item).ToList();
                    var connectionPathClone = new List<byte>(connectionPath);
                    connectionPathClone.Add(connection.id);
                    TraceConnections(cg, recursiveLimitClone, connection, connectionPathClone, segmentIds, connectionPaths, neuronReferences);
                }
            }
        }

    }


    public bool GenerateRandomSegmentGenotype(CreatureGenotype cg)
    {
        for (byte i = 1; i < 255; i++)
        {
            if (cg.GetSegment(i) == null)
            {
                // Found unused id, add node here
                SegmentGenotype generatedSegmentGenotype = new SegmentGenotype();
                generatedSegmentGenotype.r = (byte)Random.Range(0, 256);
                generatedSegmentGenotype.g = (byte)Random.Range(0, 256);
                generatedSegmentGenotype.b = (byte)Random.Range(0, 256);
                generatedSegmentGenotype.id = i;
                generatedSegmentGenotype.connections = new List<SegmentConnectionGenotype>();
                generatedSegmentGenotype.neurons = new List<NeuronGenotype>();
                generatedSegmentGenotype.recursiveLimit = (byte)Random.Range(1, 16);
                generatedSegmentGenotype.dimensionX = Random.Range(0.05f, 3f);
                generatedSegmentGenotype.dimensionY = Random.Range(0.05f, 3f);
                generatedSegmentGenotype.dimensionZ = Random.Range(0.05f, 3f);
                generatedSegmentGenotype.jointType = (JointType)Random.Range(0, 4);

                cg.segments.Add(generatedSegmentGenotype);
                return true;
            }
        }
        return false;
    }

    public void GenerateRandomNeuronGenotype(CreatureGenotype cg, List<List<byte>> connectionPaths, List<byte> segmentIds, List<NeuronReference> neuronReferences)
    {
        // Pick a random segment path
        int segmentSelectionId = Random.Range(0, connectionPaths.Count);
        List<byte> segmentPath = connectionPaths[segmentSelectionId];

        byte segmentId = segmentIds[segmentSelectionId];
        SegmentGenotype SegmentGenotype = cg.GetSegment(segmentId);

        // Make the neuron reference
        NeuronReference spawnedNeuronReference = new NeuronReference();
        spawnedNeuronReference.connectionPath = segmentPath;
        spawnedNeuronReference.isGhost = segmentPath == null; // if no segment path, neuron was chosen to spawn in ghost

        // Pick unused id
        for (byte i = 13; i < 255; i++)
        {
            if (SegmentGenotype.GetNeuron(i) == null)
            {
                // Found unused id, add node here
                spawnedNeuronReference.id = i;
                break;
            }
        }

        // Set a neuron type
        byte type = (byte)Random.Range(0, 23);
        byte typeInputs = type switch
        {
            0 => 2,
            1 => 2,
            2 => 2,
            3 => 3,
            4 => 2,
            5 => 1,
            6 => 2,
            7 => 2,
            8 => 1,
            9 => 3,
            10 => 3,
            11 => 1,
            12 => 1,
            13 => 1,
            14 => 1,
            15 => 1,
            16 => 1,
            17 => 1,
            18 => 1,
            19 => 1,
            20 => 1,
            21 => 3,
            22 => 3,
            _ => 0
        };

        // Find all valid inputs for the node and connect them
        // Valid inputs will be within the node, the parent of the node, a child of the node, or the ghost
        // Make list of all those then select randomly
        List<NeuronReference> possibleNeurons = new List<NeuronReference>();
        if (segmentPath == null)
        {
            // All neurons are valid, so list all

            foreach (NeuronReference nr in neuronReferences)
            {
                if (nr.id != 12) // Is sensor or neuron
                {
                    possibleNeurons.Add(nr);
                }
            }
        }
        else
        {

            foreach (NeuronReference nr in neuronReferences)
            {
                if (nr.id == 12) // Is effector
                {
                    continue;
                }
                if (nr.isGhost) // Is ghost
                {
                    possibleNeurons.Add(nr);
                }
                else if (nr.connectionPath.Count == segmentPath.Count)
                { // Within the node or bust
                    bool isGood = true;
                    for (int i = 0; i < nr.connectionPath.Count; i++)
                    {
                        //Debug.Log();
                        if (nr.connectionPath[i] != segmentPath[i])
                        {
                            isGood = false;
                            break;
                        }
                    }
                    if (isGood) possibleNeurons.Add(nr);

                }
                else if (nr.connectionPath.Count == segmentPath.Count + 1)
                { // Child of node or bust
                    bool isGood = true;
                    for (int i = 0; i < segmentPath.Count; i++)
                    {
                        //Debug.Log();
                        if (nr.connectionPath[i] != segmentPath[i])
                        {
                            isGood = false;
                            break;
                        }
                    }
                    if (isGood) possibleNeurons.Add(nr);
                }
                else if (nr.connectionPath.Count == segmentPath.Count - 1)
                { // Parent of node or bust
                    bool isGood = true;
                    for (int i = 0; i < nr.connectionPath.Count; i++)
                    {
                        //Debug.Log();
                        if (nr.connectionPath[i] != segmentPath[i])
                        {
                            isGood = false;
                            break;
                        }
                    }
                    if (isGood) possibleNeurons.Add(nr);
                }
            }
        }


        NeuronReference[] neuronInputs = new NeuronReference[typeInputs];
        float[] neuronWeights = new float[typeInputs];
        for (int i = 0; i < typeInputs; i++)
        {
            // Select random id
            int selectedNeuronId = Random.Range(0, possibleNeurons.Count);

            Debug.Log(i);
            Debug.Log(selectedNeuronId);
            // Add random neuron to neuronInputs
            neuronInputs[i] = possibleNeurons[selectedNeuronId];

            // Remove random neuron from possibilities
            possibleNeurons.RemoveAt(selectedNeuronId);

            // Set random weight
            neuronWeights[i] = Random.Range(-15f, 15f);
        }

        NeuronGenotype ng = new NeuronGenotype(type, neuronInputs, spawnedNeuronReference);
        ng.weights = neuronWeights;
        SegmentGenotype.neurons.Add(ng);
        //return true;
    }

    // TODELETE B/C DEBUG
    public List<List<byte>> cp1 = new List<List<byte>>();
    public List<NeuronReference> nr1 = new List<NeuronReference>();

    public CreatureGenotype MutateCreatureGenotype(CreatureGenotype cg, MutationPreferenceSetting mp)
    {
        Debug.Log("----MUTATING CREATURE----");
        // Get list of segment connection paths and all neuron connection paths for random selection

        List<List<byte>> connectionPaths = new List<List<byte>>();
        List<byte> segmentIds = new List<byte>();
        List<NeuronReference> neuronReferences = new List<NeuronReference>();

        // Create recursive limit dict
        Dictionary<byte, byte> recursiveLimitInitial = new Dictionary<byte, byte>();
        foreach (SegmentGenotype segment in cg.segments)
        {
            recursiveLimitInitial[segment.id] = segment.recursiveLimit;
        }

        TraceConnections(cg, recursiveLimitInitial, null, new List<byte>(), segmentIds, connectionPaths, neuronReferences);
        cp1 = connectionPaths;
        nr1 = neuronReferences;


        // Mutations are performed on a per element basis
        // Scale mutation frequencies by an amount inversely proportional to the size of the
        // current graph being mutated. On average, at least one mutation occurs in the entire graph.

        float graphSizeFactor = 1f;
        mp.SetFactor(graphSizeFactor);

        // First mutate the outer graph, then the inner layer.
        // Legal values of inner depend on the topology of the outer.

        // 1. Node parameters subject to alteration.
        // Mutation freq for each parameter type.
        // Bools mutated by state flip. Scalars by adding
        // several random numbers for Gaussian-like distribution.
        // Adjustment scale relative to og value.
        // Can also be negated.
        // Clamped to legal bounds (what are the legal bounds?)
        // Limited legal values new value from set.
        if (mp.mutateMorphology)
        {
            foreach (SegmentGenotype sg in cg.segments)
            {
                if (mp.CoinFlip("s_r"))
                {
                    sg.r = mp.ModifyByteNoFactor(sg.r, "s_r");
                }
                if (mp.CoinFlip("s_g"))
                {
                    sg.g = mp.ModifyByteNoFactor(sg.g, "s_g");
                }
                if (mp.CoinFlip("s_b"))
                {
                    sg.b = mp.ModifyByteNoFactor(sg.b, "s_b");
                }
                if (mp.CoinFlip("s_rl"))
                {
                    sg.recursiveLimit = mp.ModifyByte(sg.recursiveLimit, "s_rl");
                }
                if (mp.CoinFlip("s_dx"))
                {
                    sg.dimensionX = mp.ModifyFloat(sg.dimensionX, "s_dx");
                }
                if (mp.CoinFlip("s_dy"))
                {
                    sg.dimensionY = mp.ModifyFloat(sg.dimensionY, "s_dy");
                }
                if (mp.CoinFlip("s_dz"))
                {
                    sg.dimensionZ = mp.ModifyFloat(sg.dimensionZ, "s_dz");
                }
                if (mp.CoinFlip("s_jt"))
                {
                    sg.jointType = (JointType)Random.Range(0, 4);
                }
            }

            // 2. New random node added to graph.
            GenerateRandomSegmentGenotype(cg);


            // 3. Connection parameters subjected to mutation.
            // Sometimes pointer moved to point to a different node at random.
            // This is how neural nodes stay - they become the input of another neuron
            foreach (SegmentGenotype sg in cg.segments)
            {
                foreach (SegmentConnectionGenotype scg in sg.connections)
                {
                    if (mp.CoinFlip("s_dest"))
                    {
                        scg.destination = cg.segments[Random.Range(0, cg.segments.Count)].id;
                    }
                    if (mp.CoinFlip("s_a"))
                    {
                        // Actually horrible
                    }
                    if (mp.CoinFlip("s_o"))
                    {
                        Vector3 rotA = new Quaternion(scg.orientationX, scg.orientationY, scg.orientationZ, scg.orientationW).eulerAngles;
                        rotA = new Vector3(mp.ModifyFloatNoFactor(rotA.x, "s_o"), mp.ModifyFloatNoFactor(rotA.y, "s_o"), mp.ModifyFloatNoFactor(rotA.z, "s_o"));
                        Quaternion rot = Quaternion.Euler(rotA);
                        scg.orientationX = rot.x;
                        scg.orientationY = rot.y;
                        scg.orientationZ = rot.z;
                        scg.orientationW = rot.w;
                    }
                    if (mp.CoinFlip("s_s"))
                    {
                        scg.scale = mp.ModifyFloat(scg.scale, "s_s");
                    }
                    if (mp.CoinFlip("s_reflected"))
                    {
                        scg.reflected ^= true;
                    }
                    if (mp.CoinFlip("s_t"))
                    {
                        scg.terminalOnly ^= true;
                    }
                }
            }

            // 4. New random connections added and existing are removed.
            // Morphographs only. Each existing node is subject to having a new connection added.
            // Each existing connection is subject to possible removal.
            foreach (SegmentGenotype sg in cg.segments)
            {
                List<SegmentConnectionGenotype> toRemove = new List<SegmentConnectionGenotype>();
                foreach (SegmentConnectionGenotype scg in sg.connections)
                {
                    if (mp.CoinFlip("s_removec"))
                    {
                        toRemove.Add(scg);
                    }
                }
                foreach (SegmentConnectionGenotype scg in toRemove)
                {
                    sg.connections.Remove(scg);
                }
            }

            foreach (SegmentGenotype sg in cg.segments)
            {
                if (sg.id != 0 && mp.CoinFlip("s_addc"))
                {
                    // Add lowest id possible connection
                    for (byte i = 0; i < 255; i++)
                    {
                        if (sg.GetConnection(i) == null)
                        {
                            // Select random other node and connect to it
                            SegmentConnectionGenotype scg = new SegmentConnectionGenotype();
                            scg.id = i;
                            scg.destination = cg.segments[Random.Range(0, cg.segments.Count)].id;

                            // Anchors
                            switch (Random.Range(0, 3))
                            {
                                case 0:
                                    scg.anchorX = 0.5f * RandomSign();
                                    scg.anchorY = Random.Range(-0.5f, 0.5f);
                                    scg.anchorZ = Random.Range(-0.5f, 0.5f);
                                    break;
                                case 1:
                                    scg.anchorX = Random.Range(-0.5f, 0.5f);
                                    scg.anchorY = 0.5f * RandomSign();
                                    scg.anchorZ = Random.Range(-0.5f, 0.5f);
                                    break;
                                case 2:
                                    scg.anchorX = Random.Range(-0.5f, 0.5f);
                                    scg.anchorY = Random.Range(-0.5f, 0.5f);
                                    scg.anchorZ = 0.5f * RandomSign();
                                    break;
                            }

                            Quaternion rot = Random.rotationUniform;
                            scg.orientationX = rot.x;
                            scg.orientationY = rot.y;
                            scg.orientationZ = rot.z;
                            scg.orientationW = rot.w;

                            scg.scale = Random.Range(0.2f, 2f);
                            scg.reflected = RandomBool();
                            scg.terminalOnly = RandomBool();
                            sg.connections.Add(scg);
                            break;
                        }
                    }
                }
            }

            // 5. Unconnected elements are garbage collected / simplified
            // Look for all
            SimplifyCreatureGenotype(cg);
        }
        if (mp.mutateNeural)
        {
            // Get list of segment connection paths and all neuron connection paths for random selection

            connectionPaths = new List<List<byte>>();
            segmentIds = new List<byte>();
            neuronReferences = new List<NeuronReference>();

            // Create recursive limit dict
            recursiveLimitInitial = new Dictionary<byte, byte>();
            foreach (SegmentGenotype segment in cg.segments)
            {
                recursiveLimitInitial[segment.id] = segment.recursiveLimit;
            }

            TraceConnections(cg, recursiveLimitInitial, null, new List<byte>(), segmentIds, connectionPaths, neuronReferences);

            // First mutate the outer graph, then the inner layer.
            // Legal values of inner depend on the topology of the outer.

            // 1. Node parameters subject to alteration.
            // Mutation freq for each parameter type.
            // Bools mutated by state flip. Scalars by adding
            // several random numbers for Gaussian-like distribution.
            // Adjustment scale relative to og value.
            // Can also be negated.
            // Clamped to legal bounds (what are the legal bounds?)
            // Limited legal values new value from set.
            foreach (SegmentGenotype sg in cg.segments)
            {
                foreach (NeuronGenotype ng in sg.neurons)
                {
                    if (mp.CoinFlip("n_t")) // Change type, updating inputs accordingly
                    {
                        // Mutate LOL
                    }
                    if (mp.CoinFlip("n_w1"))
                    {
                        // Mutate LOL
                    }
                    if (mp.CoinFlip("n_w2"))
                    {
                        // Mutate LOL
                    }
                    if (mp.CoinFlip("n_w3"))
                    {
                        // Mutate LOL
                    }
                }
            }


            // 2. New random node added to graph.
            GenerateRandomNeuronGenotype(cg, connectionPaths, segmentIds, neuronReferences); // please work please

            // 3. Connection parameters subjected to mutation.
            // Sometimes pointer moved to point to a different node at random.
            foreach (SegmentGenotype sg in cg.segments)
            {
                foreach (NeuronGenotype ng in sg.neurons)
                {
                    for (int i = 0; i < ng.inputs.Length; i++)
                    {
                        if (mp.CoinFlip("n_relocateinput"))
                        {
                            // Mutate LOL
                        }
                    }
                }
            }
        }
        // 5. Unconnected elements are garbage collected / simplified
        SimplifyCreatureGenotype(cg);

        return cg;
    }
}