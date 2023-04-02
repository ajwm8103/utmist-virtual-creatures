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

    [System.Serializable]
    public class MutationPreferenceSetting
    {
        public bool mutateMorphology = true;
        public bool mutateNeural = true;
        public float stdevSizeAdjustmentFactor = 0.3f;

        [System.Serializable]
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
            {"s_rl", new MutationPreference(0.25f, 0.25f)}, // Recursive limit (byte, 1:15)
            {"s_dx", new MutationPreference(0.25f, 0.25f)}, // Dimension X (float, 0.05f:1.2f)
            {"s_dy", new MutationPreference(0.25f, 0.25f)}, // Dimension Y (float, 0.05f:1.2f)
            {"s_dz", new MutationPreference(0.25f, 0.25f)}, // Dimension Z (float, 0.05f:1.2f)
            {"s_jt", new MutationPreference(0.25f, 0.25f)}, // JointType (enum, 0:3)
            {"s_dest", new MutationPreference(0.25f, 0.25f)}, // Destination (byte, 1:255)
            {"s_a", new MutationPreference(0.25f, 0.25f)}, // Anchor (sadness)
            {"s_o", new MutationPreference(0.25f, 0.25f)}, // Orientation (float, 0f:360f)
            {"s_s", new MutationPreference(0.25f, 0.25f)}, // Scale (float, 0.2f:1.2f)
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
            {"s_dx", new float[2]{0.05f,1.2f}},
            {"s_dy", new float[2]{0.05f,1.2f}},
            {"s_dz", new float[2]{0.05f,1.2f}},
            {"s_o", new float[2]{0f,360f}},
            {"s_s", new float[2]{0.2f,1.2f}},
            {"n_w1", new float[2]{-15f,15f}},
            {"n_w2", new float[2]{-15f,15f}},
            {"n_w3", new float[2]{-15f,15f}},
        };

        public Dictionary<string, byte[]> byteClamps = new Dictionary<string, byte[]>() {
            {"s_r", new byte[2]{0,255}},
            {"s_g", new byte[2]{0,255}},
            {"s_b", new byte[2]{0,255}},
            {"s_rl", new byte[2]{1,3}}, // temporary fix, used to be max 15
            {"s_dest", new byte[2]{1,255}},
        };

        public bool CoinFlip(string parameter)
        {
            return MutateGenotype.CoinFlip(mutationFrequencies[parameter].mutationChance * currentScaleFactor);
        }

        public float GetFloatMean(string parameter) {
            return (floatClamps[parameter][1] - floatClamps[parameter][0]) / 2f;
        }

        public byte GetByteMean(string parameter)
        {
            return (byte)Mathf.RoundToInt((byteClamps[parameter][1] - byteClamps[parameter][0]) / 2f);
        }

        public float GetRandomFloat(string parameter) {
            return ModifyFloat(GetFloatMean(parameter), parameter);
        }

        public byte GetRandomByte(string parameter)
        {
            return ModifyByte(GetByteMean(parameter), parameter);
        }

        public float ModifyFloat(float mean, string parameter)
        {
            /// Modifies float using Gaussian distribution where stdev is scaled by mean
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

    public static void SimplifyCreatureGenotype(ref CreatureGenotype cg)
    {
        if (cg.GetSegment(0) == null) {

            cg.segments.Insert(0, SegmentGenotype.ghost);
        }

        // Correct any terminal only flags
        foreach (SegmentGenotype sg in cg.segments)
        {
            bool canTerminalOnly = false;
            List<SegmentConnectionGenotype> toFlip = new List<SegmentConnectionGenotype>();
            foreach (SegmentConnectionGenotype scg in sg.connections)
            {
                if (scg.terminalOnly) {
                    if (scg.destination == sg.id && scg.terminalOnly) {
                        // Can never terminal only to self
                        scg.terminalOnly = false;
                    } else {
                        toFlip.Add(scg);
                    }
                }
                if (scg.destination == sg.id) {
                    // If connecting to self, then terminal only outward is valid
                    canTerminalOnly = true;
                    break;
                }
            }
            if (!canTerminalOnly) {
                // Turn all flags to false
                foreach (SegmentConnectionGenotype scg in toFlip)
                {
                    scg.terminalOnly = false;
                }
            }
        }

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

        /// adds everything connected to root (id: 1) to the queue
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
                segmentConnectionsByDest.Add(scg.destination, new List<SegmentConnectionGenotype>() { scg });
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
            for (byte i = 2; i < sg.id; i++)
            {
                if (cg.GetSegment(i) == null)
                {
                    //Debug.Log($"Replacing old id {sg.id} with new id {i}.");
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

    public static void TraceConnectionRoot(CreatureGenotype cg, out List<byte> segmentIds,
            out List<List<byte>> connectionPaths, out List<NeuronReference> neuronReferences)
    {
        segmentIds = new List<byte>();
        connectionPaths = new List<List<byte>>();
        neuronReferences = new List<NeuronReference>();

        TraceConnections(cg, null, null, new List<byte>(), segmentIds, connectionPaths, neuronReferences, null);
    }

    public static void TraceConnectionOther(CreatureGenotype cg, SegmentGenotype sg, out List<byte> segmentIds,
            out List<List<byte>> connectionPaths, out List<NeuronReference> neuronReferences)
    {
        segmentIds = new List<byte>();
        connectionPaths = new List<List<byte>>();
        neuronReferences = new List<NeuronReference>();
        SegmentConnectionGenotype scg = new SegmentConnectionGenotype(); // dummy connection
        scg.destination = sg.id;
        TraceConnections(cg, null, scg, new List<byte>(), segmentIds, connectionPaths, neuronReferences, sg.id);
    }

    /// <summary>
    /// Runs through entire creature genotype and fills out connectionPaths, segmentIds, neuronReferences
    /// Also updates NeuronGenotype's self-NeuronReferences to contain their connection path
    /// </summary>
    /// <param name="cg"></param>
    /// <param name="recursiveLimitValues">Values of recursive limit for each SegmentGenotype for tracking.</param>
    /// <param name="myConnection">Connection the current SegmentGenotype originated from.</param>
    /// <param name="connectionPath">The full path of connection ids the current SegmentGenotype originated through.</param>
    /// <param name="segmentIds">The id of the segment that results from some path in connectionPaths.</param>
    /// <param name="connectionPaths">A list of full paths to every to-be-instantiated Segment</param>
    /// <param name="neuronReferences">A list of full paths </param>
    public static void TraceConnections(CreatureGenotype cg, Dictionary<byte, byte> recursiveLimitValues,
            SegmentConnectionGenotype myConnection, List<byte> connectionPath, List<byte> segmentIds,
            List<List<byte>> connectionPaths, List<NeuronReference> neuronReferences, byte? initialSegmentId)
    {
        if (segmentIds == null || connectionPaths == null || neuronReferences == null){
            throw new System.Exception("Unexpected null input");
        }

        if (recursiveLimitValues == null){
            recursiveLimitValues = new Dictionary<byte, byte>();
            foreach (SegmentGenotype segment in cg.segments)
            {
                recursiveLimitValues[segment.id] = segment.recursiveLimit;
            }
        }

        cg.counter++;
        if (initialSegmentId == null) {
            //Debug.Log(cg.counter + " null");
        } else {
            //Debug.Log(cg.counter + " " + (byte)initialSegmentId);
        }

        if (cg.counter == 60)
        {
            Debug.Log("Likely looping trace, save for debug.");
            string name = "/debug_" + Random.Range(0, 100) + ".creature";
            cg.SaveData(name, false);
            Debug.Log("Saved to " + Application.persistentDataPath + name);
        }

        // Find SegmentGenotype
        byte id;
        if (myConnection == null)
        {
            // Do the stuff for Ghost
            id = 0;
            SegmentGenotype ghostSegmentGenotype = cg.GetSegment(id);

            // Add neurons
            //Debug.Log("segment id 0 " + ghostSegmentGenotype.neurons.Count);
            foreach (NeuronGenotype ng in ghostSegmentGenotype.neurons)
            {
                NeuronReference nr = new NeuronReference();
                nr.id = ng.nr.id;
                nr.connectionPath = connectionPath;
                nr.relativityNullable = NeuronReferenceRelativity.CHILD;
                //neuronReferences.Add(nr);
            }

            connectionPaths.Add(null);
            segmentIds.Add(id);
            
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
            //Debug.Log("segment id 1 " + rootSegmentGenotype.neurons.Count);
            // Add neurons
            if (initialSegmentId == null)
            {
                foreach (NeuronGenotype ng in rootSegmentGenotype.neurons)
                {
                    NeuronReference nr = new NeuronReference();
                    nr.id = ng.nr.id;
                    nr.connectionPath = connectionPath;
                    nr.relativityNullable = NeuronReferenceRelativity.CHILD;
                    neuronReferences.Add(nr);

                    ng.nr.connectionPath = connectionPath;
                    ng.nr.relativityNullable = NeuronReferenceRelativity.TRACED;
                }
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
                    Dictionary<byte, byte> recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                    List<byte> connectionPathClone = connectionPath.Select(item => (byte)item).ToList();
                    connectionPathClone.Add(connection.id);
                    TraceConnections(cg, recursiveLimitClone, connection, connectionPathClone, segmentIds, connectionPaths, neuronReferences, initialSegmentId);
                }
            }
        }
        else
        {
            id = myConnection.destination;
            SegmentGenotype currentSegmentGenotype = cg.GetSegment(id);


            //Debug.Log("segment id " + id + " count " + currentSegmentGenotype.neurons.Count);

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
                if (initialSegmentId == null || id != initialSegmentId){
                    NeuronReference nr = new NeuronReference();
                    nr.id = ng.nr.id;
                    nr.connectionPath = connectionPath;
                    nr.relativityNullable = NeuronReferenceRelativity.CHILD;
                    neuronReferences.Add(nr);
                }

                if (initialSegmentId == null)
                {
                    ng.nr.connectionPath = connectionPath;
                    ng.nr.relativityNullable = NeuronReferenceRelativity.TRACED;
                }
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
                    Dictionary<byte, byte> recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                    List<byte> connectionPathClone = new List<byte>(connectionPath);
                    connectionPathClone.Add(connection.id);
                    TraceConnections(cg, recursiveLimitClone, connection, connectionPathClone, segmentIds, connectionPaths, neuronReferences, initialSegmentId);
                }
            }
        }

    }

    public static CreatureGenotype GenerateRandomCreatureGenotype(MutationPreferenceSetting mp)
    {
        // TODO: May need reference to MutationPreferenceSetting?
        return null;
    }

    public static SegmentGenotype GenerateRandomSegmentGenotype(ref CreatureGenotype cg, MutationPreferenceSetting mp)
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
                generatedSegmentGenotype.recursiveLimit = mp.GetRandomByte("s_rl");
                generatedSegmentGenotype.dimensionX = mp.GetRandomFloat("s_dx");
                generatedSegmentGenotype.dimensionY = mp.GetRandomFloat("s_dy");
                generatedSegmentGenotype.dimensionZ = mp.GetRandomFloat("s_dz");
                generatedSegmentGenotype.jointType = (JointType)Random.Range(0, 4);

                cg.segments.Add(generatedSegmentGenotype);
                return generatedSegmentGenotype;
            }
        }
        return null;
    }

    public static void GenerateRandomNeuronGenotype(CreatureGenotype cg, List<List<byte>> connectionPaths, List<byte> segmentIds, List<NeuronReference> rootNeuronReferences)
    {
        // Select random SegmentGenotype
        byte segmentId = (byte)Random.Range(0, cg.segments.Count);
        //Debug.Log("SegmentID " + segmentId);
        //Debug.Log("rootNeuronReferences count " + rootNeuronReferences.Count);
        SegmentGenotype segmentGenotype = cg.GetSegment(segmentId);

        // Create NeuronReference
        NeuronReference spawnedNeuronReference = new NeuronReference();
        spawnedNeuronReference.connectionPath = null;
        spawnedNeuronReference.relativityNullable = null;
        spawnedNeuronReference.relativeLevelNullable = null;

        // Pick unused id
        for (byte i = 13; i < 255; i++)
        {
            if (segmentGenotype.GetNeuron(i) == null)
            {
                // Found unused id, add node here
                spawnedNeuronReference.id = i;
                break;
            }
        }

        // Set a neuron type
        byte type = (byte)Random.Range(0, 23);
        byte typeInputs = NeuronGenotype.GetTypeInputs(type);

        // Find all valid inputs for the node and connect them
        // Valid inputs will be within the node, the parent of the node, a child of the node, or the ghost
        // Make list of all those then select randomly
        List<NeuronReference> possibleNeurons = new List<NeuronReference>();

        // Always add ghost neurons
        foreach (NeuronGenotype ng in cg.GetSegment(0).neurons)
        {
            if (ng.nr.id != 12) // Remove effectors
            {
                NeuronReference nr = new NeuronReference();
                nr.relativityNullable = NeuronReferenceRelativity.GHOST;
                nr.id = ng.nr.id;
                possibleNeurons.Add(nr);
            }
        }

        // Always add own neurons if not ghost
        if (segmentId != 0){
            foreach (NeuronGenotype ng in segmentGenotype.neurons)
            {
                if (ng.nr.id != 12) // Remove effectors
                {
                    NeuronReference nr = new NeuronReference();
                    nr.relativityNullable = NeuronReferenceRelativity.SELF;
                    nr.id = ng.nr.id;
                    possibleNeurons.Add(nr);
                }
            }
        }

        if (segmentId == 0)
        {
            // Is Ghost, add all from root
            foreach (NeuronReference nr in rootNeuronReferences)
            {
                if (nr.id != 12) // Is sensor or neuron, and not repeating ghost
                {
                    possibleNeurons.Add(nr);
                }
            }
        } else if (segmentId == 1)
        {
            // Is root, add all from root
            // Trace outward
            List<byte> otherSegmentIds; List<List<byte>> otherConnectionPaths; List<NeuronReference> otherNeuronReferences;
            // TODO: Adjust below line to start from non-root node outward
            TraceConnectionOther(cg, segmentGenotype, out otherSegmentIds, out otherConnectionPaths, out otherNeuronReferences);

            foreach (NeuronReference nr in otherNeuronReferences)
            {
                if (nr.id != 12) // Is sensor or neuron, and not repeating ghost
                {
                    possibleNeurons.Add(nr);
                }
            }
        }
        else
        {
            // Non-root, so trace outward
            List<byte> otherSegmentIds; List<List<byte>> otherConnectionPaths; List<NeuronReference> otherNeuronReferences;
            // TODO: Adjust below line to start from non-root node outward
            TraceConnectionOther(cg, segmentGenotype, out otherSegmentIds, out otherConnectionPaths, out otherNeuronReferences);

            //Debug.Log("otherNeuronReferences count " + otherNeuronReferences.Count);
            // Add child segments' neurons
            foreach (NeuronReference nr in otherNeuronReferences)
            {
                if (nr.id != 12) // Is not an effector
                {
                    possibleNeurons.Add(nr);
                }
            }

            // Find segment's id in segmentIds
            /*SegmentGenotype currentSG; int i = 0;
            while (currentSG != segmentGenotype){
                currentSG = currentSG.connections[connectionPaths[i]]
            }*/
            int segmentIndex = segmentIds.IndexOf(segmentId);

            if (segmentIndex == -1) {
                foreach (SegmentGenotype sg in cg.segments)
                {
                    Debug.Log("sg id " + sg.id + " recursiveLimit " + sg.recursiveLimit);
                    foreach (SegmentConnectionGenotype scg in sg.connections)
                    {
                        Debug.Log("Connection id " + scg.id + " to segment id " + scg.destination);
                        Debug.Log("Terminal only " + scg.terminalOnly);
                    }
                }
                foreach (byte idd in segmentIds)
                {
                    Debug.Log("idd " + idd);
                }
                throw new System.Exception("No segmentId " + segmentId + " out of " + cg.segments.Count + " segments.");
            }

            List<SegmentGenotype> parentGenotypes = new List<SegmentGenotype>();
            Dictionary<byte, byte> parentsDict = cg.GetParentsDict();
            byte currentId = segmentId;
            while (currentId != 1)
            {
                byte nextId = parentsDict[currentId];
                if (nextId != currentId)
                {
                    // Found a parent segment
                    parentGenotypes.Add(cg.GetSegment(nextId));
                }
                currentId = nextId;
            }

            //Debug.Log("parentGenotypes count " + parentGenotypes.Count);
            // Add parent segments' neurons
            for (int i = 0; i < parentGenotypes.Count; i++)
            {
                //Debug.Log("n " + parentGenotypes[i].neurons.Count);
                foreach (NeuronGenotype ng in parentGenotypes[i].neurons)
                {
                    if (ng.nr.id != 12) // Is not an effector
                    {
                        NeuronReference nr = new NeuronReference();
                        nr.id = ng.nr.id;
                        nr.relativityNullable = NeuronReferenceRelativity.PARENT;
                        nr.relativeLevel = (byte)(i + 1);
                        possibleNeurons.Add(nr);
                    }
                }
            }
        }

        if (possibleNeurons.Count == 0){
            throw new System.Exception("No possible Neurons for segmentId " + segmentId);
        }
        
        // Select random input NeuronReferences
        NeuronReference[] neuronInputs = new NeuronReference[typeInputs];
        float[] neuronWeights = new float[typeInputs];
        for (int i = 0; i < typeInputs; i++)
        {
            // Select random id
            int selectedNeuronId = Random.Range(0, possibleNeurons.Count);

            //Debug.Log(i);
            //Debug.Log(selectedNeuronId);
            // Add random neuron to neuronInputs
            neuronInputs[i] = possibleNeurons[selectedNeuronId];
            if (neuronInputs[i].relativityNullable.Equals(null)){
                //Debug.Log("null");
                throw new System.Exception("Null neuron input relativity.");
            } else {
                //Debug.Log((NeuronReferenceRelativity)neuronInputs[i].relativityNullable);
            }

            // Set random weight
            neuronWeights[i] = Random.Range(-15f, 15f);
        }

        // Create and install NeuronGenotype
        NeuronGenotype ngOut = new NeuronGenotype(type, neuronInputs, spawnedNeuronReference);
        ngOut.weights = neuronWeights;
        //Debug.Log(string.Format("Added neuron with type {0}, {1} inputs, on segment id {2}", type.ToString(), neuronInputs.Length.ToString(), segmentId.ToString()));
        segmentGenotype.neurons.Add(ngOut);
    }

    [System.Obsolete("GenerateRandomNeuronGenotypeOld is deprecated, please use GenerateRandomNeuronGenotype instead.", true)]
    public static void GenerateRandomNeuronGenotypeOld(CreatureGenotype cg, List<List<byte>> connectionPaths, List<byte> segmentIds, List<NeuronReference> neuronReferences)
    {
        // Pick a random segment path
        int segmentSelectionId = Random.Range(0, connectionPaths.Count);
        List<byte> segmentPath = connectionPaths[segmentSelectionId];

        byte segmentId = segmentIds[segmentSelectionId];
        SegmentGenotype segmentGenotype = cg.GetSegment(segmentId);

        // Make the neuron reference
        NeuronReference spawnedNeuronReference = new NeuronReference();
        spawnedNeuronReference.connectionPath = segmentPath;
        //spawnedNeuronReference.isGhost = segmentPath == null; // if no segment path, neuron was chosen to spawn in ghost

        // Pick unused id
        for (byte i = 13; i < 255; i++)
        {
            if (segmentGenotype.GetNeuron(i) == null)
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
                if (nr.relativity == NeuronReferenceRelativity.GHOST) // Is ghost
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
        segmentGenotype.neurons.Add(ng);
        //return true;
    }

    // TODELETE B/C DEBUG
    public List<List<byte>> cp1 = new List<List<byte>>();
    public List<NeuronReference> nr1 = new List<NeuronReference>();

    public static CreatureGenotype MutateCreatureGenotype(CreatureGenotype cg, MutationPreferenceSetting mp)
    {
        //Debug.Log("----MUTATING CREATURE----");
        cg = cg.Clone();
        SimplifyCreatureGenotype(ref cg);

        // test
        cg.name += Random.Range(0, 100);

        // Get list of segment connection paths and all neuron connection paths for random selection

        List<List<byte>> connectionPaths; List<byte> segmentIds; List<NeuronReference> neuronReferences;
        TraceConnectionRoot(cg, out segmentIds, out connectionPaths, out neuronReferences);
        //cp1 = connectionPaths;
        //nr1 = neuronReferences;


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
                /*if (mp.CoinFlip("s_jt"))
                {
                    sg.jointType = (JointType)Random.Range(0, 4);
                }*/
            }

            // 2. New random node added to graph.
            SegmentGenotype generatedSegmentGenotype = GenerateRandomSegmentGenotype(ref cg, mp);

            // TODO: Bring back commented out - mutate connection params and add connection

            // 3. Connection parameters subjected to mutation.
            // Sometimes pointer moved to point to a different node at random.
            // This is how neural nodes stay - they become the input of another neuron
            foreach (SegmentGenotype sg in cg.segments)
            {
                foreach (SegmentConnectionGenotype scg in sg.connections)
                {
                    if (mp.CoinFlip("s_dest"))
                    {
                        // Gather possible SegmentGenotypes
                        List<byte> possibleDestinations = new List<byte>();
                        foreach (SegmentGenotype sgSearch in cg.segments)
                        {
                            if (sgSearch.id == 0) continue;
                            possibleDestinations.Add(sgSearch.id);
                        }

                        // Check all unconnected nodes, hence excluding the current destination from the list
                        foreach (SegmentGenotype sgSearch in cg.segments)
                        {
                            foreach (SegmentConnectionGenotype scgSearch in sgSearch.connections)
                            {
                                if (possibleDestinations.Any(x => x == scgSearch.destination)) {
                                    possibleDestinations.Remove(scgSearch.destination);
                                }
                            }
                        }

                        if (possibleDestinations.Count != 0){
                            scg.destination = possibleDestinations[Random.Range(0, possibleDestinations.Count)];
                        }
                    }
                    if (mp.CoinFlip("s_a"))
                    {
                        // Actually horrible
                        // Ok fine, turn it into spherical coords, nudge that, turn back into cube
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
                sg.connections.RemoveAll(connection => mp.CoinFlip("s_removec"));
                /*List<SegmentConnectionGenotype> toRemove = new List<SegmentConnectionGenotype>();
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
                }*/
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

                            if (sg.connections.Where(c => c.destination == sg.id) != null)
                            {
                                // Already exists a self-connection
                                scg.destination = generatedSegmentGenotype.id;
                            } else {
                                // All other options create loops
                                scg.destination = (RandomBool() ? generatedSegmentGenotype : sg).id;
                            }
                            //scg.destination = cg.segments[Random.Range(0, cg.segments.Count)].id;

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

                            scg.scale = Random.Range(0.2f, 1.2f);
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
            SimplifyCreatureGenotype(ref cg);
        }
        if (mp.mutateNeural)
        {
            // Get list of segment connection paths and all neuron connection paths for random selection

            segmentIds = null;
            connectionPaths = null;
            neuronReferences = null;
            TraceConnectionRoot(cg, out segmentIds, out connectionPaths, out neuronReferences);

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
                    if (ng.weights.Length > 0 && mp.CoinFlip("n_w1"))
                    {
                        // Mutate LOL
                        ng.weights[0] = mp.ModifyFloat(ng.weights[0], "n_w1");
                    }
                    if (ng.weights.Length > 1 && mp.CoinFlip("n_w2"))
                    {
                        // Mutate LOL
                        ng.weights[1] = mp.ModifyFloat(ng.weights[1], "n_w2");
                    }
                    if (ng.weights.Length > 2 && mp.CoinFlip("n_w3"))
                    {
                        // Mutate LOL
                        ng.weights[2] = mp.ModifyFloat(ng.weights[2], "n_w3");
                    }
                }
            }


            // 2. New random node added to graph.
            GenerateRandomNeuronGenotype(cg, connectionPaths, segmentIds, neuronReferences); // please work please

            // 3. Connection parameters subjected to mutation.
            // Sometimes pointer moved to point to a different node at random.
            // TODO function for list of possible nodes, basically copy from generate random and use there and here
            foreach (SegmentGenotype sg in cg.segments)
            {
                foreach (NeuronGenotype ng in sg.neurons)
                {
                    foreach (NeuronReference nr in ng.inputs)
                    {
                        if (mp.CoinFlip("n_relocateinput"))
                        {
                            // Mutate LOL

                            // Pick a random segment path
                            int segmentSelectionId = Random.Range(0, connectionPaths.Count);
                            List<byte> segmentPath = connectionPaths[segmentSelectionId];

                            byte segmentId = segmentIds[segmentSelectionId];
                            SegmentGenotype SegmentGenotype = cg.GetSegment(segmentId);

                            if (segmentId == 0){
                                // Ghost

                            } else if (segmentId == sg.id) {
                                // Self

                            }// else if (segmentId)
                        }
                    }
                }
            }
        }

        // 4. Omitted for neural graph

        // 5. Unconnected elements are garbage collected / simplified
        SimplifyCreatureGenotype(ref cg);

        return cg;
    }
}
