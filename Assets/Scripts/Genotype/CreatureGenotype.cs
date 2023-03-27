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
    }

    public NeuronGenotype Clone()
    {
        NeuronGenotype ng = new NeuronGenotype(type, (NeuronReference[])inputs.Clone(), nr);
        ng.weights = (float[])weights.Clone();
        return ng;
    }

}

public enum NeuronReferenceRelativity { GHOST, RELATIVE, DIRECT };
// GHOST => isGhost, RELATIVE => isParent, isSelf, or child, DIRECT => connectionPath

[System.Serializable]
public struct NeuronReference
{
    //[Tooltip("-3:ghost\n-2:parent\n-1:self\n0>:child connection id")]
    //public int ownerSegment;

    public NeuronReferenceRelativity relativity;
    public bool isGhost;
    public bool isParent;
    public bool isSelf;
    public List<byte> connectionPath;
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

    public float anchorX; // -0.5 - 0.5
    public float anchorY;
    public float anchorZ;

    // Orientation four vars maybe...check Joint component vars.
    public float orientationX;
    public float orientationY;
    public float orientationZ;
    public float orientationW;

    // this is only used for player design
    public float eulerX;
    public float eulerY;
    public float eulerZ;

    public float scale; // 0.20 - 2
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

    public float dimensionX; // Random.Range(0.05f, 3f);
    public float dimensionY; // Random.Range(0.05f, 3f);
    public float dimensionZ; // Random.Range(0.05f, 3f);

    public JointType jointType;

    private static SegmentGenotype _ghost;
    public static SegmentGenotype ghost
    {
        get
        {
            if (_ghost == null)
            {
                _ghost = new SegmentGenotype();
                _ghost.id = 0;
            }
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

    public CreatureGenotype Clone()
    {
        CreatureGenotype cg = new CreatureGenotype();
        cg.name = name;
        cg.segments = segments.Select(item => item.Clone()).ToList();
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

