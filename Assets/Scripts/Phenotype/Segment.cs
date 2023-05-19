using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyState
{

    public Vector3 velocity;
    public Vector3 angularVelocity;
    public float drag;
    public float angularDrag;
    public float mass;
    public bool useGravity;
    public bool freezeRotation;
    public Vector3 centerOfMass;
    public Vector3 worldCenterOfMass;
    public Quaternion inertiaTensorRotation;
    public Vector3 inertiaTensor;
    public bool detectCollisions;
    public Vector3 position;
    public Quaternion rotation;
    public RigidbodyInterpolation interpolation;

    public RigidbodyState(Rigidbody rb)
    {
        velocity = rb.velocity;
        angularVelocity = rb.angularVelocity;
        drag = rb.drag;
        angularDrag = rb.angularDrag;
        mass = rb.mass;
        useGravity = rb.useGravity;
        freezeRotation = rb.freezeRotation;
        centerOfMass = rb.centerOfMass;
        worldCenterOfMass = rb.worldCenterOfMass;
        inertiaTensorRotation = rb.inertiaTensorRotation;
        inertiaTensor = rb.inertiaTensor;
        detectCollisions = rb.detectCollisions;
        position = rb.position;
        rotation = rb.rotation;
        interpolation = rb.interpolation;
    }

    // even though this isn't passing by reference, it still works due to some 
    // oddity in Unity. WHO KNOWS. Using ref gives an error.
    public void SetRigidbody(Rigidbody rb)
    {
        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.mass = mass;
        rb.useGravity = useGravity;
        rb.freezeRotation = freezeRotation;
        rb.centerOfMass = centerOfMass;
        // read-only
        //rb.worldCenterOfMass = worldCenterOfMass;
        rb.inertiaTensorRotation = inertiaTensorRotation;
        rb.inertiaTensor = inertiaTensor;
        rb.detectCollisions = detectCollisions;
        rb.position = position;
        rb.rotation = rotation;
        rb.interpolation = interpolation;
    }
}

public class Segment : MonoBehaviour
{
    public byte id { get; private set; }
    public Dictionary<byte, Segment> children { get; private set; }
    public System.Tuple<byte, Segment> parent { get; private set; }
    public List<Neuron> neurons;
    public Creature creature { get; private set; }

    private RigidbodyState storedState;

    public bool isTopEmpty;
    public bool isBottomEmpty;
    public bool isRightEmpty;
    public bool isLeftEmpty;
    public bool isFrontEmpty;
    public bool isBackEmpty;

    public HingeJoint joint;
    public Rigidbody myRigidbody;
    public float jointAxisX;
    public float jointAxisY;
    public float jointAxisZ;

    public List<byte> path { get; private set; }

    void Awake()
    {
        joint = GetComponent<HingeJoint>();
        myRigidbody = GetComponent<Rigidbody>();
        children = new Dictionary<byte, Segment>();
        neurons = new List<Neuron>();
    }

    void FixedUpdate()
    {
        isTopEmpty = true;
        isBottomEmpty = true;
        isRightEmpty = true;
        isLeftEmpty = true;
        isFrontEmpty = true;
        isBackEmpty = true;

        if (joint != null)
        {
            Vector3 angles = (transform.localRotation * Quaternion.Inverse(joint.connectedBody.transform.localRotation)).eulerAngles;
            //Vector3 angles = Quaternion.FromToRotation(joint.connectedBody.transform.rotation.eulerAngles, transform.rotation.).eulerAngles;
            jointAxisX = angles.x;
            jointAxisY = angles.y;
            jointAxisZ = angles.z;

        } else {
            joint = GetComponent<HingeJoint>();
        }
    }

    public void StoreState(){
        storedState = new RigidbodyState(myRigidbody);
    }

    public void RestoreState(){
        storedState.SetRigidbody(myRigidbody);
    }

    public void SetId(byte id){
        this.id = id;
    }

    public void SetPath(List<byte> path){
        this.path = path;
    }

    public void SetParent(byte connectionId, Segment s){
        parent = new System.Tuple<byte, Segment>(connectionId, s);
    }

    public void SetCreature(Creature c){
        creature = c;
    }

    public void AddChild(byte connectionId, Segment s){
        children.Add(connectionId, s);
    }

    public void AddNeuron(Neuron n){
        neurons.Add(n);
    }

    public List<float> GetObservations(){
        // add 12 observations of the sensor to list, and return
        List<float> obs = new List<float>();
        obs.Add(GetContact("Right"));
        obs.Add(GetContact("Left"));
        obs.Add(GetContact("Top"));
        obs.Add(GetContact("Bottom"));
        obs.Add(GetContact("Front"));
        obs.Add(GetContact("Back"));
        obs.Add(jointAxisX);
        obs.Add(jointAxisY);
        obs.Add(jointAxisZ);
        obs.Add(GetPhotosensor(0));
        obs.Add(GetPhotosensor(1));
        obs.Add(GetPhotosensor(2));

        return obs;
    }

    public sbyte GetContact(string name)
    {
        bool value = name switch
        {
            "Top" => isTopEmpty,
            "Bottom" => isBottomEmpty,
            "Right" => isRightEmpty,
            "Left" => isLeftEmpty,
            "Front" => isFrontEmpty,
            "Back" => isBackEmpty,
            _ => true
        };
        return (sbyte)(value ? -1 : 1);
    }

    public float GetPhotosensor(int varNumber)
    {
        LightSource lightsource = FindObjectOfType<LightSource>();
        if (lightsource == null)
        {
            return 0;
        }
        else
        {
            Vector3 lspos = lightsource.transform.position;
            Vector3 normalVector = (lspos - transform.position).normalized;
            return varNumber switch
            {
                0 => normalVector.x,
                1 => normalVector.y,
                2 => normalVector.z,
                _ => 0,
            };
        }
    }

    public void HandleStay(Collider other, string name)
    {
        if (other.gameObject.layer != 6)
        {
            switch (name)
            {
                case ("Top"):
                    isTopEmpty = false;
                    break;
                case ("Bottom"):
                    isBottomEmpty = false;
                    break;
                case ("Right"):
                    isRightEmpty = false;
                    break;
                case ("Left"):
                    isLeftEmpty = false;
                    break;
                case ("Front"):
                    isFrontEmpty = false;
                    break;
                case ("Back"):
                    isBackEmpty = false;
                    break;
            }
        }
    }
}
