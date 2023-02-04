using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MutateGenotype;

public class CreatureSpawner : MonoBehaviour
{
    public Creature creaturePrefab;
    public GameObject segmentPrefab;

    
    public CreatureGenotype creatureGenotype;
    public List<CreatureGenotype> creatureGenotypeHistory;

    // Start is called before the first frame update
    void Start()
    {
        CreatureGenotype testCreature = new CreatureGenotype
        {
            name = "Test Creature"
        };

        SegmentGenotype testSegment = new SegmentGenotype
        {
            id = 1,
            jointType = JointType.Fixed,
            r = 255,
            g = 0,
            b = 0
        };

        SegmentConnectionGenotype testConnection1 = new SegmentConnectionGenotype
        {
            destination = 1,
            anchorX = 0.5f,
            anchorY = 0.0f,
            anchorZ = 0.0f,
            scale = 0.5f
        };

        SpawnCreature(creatureGenotype, (Vector3.up + Vector3.right));
        /*creatureGenotypeHistory.Add(creatureGenotype.Clone());
        for (int i = 0; i < 16; i++)
        {
            GetComponent<MutateGenotype>().MutateCreatureGenotype(creatureGenotype, new MutationPreferenceSetting());
            creatureGenotypeHistory.Add(creatureGenotype.Clone());
        }
        SpawnCreature(creatureGenotype, (Vector3.up + Vector3.right) * 2);*/
    }

    // Update is called once per frame
    void Update()
    {

    }

    bool VerifyCreatureGenotypeIntegrity(CreatureGenotype cm)
    {
        return true;
    }

    // Creature & GHOST (ID 0)
    void SpawnCreature(CreatureGenotype cm, Vector3 position)
    {
        // Verify
        if (!VerifyCreatureGenotypeIntegrity(cm))
        {
            return;
        }

        // Create recursive limit dict
        Dictionary<byte, byte> recursiveLimitInitial = new Dictionary<byte, byte>();
        foreach (SegmentGenotype segment in cm.segments)
        {
            recursiveLimitInitial[segment.id] = segment.recursiveLimit;
        }

        Creature c = Instantiate(creaturePrefab, Vector3.zero, Quaternion.identity);
        c.name = $"Creature ({cm.name})";

        // Add neurons
        SegmentGenotype ghost = cm.GetSegment(0);
        if (ghost != null)
        {
            foreach (NeuronGenotype nm in ghost.neurons)
            {
                c.AddNeuron(nm, null, null);
            }
        }

        SpawnSegment(cm, c, recursiveLimitInitial, position);
        c.InitializeCreature();


    }


    // Non-root (ID 2>)
    void SpawnSegment(CreatureGenotype cm, Creature c, Dictionary<byte, byte> recursiveLimitValues, SegmentConnectionGenotype myConnection, GameObject parentSegment, float parentGlobalScale, bool parentReflect, List<byte> connectionPath)
    {
        myConnection.EulerToQuat(); //Debug, remove later (this changes internal rotation storage stuff to make inspector editing easier.)


        byte id = myConnection.destination;
        //Debug.Log($"S: {myConnection.destination} ({recursiveLimitValues[id]})");

        // Find segmentGenotype
        SegmentGenotype currentSegmentGenotype = cm.GetSegment(id);

        if (currentSegmentGenotype == null)
            return;

        Transform parentTransform = parentSegment.transform;

        int reflectInt = myConnection.reflected ? -1 : 1;
        //bool otherReflectBool = myConnection.reflected ^ (Mathf.Sign(parentTransform.localScale.x)) == -1;
        int parentReflectInt = parentReflect ? -1 : 1;
        bool otherReflectBool = myConnection.reflected ^ parentReflect;
        int otherReflectInt = otherReflectBool ? -1 : 1;


        Vector3 spawnPos = parentTransform.position +
            parentTransform.right * parentTransform.localScale.x * myConnection.anchorX * reflectInt * parentReflectInt +
            parentTransform.up * parentTransform.localScale.y * (myConnection.anchorY + 0.5f) +
            parentTransform.forward * parentTransform.localScale.z * myConnection.anchorZ;

        Quaternion spawnAngle = new Quaternion(myConnection.orientationX, myConnection.orientationY, myConnection.orientationZ, myConnection.orientationW);

        if (otherReflectBool)
        {
            //spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, parentTransform.right), parentTransform.up);
            //spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, parentTransform.right), Vector3.Reflect(spawnAngle * Vector3.up, parentTransform.right));
            //spawnAngle = spawnAngle.eulerAngles
            //spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, parentTransform.right), Vector3.Reflect(spawnAngle * Vector3.up, parentTransform.right));
            //Quaternion mirrorNormalQuat = new Quaternion(parentTransform.right.x, parentTransform.right.y, parentTransform.right.z, 0);
            spawnAngle = Quaternion.LookRotation(Vector3.Reflect(spawnAngle * Vector3.forward, Vector3.right), Vector3.Reflect(spawnAngle * Vector3.up, Vector3.right));
            //spawnAngle = mirrorNormalQuat * spawnAngle;
            //spawnAngle *= Quaternion.Euler(parentTransform.up * 180);
        }
        spawnAngle *= parentTransform.rotation;
        GameObject spawnedSegmentGameObject = Instantiate(segmentPrefab, spawnPos, spawnAngle);

        spawnedSegmentGameObject.transform.parent = c.transform;
        spawnedSegmentGameObject.name = $"Segment {currentSegmentGenotype.id}";

        Vector3 dimVector = new Vector3(currentSegmentGenotype.dimensionX /* * otherReflectInt*/, currentSegmentGenotype.dimensionY, currentSegmentGenotype.dimensionZ);
        dimVector *= parentGlobalScale * myConnection.scale;
        spawnedSegmentGameObject.transform.localScale = dimVector;
        //spawnedSegment.GetComponent<BoxCollider>().size = dimVector;
        Transform spawnedGraphic = spawnedSegmentGameObject.transform.Find("Graphic");
        //spawnedGraphic.localScale = dimVector;
        spawnedGraphic.GetComponent<Renderer>().material.color = new Color(currentSegmentGenotype.r / 255f, currentSegmentGenotype.g / 255f, currentSegmentGenotype.b / 255f);

        Rigidbody rb = spawnedSegmentGameObject.GetComponent<Rigidbody>();
        rb.mass *= spawnedSegmentGameObject.transform.localScale.x * spawnedSegmentGameObject.transform.localScale.y * spawnedSegmentGameObject.transform.localScale.z;
        switch (currentSegmentGenotype.jointType)
        {
            case (JointType.Fixed):
                {
                    FixedJoint j = spawnedSegmentGameObject.AddComponent<FixedJoint>();
                    j.connectedBody = parentSegment.GetComponent<Rigidbody>();
                }
                break;

            case (JointType.HingeX):
                {
                    HingeJoint j = spawnedSegmentGameObject.AddComponent<HingeJoint>();
                    j.connectedBody = parentSegment.GetComponent<Rigidbody>();
                    j.axis = new Vector3(1, 0, 0);
                    j.useMotor = true;
                    JointMotor motor = j.motor;
                    motor.targetVelocity = 0;
                    motor.force = 100;
                    j.motor = motor;
                }
                break;

            case (JointType.HingeY):
                {
                    HingeJoint j = spawnedSegmentGameObject.AddComponent<HingeJoint>();
                    j.connectedBody = parentSegment.GetComponent<Rigidbody>();
                    j.axis = new Vector3(0, 1 * otherReflectInt, 0);
                    j.useMotor = true;
                    JointMotor motor = j.motor;
                    motor.targetVelocity = 0;
                    motor.force = 100;
                    j.motor = motor;
                }
                break;

            case (JointType.HingeZ):
                {
                    HingeJoint j = spawnedSegmentGameObject.AddComponent<HingeJoint>();
                    j.connectedBody = parentSegment.GetComponent<Rigidbody>();
                    j.axis = new Vector3(0, 0, 1 * otherReflectInt);
                    j.useMotor = true;
                    JointMotor motor = j.motor;
                    motor.targetVelocity = 0;
                    motor.force = 100;
                    j.motor = motor;
                }
                break;

            case (JointType.Spherical):
                {
                    ConfigurableJoint j = spawnedSegmentGameObject.AddComponent<ConfigurableJoint>();
                    j.connectedBody = parentSegment.GetComponent<Rigidbody>();
                    j.xMotion = ConfigurableJointMotion.Locked;
                    j.yMotion = ConfigurableJointMotion.Locked;
                    j.zMotion = ConfigurableJointMotion.Locked;
                    JointDrive jdx = j.angularXDrive;
                    jdx.positionSpring = 99999;
                    jdx.positionDamper = 99999;
                    j.angularXDrive = jdx;
                    JointDrive jdyz = j.angularYZDrive;
                    jdyz.positionSpring = 99999;
                    jdyz.positionDamper = 99999;
                    j.angularYZDrive = jdyz;
                    j.targetAngularVelocity = new Vector3(0, 0, 0);
                }
                break;

            default:
                return;
        }

        // Change recursiveLimit stuff
        bool runTerminalOnly = false;
        recursiveLimitValues[id]--;
        if (recursiveLimitValues[id] == 0 || !currentSegmentGenotype.connections.Any(scm => scm.destination == currentSegmentGenotype.id))
        {
            runTerminalOnly = true;
        }

        // Add neurons
        foreach (NeuronGenotype nm in currentSegmentGenotype.neurons)
        {
            nm.nr.connectionPath = connectionPath;
            if (nm.nr.id == 12)
            {
                c.AddNeuron(nm, spawnedSegmentGameObject.GetComponent<HingeJoint>(), null);
            }
            else if (nm.nr.id <= 11)
            {
                c.AddNeuron(nm, null, spawnedSegmentGameObject.GetComponent<Segment>());
            }
            else
            {
                c.AddNeuron(nm, null, null);
            }
        }

        foreach (SegmentConnectionGenotype connection in currentSegmentGenotype.connections)
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
                SpawnSegment(cm, c, recursiveLimitClone, connection, spawnedSegmentGameObject, parentGlobalScale * myConnection.scale, otherReflectBool, connectionPathClone);
            }
        }
    }

    // Root (ID 1)
    void SpawnSegment(CreatureGenotype cm, Creature c, Dictionary<byte, byte> recursiveLimitValues, Vector3 position)
    {
        //Debug.Log("S: ROOT");

        // Find segmentGenotype
        SegmentGenotype currentSegmentGenotype = cm.GetSegment(1);
        if (currentSegmentGenotype == null)
            return;

        GameObject spawnedSegmentGameObject = Instantiate(segmentPrefab, position, Quaternion.identity);
        spawnedSegmentGameObject.transform.parent = c.transform;
        spawnedSegmentGameObject.name = $"Segment {currentSegmentGenotype.id}";

        Vector3 dimVector = new Vector3(currentSegmentGenotype.dimensionX, currentSegmentGenotype.dimensionY, currentSegmentGenotype.dimensionZ);
        //spawnedSegment.GetComponent<BoxCollider>().size = dimVector;
        spawnedSegmentGameObject.transform.localScale = dimVector;
        Transform spawnedGraphic = spawnedSegmentGameObject.transform.Find("Graphic");
        //spawnedGraphic.localScale = dimVector;
        spawnedGraphic.GetComponent<Renderer>().material.color = new Color(currentSegmentGenotype.r / 255f, currentSegmentGenotype.g / 255f, currentSegmentGenotype.b / 255f);

        // Change recursiveLimit stuff
        bool runTerminalOnly = false;
        recursiveLimitValues[1]--;
        if (recursiveLimitValues[1] == 0 || !currentSegmentGenotype.connections.Any(scm => scm.destination == currentSegmentGenotype.id))
        {
            runTerminalOnly = true;
        }

        List<byte> connectionPath = new List<byte>();

        // Add neurons
        foreach (NeuronGenotype nm in currentSegmentGenotype.neurons)
        {
            nm.nr.connectionPath = connectionPath;

            if (nm.nr.id == 12)
            {
                c.AddNeuron(nm, spawnedSegmentGameObject.GetComponent<HingeJoint>(), null);
            }
            else if (nm.nr.id <= 11)
            {
                c.AddNeuron(nm, null, spawnedSegmentGameObject.GetComponent<Segment>());
            }
            else
            {
                c.AddNeuron(nm, null, null);
            }
        }


        foreach (SegmentConnectionGenotype connection in currentSegmentGenotype.connections)
        {

            if (recursiveLimitValues[connection.destination] > 0)
            {
                if (!runTerminalOnly && connection.terminalOnly)
                {
                    continue;
                }
                var recursiveLimitClone = recursiveLimitValues.ToDictionary(entry => entry.Key, entry => entry.Value);
                SpawnSegment(cm, c, recursiveLimitClone, connection, spawnedSegmentGameObject, 1, false, new List<byte>() { connection.id });
            }
        }
    }
}