using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidDrag : MonoBehaviour
{
    public bool autoCorrectDensity = false;
    private float viscosityDrag = 1f;
    private float fluidDensity = 1000f;
    // private float dragScaling = 1f;
    private Rigidbody myRigidbody;
    private FluidManager fluidManager;

    // Surface Areas for each pair of faces (neg x will be same as pos x):
    public bool posXCovered = false;
    public bool negXCovered = false;
    public bool posYCovered = false;
    public bool negYCovered = false;
    public bool posZCovered = false;
    public bool negZCovered = false;

    private float sa_x;
    private float sa_y;
    private float sa_z;
    private float volume;
    private float C_D = 1.05f;

    // Use this for initialization
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();

        // Calculate surface areas for each face:
        sa_x = transform.localScale.y * transform.localScale.z;
        sa_y = transform.localScale.x * transform.localScale.z;
        sa_z = transform.localScale.x * transform.localScale.y;
        volume = transform.localScale.x * transform.localScale.y * transform.localScale.z;

        float areaConstraint = myRigidbody.mass / (Time.fixedDeltaTime * viscosityDrag);
        sa_x = Mathf.Min(sa_x, areaConstraint);
        sa_y = Mathf.Min(sa_y, areaConstraint);
        sa_z = Mathf.Min(sa_z, areaConstraint);

        // Store local parameters
        fluidManager = FluidManager.instance;
    }

    float LinearDragFloat(float A, float dot)
    {
        // TOOD: Probably incorrect
        return A * dot * viscosityDrag * C_D;
    }
    float QuadraticDragFloat(float A, float dot)
    {
        // 0.5 * rho * v^2 * C_D * A
        return 0.5f * fluidDensity * Mathf.Pow(dot, 2) * C_D * A;
    }

    void FixedUpdate()
    {
        if (fluidManager == null || !fluidManager.fluidEnabled) return;

        fluidDensity = fluidManager.fluidDensity;
        viscosityDrag = fluidManager.viscosityDrag;

        if (autoCorrectDensity){
            myRigidbody.mass = fluidDensity * volume;
        }

        // F_drag = 0.5 * C_D * A * rho * v^2
        // Cache positive axis vectors:
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;
        Vector3 right = transform.right;
        Vector3 com = (up * transform.localScale.y / 2) + transform.position;

        int Xcoef = 0;
        Xcoef += posXCovered ? 0 : 1;
        Xcoef += negXCovered ? 0 : 1;
        if (Xcoef != 0){
            Vector3 xpos_face_center = (up * transform.localScale.y / 2) + (right * transform.localScale.x / 2) + transform.position;
            Vector3 pointVelPosX = myRigidbody.GetPointVelocity(xpos_face_center);
            Vector3 fluidDragVecPosX;

            float dotX;
            dotX = -right.x * pointVelPosX.x +
                -right.y * pointVelPosX.y +
                -right.z * pointVelPosX.z;

            fluidDragVecPosX.x = right.x * dotX * sa_x * viscosityDrag * fluidDensity;
            fluidDragVecPosX.y = right.y * dotX * sa_x * viscosityDrag * fluidDensity;
            fluidDragVecPosX.z = right.z * dotX * sa_x * viscosityDrag * fluidDensity;

            myRigidbody.AddForceAtPosition(fluidDragVecPosX * Xcoef, xpos_face_center);
        }

        int Ycoef = 0;
        Ycoef += posYCovered ? 0 : 1;
        Ycoef += negYCovered ? 0 : 1;
        if (Ycoef != 0)
        {
            Vector3 ypos_face_center = (up * transform.localScale.y) + transform.position;
            Vector3 pointVelPosY = myRigidbody.GetPointVelocity(ypos_face_center);

            Vector3 fluidDragVecPosY;

            float dotY;
            dotY = -up.x * pointVelPosY.x +
                -up.y * pointVelPosY.y +
                -up.z * pointVelPosY.z;

            fluidDragVecPosY.x = up.x * dotY * sa_y * viscosityDrag * fluidDensity;
            fluidDragVecPosY.y = up.y * dotY * sa_y * viscosityDrag * fluidDensity;
            fluidDragVecPosY.z = up.z * dotY * sa_y * viscosityDrag * fluidDensity;

            myRigidbody.AddForceAtPosition(fluidDragVecPosY * Ycoef, ypos_face_center);
        }

        int Zcoef = 0;
        Zcoef += posZCovered ? 0 : 1;
        Zcoef += negZCovered ? 0 : 1;
        if (Zcoef != 0)
        {
            Vector3 zpos_face_center = (up * transform.localScale.y / 2) + (forward * transform.localScale.z / 2) + transform.position;
            Vector3 pointVelPosZ = myRigidbody.GetPointVelocity(zpos_face_center);
            Vector3 fluidDragVecPosZ;

            // Do the dot product first, but break it up and do it manually like this instead of calling the Dot() method:
            float dotZ;
            dotZ = -forward.x * pointVelPosZ.x +
                -forward.y * pointVelPosZ.y +
                -forward.z * pointVelPosZ.z;

            fluidDragVecPosZ.x = forward.x * dotZ * sa_z * viscosityDrag * fluidDensity;
            fluidDragVecPosZ.y = forward.y * dotZ * sa_z * viscosityDrag * fluidDensity;
            fluidDragVecPosZ.z = forward.z * dotZ * sa_z * viscosityDrag * fluidDensity;

            myRigidbody.AddForceAtPosition(fluidDragVecPosZ * Zcoef, zpos_face_center);
        }

        // Find centers of each of box's faces
        Vector3 xneg_face_center = (up * transform.localScale.y / 2) - (right * transform.localScale.x / 2) + transform.position;
        Vector3 yneg_face_center = transform.position;
        Vector3 zneg_face_center = (up * transform.localScale.y / 2) - (forward * transform.localScale.z / 2) + transform.position;

        //Debug.Log(string.Format("mass {0}, other {1}, {2}", myRigidbody.mass, -fluidDensity * volume, transform.parent.parent.parent.name));
        myRigidbody.AddForceAtPosition(-fluidDensity * Physics.gravity * volume, com, ForceMode.Force);

        //=== FOR EACH FACE of rigidbody box: ----------------------------------------
        //=== Get Velocity: ---------------------------------------------
        //=== Apply Opposing Force ----------------------------------------

        /*
        // FRONT (posZ):
        Vector3 pointVelPosZ = rigidbody.GetPointVelocity(zpos_face_center);
        // linear drag: Vector3 fluidDragVecPosZ = -forward * LinearDragFloat(sa_z, Vector3.Dot(forward, pointVelPosZ));
        Vector3 fluidDragVecPosZ = -forward * dragScaling * QuadraticDragFloat(sa_z, Vector3.Dot(forward, pointVelPosZ));
        rigidbody.AddForceAtPosition(fluidDragVecPosZ, zpos_face_center);  // Apply force at face's center, in the direction opposite the face normal

        // TOP (posY):
        Vector3 pointVelPosY = rigidbody.GetPointVelocity(ypos_face_center);
        // linear drag: Vector3 fluidDragVecPosY = -up * LinearDragFloat(sa_y, Vector3.Dot(up, pointVelPosY));
	    Vector3 fluidDragVecPosY = -up * dragScaling * QuadraticDragFloat(sa_y, Vector3.Dot(up, pointVelPosY));
        rigidbody.AddForceAtPosition(fluidDragVecPosY, ypos_face_center);

        // RIGHT (posX):
        Vector3 pointVelPosX = rigidbody.GetPointVelocity(xpos_face_center);
        // linear drag: Vector3 fluidDragVecPosX = -right * LinearDragFloat(sa_x, Vector3.Dot(right, pointVelPosX));
	    Vector3 fluidDragVecPosX = -right * dragScaling * QuadraticDragFloat(sa_x, Vector3.Dot(right, pointVelPosX));
        rigidbody.AddForceAtPosition(fluidDragVecPosX, xpos_face_center);*/

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;
        Vector3 right = transform.right;

        Vector3 xpos_face_center = (up * transform.localScale.y / 2) + (right * transform.localScale.x / 2) + transform.position;
        Vector3 ypos_face_center = (up * transform.localScale.y) + transform.position;
        Vector3 zpos_face_center = (up * transform.localScale.y / 2) + (forward * transform.localScale.z / 2) + transform.position;

        Gizmos.DrawWireSphere(xpos_face_center, 0.1f);
        Gizmos.DrawWireSphere(ypos_face_center, 0.1f);
        Gizmos.DrawWireSphere(zpos_face_center, 0.1f);
    }
}