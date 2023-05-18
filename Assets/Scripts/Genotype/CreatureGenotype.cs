using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class NeuronGenotype
{
    [Tooltip("0:Sum,1:Product,2:Divide,3:Sum-threshold,4:Greater-than")]
    public byte type; // 0,1,2,3..22

    public NeuronReference[] inputs;
    [Range(-15f, 15f)]
    public float[] weights; // -15 - 15
    public NeuronReference nr;

    public NeuronGenotype(byte type, NeuronReference[] inputs, NeuronReference nr)
    {
        this.type = type;
        this.inputs = inputs;
        this.nr = nr;
    }

    public NeuronGenotype(NeuronReference nr)
    {
        this.nr = nr;
        type = 0;
        weights = new float[0];
        inputs = new NeuronReference[0];
    }

    public NeuronGenotype Clone()
    {
        NeuronGenotype ng;
        if (inputs.Length == 0){
            ng = new NeuronGenotype(nr);
        } else {
            ng = new NeuronGenotype(type, (NeuronReference[])inputs.Clone(), nr);
            ng.weights = (float[])weights.Clone();
        }
        return ng;
    }
    public static byte GetTypeInputs(byte type)
    {
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
        return typeInputs;
    }
}

public enum NeuronReferenceRelativity { GHOST, PARENT, SELF, CHILD, TRACED };
// GHOST => isGhost, PARENT, SELF => isParent, isSelf, CHILD => connectionPath
// TRACED => stored as the NeuronGenotype's self identifier

[System.Serializable]
public struct NeuronReference
{
    //[Tooltip("-3:ghost\n-2:parent\n-1:self\n0>:child connection id")]
    //public int ownerSegment;

    public SN<NeuronReferenceRelativity> relativityNullable;
    public NeuronReferenceRelativity relativity
    {
        get
        {
            if (relativityNullable.Equals(null)) throw new System.Exception("Unexpected null relativity.");
            return (NeuronReferenceRelativity)relativityNullable;
        }
    }

    public SN<byte> relativeLevelNullable; // Used for PARENT only, 1 if direct parent, etc...
    public byte relativeLevel
    {
        get
        {
            if (relativeLevelNullable.Equals(null)) throw new System.Exception("Unexpected null relative level.");
            return (byte)relativeLevelNullable;
        }
        set
        {
            relativeLevelNullable = value;
        }
    }
    public List<byte> connectionPath; // Used for CHILD and TRACED only
    [Tooltip(
    "0-5:collider\n6-8:joint angle\n9-11:photosensors\n12:joint effector\n13>:other neurons"
    )]
    public byte id;

    // 0-5 - colliders
    // 6-8 - joint angle
    // 9-11 - photosensors
    // 12 - joint effector
    // 13-onwards - other neurons
}

[System.Serializable]
public class SegmentConnectionGenotype
{
    public byte id; // 0-255
    public byte destination;

    [Range(-0.5f, 0.5f)]
    public float anchorX; // -0.5 - 0.5
    [Range(-0.5f, 0.5f)]
    public float anchorY;
    [Range(-0.5f, 0.5f)]
    public float anchorZ;

    // Orientation four vars maybe...check Joint component vars.
    [HideInInspector]
    public float orientationX;
    [HideInInspector]
    public float orientationY;
    [HideInInspector]
    public float orientationZ;
    [HideInInspector]
    public float orientationW;

    // this is only used for player design
    public float eulerX;
    public float eulerY;
    public float eulerZ;

    [Range(0.20f, 2f)]
    public float scale = 1; // 0.20 - 2
    public bool reflected;
    public bool terminalOnly;

    public void SetOrientation(Vector3 eulerAngles)
    {
        Quaternion q = Quaternion.Euler(eulerAngles);
        orientationX = q.x;
        orientationY = q.y;
        orientationZ = q.z;
        orientationW = q.w;
    }

    public void EulerToQuat()
    {
        Quaternion q = Quaternion.Euler(eulerX, eulerY, eulerZ);
        orientationX = q.x;
        orientationY = q.y;
        orientationZ = q.z;
        orientationW = q.w;
    }

    public SegmentConnectionGenotype Clone()
    {
        SegmentConnectionGenotype scg = new SegmentConnectionGenotype();
        scg.id = id;
        scg.destination = destination;
        scg.anchorX = anchorX;
        scg.anchorY = anchorY;
        scg.anchorZ = anchorZ;

        scg.orientationX = orientationX;
        scg.orientationY = orientationY;
        scg.orientationZ = orientationZ;
        scg.orientationW = orientationW;

        scg.eulerX = eulerX;
        scg.eulerY = eulerY;
        scg.eulerZ = eulerZ;

        scg.scale = scale;
        scg.reflected = reflected;
        scg.terminalOnly = terminalOnly;
        return scg;
    }
}

[System.Serializable]
public enum JointType
{
    Fixed,
    HingeX,
    HingeY,
    HingeZ,
    Configurable,
    Spherical,
}

[System.Serializable]
public enum TrainingStage
{
    KSS,
    RL,
}

[System.Serializable]
public class SegmentGenotype
{
    public byte r;
    public byte g;
    public byte b;

    public byte id; // 0-ghost, 1-root, 2>-else
    public List<SegmentConnectionGenotype> connections;
    public List<NeuronGenotype> neurons;

    public byte recursiveLimit; //1-15

    [Range(0.05f, 3f)]
    public float dimensionX; // Random.Range(0.05f, 3f);
    [Range(0.05f, 3f)]
    public float dimensionY; // Random.Range(0.05f, 3f);
    [Range(0.05f, 3f)]
    public float dimensionZ; // Random.Range(0.05f, 3f);

    public JointType jointType;

    public static SegmentGenotype ghost
    {
        get
        {
            SegmentGenotype _ghost = new SegmentGenotype();
            _ghost.id = 0;
            return _ghost;
        }
    }

    public SegmentGenotype(){
        connections = new List<SegmentConnectionGenotype>();
        neurons = new List<NeuronGenotype>();
        recursiveLimit = 1;
        dimensionX = 1;
        dimensionY = 1;
        dimensionZ = 1;
        r = 1;
        g = 1;
        b = 1;
    }

    public NeuronGenotype GetNeuron(byte id)
    {
        foreach (NeuronGenotype nm in neurons)
        {
            if (nm.nr.id == id)
            {
                return nm;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the first connection (if any) of that id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public SegmentConnectionGenotype GetConnection(byte id)
    {
        foreach (SegmentConnectionGenotype scm in connections)
        {
            if (scm.id == id)
            {
                return scm;
            }
        }
        return null;
    }

    public SegmentGenotype Clone()
    {
        SegmentGenotype sg = new SegmentGenotype();
        sg.r = r;
        sg.g = g;
        sg.b = b;
        sg.id = id;
        sg.connections = connections.Select(item => item.Clone()).ToList();
        sg.neurons = neurons.Select(item => item.Clone()).ToList();
        sg.recursiveLimit = recursiveLimit;
        sg.dimensionX = dimensionX;
        sg.dimensionY = dimensionY;
        sg.dimensionZ = dimensionZ;
        sg.jointType = jointType;
        return sg;
    }

}

[System.Serializable]
public class CreatureGenotype
{
    public string name;
    public TrainingStage stage;
    public List<SegmentGenotype> segments;

    // Orientation four vars maybe...check Joint component vars.
    [HideInInspector]
    public float orientationX;
    [HideInInspector]
    public float orientationY;
    [HideInInspector]
    public float orientationZ;
    [HideInInspector]
    public float orientationW;

    // this is only used for player design
    public float eulerX;
    public float eulerY;
    public float eulerZ;

    // DEBUG
    public int counter = 0;

    public int obsDim;

    public int actDim;

    public SegmentGenotype GetSegment(byte id)
    {
        foreach (SegmentGenotype segment in segments)
        {
            if (segment.id == id)
            {
                return segment;
            }
        }
        return null;
    }

    public void EulerToQuat()
    {
        Quaternion q = Quaternion.Euler(eulerX, eulerY, eulerZ);
        orientationX = q.x;
        orientationY = q.y;
        orientationZ = q.z;
        orientationW = q.w;
    }

    private static readonly List<string> LatinWordParts = new List<string>
    {
        "aero", "albus", "ama", "ambi", "angui", "aqua", "ardea", "arvo", "aurum", "avium",
        "bellus", "bestia", "caelum", "canis", "cattus", "collis", "corvus", "crescens", "dextro",
        "dominus", "draco", "equus", "faunus", "felis", "ferox", "flumen", "gigas", "herba",
        "ignis", "leo", "lupus", "magnus", "mare", "montis", "mortis", "natura", "nox", "oliv",
        "ornis", "pelagus", "planta", "pontus", "pratum", "pulcher", "rex", "saxum", "serpens",
        "silva", "sol", "stellis", "terra", "umbra", "ventus", "vermis", "vesper", "vita"
    };

    public static string GenerateName(string parentName)
    {
        // Select two random Latin word parts
        int idx1 = Random.Range(0, LatinWordParts.Count);
        int idx2 = Random.Range(0, LatinWordParts.Count);
        string part1 = LatinWordParts[idx1];
        string part2 = LatinWordParts[idx2];

        // Combine the two word parts
        string generatedName = part1 + part2;

        // If there's a parent, add a prefix from its name
        if (!string.IsNullOrEmpty(parentName))
        {
            string uniquePart = parentName;

            // Check if the parent has a prefix
            if (parentName.Contains("-"))
            {
                // Split the parent's name and extract the unique part
                string[] nameParts = parentName.Split('-');
                uniquePart = nameParts.Length > 1 ? nameParts[1] : parentName;
            }

            // Take the first three letters of the unique part as the prefix
            string parentPrefix = uniquePart.Substring(0, Mathf.Min(3, uniquePart.Length));
            generatedName = parentPrefix + "-" + generatedName;
        }

        return generatedName;
    }

    /// <summary>
    /// Returns a dictionary of <child, parents></child>
    /// </summary>
    /// <returns></returns>
    public Dictionary<byte, byte> GetParentsDict(){
        Dictionary<byte, byte> parentsDict = new Dictionary<byte, byte>(); // (segmentId, parentId)
        Queue<SegmentGenotype> segmentsToSearch = new Queue<SegmentGenotype>();
        segmentsToSearch.Enqueue(GetSegment(1));
        while (segmentsToSearch.Count > 0){
            SegmentGenotype sg = segmentsToSearch.Dequeue();

            foreach (SegmentConnectionGenotype scg in sg.connections)
            {
                bool destSearched = parentsDict.ContainsKey(scg.destination);
                if (!destSearched) {
                    parentsDict.Add(scg.destination, sg.id);
                    segmentsToSearch.Enqueue(GetSegment(scg.destination));
                } 
            }
        }

        return parentsDict;
    }

    /// <summary>
    /// Traces through the creature genotype, returning a list of all the connections to a certain segment genotype
    /// </summary>
    /// <param name="isFull"></param>
    /// <param name="isRecursive"></param>
    /// <returns></returns>
    public Dictionary<byte, List<SegmentConnectionGenotype>> GetSegmentConnections(bool isFull, bool isRecursive){
        Dictionary<byte, List<SegmentConnectionGenotype>> segmentConnectionsByDest = new Dictionary<byte, List<SegmentConnectionGenotype>>();

        if (isFull){
            foreach (SegmentGenotype sg in segments)
            {
                foreach (SegmentConnectionGenotype scg in sg.connections)
                {
                    if (!isRecursive && scg.id == sg.id) continue;
                    if (segmentConnectionsByDest.ContainsKey(scg.destination))
                    {
                        segmentConnectionsByDest[scg.destination].Add(scg);
                    }
                    else
                    {
                        segmentConnectionsByDest.Add(scg.destination, new List<SegmentConnectionGenotype>() { scg });
                    }

                }
            }
        } else {
            
        }

        return segmentConnectionsByDest;
    }

    public SegmentGenotype GetParentSegmentGenotype(byte id){
        if (id == 0 || id == 1) return null;

        foreach (SegmentGenotype sg in segments)
        {
            if (sg.id == 0 || sg.id == id) continue;
            foreach (SegmentConnectionGenotype scg in sg.connections)
            {
                if (scg.destination == id){
                    return sg;
                }
            }
        }
        return null;
    }

    public System.Tuple<NeuronGenotype, SegmentGenotype> GetNeuronInput(NeuronReference guidingNR, SegmentGenotype requestingSG)
    {
        SegmentGenotype foundSegmentGenotype = null;
        if (guidingNR.relativity == NeuronReferenceRelativity.GHOST)
        {
            foundSegmentGenotype = GetSegment(0);
        }
        else if (guidingNR.relativity == NeuronReferenceRelativity.PARENT)
        {
            // This will be in a parent of the requesting neuron, so go through path to find correct relativeLevel
            int relativityLeft = guidingNR.relativeLevel;
            SegmentGenotype currentSegmentGenotype = requestingSG;
            while (relativityLeft != 0)
            {
                SegmentGenotype nextSegmentGenotype = GetParentSegmentGenotype(currentSegmentGenotype.id);
                if (nextSegmentGenotype != null)
                {
                    relativityLeft--;
                    currentSegmentGenotype = nextSegmentGenotype;
                } else {
                    break;
                }
            }
            foundSegmentGenotype = currentSegmentGenotype;
        }
        else if (guidingNR.relativity == NeuronReferenceRelativity.SELF)
        {
            foundSegmentGenotype = requestingSG;
        }
        else if (guidingNR.relativity == NeuronReferenceRelativity.CHILD)
        {
            SegmentGenotype currentSegmentGenotype = requestingSG.id == 0 ? GetSegment(1) : requestingSG;
            if (guidingNR.connectionPath == null || guidingNR.connectionPath.Count == 0){
                foundSegmentGenotype = currentSegmentGenotype;
            } else {
                foreach (byte connectionId in guidingNR.connectionPath)
                {
                    byte destId = currentSegmentGenotype.GetConnection(connectionId).destination;
                    currentSegmentGenotype = GetSegment(destId);
                }
                foundSegmentGenotype = currentSegmentGenotype;
            }
        }

        if (foundSegmentGenotype == null) return null;

        foreach (NeuronGenotype ng in foundSegmentGenotype.neurons)
        {
            if (ng.nr.id == guidingNR.id)
            {
                return new System.Tuple<NeuronGenotype, SegmentGenotype>(ng, foundSegmentGenotype);
            }
        }
        return null;
    }

    /// <summary>
    /// Returns a dictionary with key: destination, value: (parentId, number of connections to it)
    /// </summary>
    /// <param name="isFull"></param>
    /// <returns></returns>
    public Dictionary<byte, byte[]> GetSegmentParents(bool isFull, bool onlySelf)
    {
        Dictionary<byte, byte[]> segmentParentsByDest = new Dictionary<byte, byte[]>();

        if (isFull)
        {
            foreach (SegmentGenotype sg in segments)
            {
                foreach (SegmentConnectionGenotype scg in sg.connections)
                {
                    if (scg.destination == sg.id && onlySelf){
                        if (segmentParentsByDest.ContainsKey(scg.destination))
                        {
                            segmentParentsByDest[scg.destination][0]++;
                        }
                        else
                        {
                            segmentParentsByDest.Add(scg.destination, new byte[1] { 1 });
                        }
                    } else if (scg.destination != sg.id && !onlySelf) {
                        if (segmentParentsByDest.ContainsKey(scg.destination))
                        {
                            segmentParentsByDest[scg.destination][1]++;
                        }
                        else
                        {
                            segmentParentsByDest.Add(scg.destination, new byte[2] { sg.id, 1 });
                        }
                    }
                }
            }
        }
        else
        {

        }

        return segmentParentsByDest;
    }

    public CreatureGenotype Clone()
    {
        CreatureGenotype cg = new CreatureGenotype();
        cg.name = name;
        cg.segments = segments.Select(item => item.Clone()).ToList();
        cg.eulerX = eulerX;
        cg.eulerY = eulerY;
        cg.eulerZ = eulerZ;
        cg.orientationW = orientationW;
        cg.orientationX = orientationX;
        cg.orientationY = orientationY;
        cg.orientationZ = orientationZ;
        cg.stage = stage;
        return cg;
    }

    public void SaveData(string path, bool isFullPath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string fullPath = isFullPath ? path : Application.persistentDataPath + path;

        FileStream stream = new FileStream(fullPath, FileMode.Create);

        formatter.Serialize(stream, this);
        stream.Close();
    }

    public static CreatureGenotype LoadData(string path, bool isFullPath)
    {
        string fullPath = isFullPath ? path : Application.persistentDataPath + path;

        if (File.Exists(fullPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(fullPath, FileMode.Open);

            CreatureGenotype data = formatter.Deserialize(stream) as CreatureGenotype;

            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Error: Save file not found in " + fullPath);
            return null;
        }
    }

    /// <summary>
    /// Runs through entire creature genotype and counts the number of segments
    /// </summary>
    /// <param name="cg"></param>
    /// <param name="recursiveLimitValues"></param>
    /// <param name="myConnection"></param>
    /// <param name="connectionPath"></param>
    public void IterateSegment(CreatureGenotype cg, Dictionary<byte, byte> recursiveLimitValues,
            SegmentConnectionGenotype myConnection, List<byte> connectionPath, ref int segmentCount)
    {
        segmentCount++;

        // Find SegmentGenotype
        byte id = myConnection == null ? (byte)1 : myConnection.destination;

        SegmentGenotype currentSegmentGenotype = cg.GetSegment(id);

        if (currentSegmentGenotype == null) return;

        // Change recursiveLimit stuff
        bool runTerminalOnly = false;
        recursiveLimitValues[id]--;
        if (recursiveLimitValues[id] == 0)
        {
            runTerminalOnly = true;
        }

        foreach (SegmentConnectionGenotype connection in currentSegmentGenotype.connections)
        {

            if (recursiveLimitValues[connection.destination] > 0)
            {
                if (!runTerminalOnly && connection.terminalOnly)
                {
                    continue;
                }
                Dictionary<byte, byte> recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                List<byte> connectionPathClone = connectionPath.Select(item => (byte)item).ToList();
                //List<byte> connectionPathClone = new List<byte>(connectionPath);
                connectionPathClone.Add(connection.id);
                IterateSegment(cg, recursiveLimitClone, connection, connectionPathClone, ref segmentCount);
            }
        }
    }

    /// <summary>
    /// Calculates dimensions of observation and action vectors
    /// </summary>
    void CalculateDims()
    {
        // Initialize recurisve limit tracker
        Dictionary<byte, byte> recursiveLimitInitial = new Dictionary<byte, byte>();
        foreach (SegmentGenotype segment in segments) recursiveLimitInitial[segment.id] = segment.recursiveLimit;

        int segmentCount = 0;

        // Iterate
        IterateSegment(this, recursiveLimitInitial, null, new List<byte>(), ref segmentCount);

        // Set resultant dims
        actDim = segmentCount - 1;
        obsDim = segmentCount * 12;
    }
}

