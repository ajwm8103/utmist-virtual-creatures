using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidDrag : MonoBehaviour
{
    private float viscosityDrag = 1f;
    private float fluidDensity = 1000f;
    private float dragScaling = 1f;
    private Rigidbody myRigidbody;

    // Surface Areas for each pair of faces (neg x will be same as pos x):
    private float sa_x;
    private float sa_y;
    private float sa_z;
    private float C_D = 1.05f;

    // Use this for initialization
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();

        // Calculate surface areas for each face:
        sa_x = transform.localScale.y * transform.localScale.z;
        sa_y = transform.localScale.x * transform.localScale.z;
        sa_z = transform.localScale.x * transform.localScale.y;

        // Store local parameters
        FluidManager fluidManager = FluidManager.instance;
        fluidDensity = fluidManager.fluidDensity;
        viscosityDrag = fluidManager.viscosityDrag;
    }

    float LinearDragFloat(float area, float dot)
    {
        // TODO: Probably incorrect
        return area * dot * viscosityDrag * C_D;
    }
    float QuadraticDragFloat(float area, float dot)
    {
        // 0.5 * rho * v^2 * C_D * area
        return 0.5f * fluidDensity * Mathf.Pow(dot, 2) * C_D * area;
    }

    void FixedUpdate()
    {
        // F_drag = 0.5 * C_D * area * rho * v^2
        // Cache positive axis vectors:
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;
        Vector3 right = transform.right;

        // Find centers of each of box's faces
        Vector3 xpos_face_center = (right * transform.localScale.x / 2) + transform.position;
        Vector3 ypos_face_center = (up * transform.localScale.y / 2) + transform.position;
        Vector3 zpos_face_center = (forward * transform.localScale.z / 2) + transform.position;
        Vector3 xneg_face_center = -(right * transform.localScale.x / 2) + transform.position;
        Vector3 yneg_face_center = -(up * transform.localScale.y / 2) + transform.position;
        Vector3 zneg_face_center = -(forward * transform.localScale.z / 2) + transform.position;

        Vector3 pointVelPosZ = myRigidbody.GetPointVelocity(zpos_face_center);
        Vector3 pointVelPosY = myRigidbody.GetPointVelocity(ypos_face_center);
        Vector3 pointVelPosX = myRigidbody.GetPointVelocity(xpos_face_center);
        
        Vector3 pointVelNegZ = myRigidbody.GetPointVelocity(zneg_face_center);
        Vector3 pointVelNegY = myRigidbody.GetPointVelocity(yneg_face_center);
        Vector3 pointVelNegX = myRigidbody.GetPointVelocity(xneg_face_center);


        // Quadratic drag seems to break some stuff with symmetry, so using linear drag as in the original paper:

        Vector3 fluidDragVecPosZ, fluidDragVecNegZ;

        // Do the dot product first, but break it up and do it manually like this instead of calling the Dot() method:
        float dotPosZ, dotNegZ;

        dotPosZ = -forward.x * pointVelPosZ.x +
            -forward.y * pointVelPosZ.y +
            -forward.z * pointVelPosZ.z;

        dotNegZ = -forward.x * pointVelNegZ.x +
            -forward.y * pointVelNegZ.y +
            -forward.z * pointVelNegZ.z;


        fluidDragVecPosZ.x = forward.x * dotPosZ * sa_z * viscosityDrag;
        fluidDragVecPosZ.y = forward.y * dotPosZ * sa_z * viscosityDrag;
        fluidDragVecPosZ.z = forward.z * dotPosZ * sa_z * viscosityDrag;

        fluidDragVecNegZ.x = forward.x * dotNegZ * sa_z * viscosityDrag;
        fluidDragVecNegZ.y = forward.y * dotNegZ * sa_z * viscosityDrag;
        fluidDragVecNegZ.z = forward.z * dotNegZ * sa_z * viscosityDrag;

        myRigidbody.AddForceAtPosition(fluidDragVecPosZ + fluidDragVecNegZ, zpos_face_center);

        Vector3 fluidDragVecPosY, fluidDragVecNegY;

        float dotPosY, dotNegY;

        dotPosY = -up.x * pointVelPosY.x +
            -up.y * pointVelPosY.y +
            -up.z * pointVelPosY.z;

        dotNegY = -up.x * pointVelNegY.x +
            -up.y * pointVelNegY.y +
            -up.z * pointVelNegY.z;

        fluidDragVecPosY.x = up.x * dotPosY * sa_y * viscosityDrag;
        fluidDragVecPosY.y = up.y * dotPosY * sa_y * viscosityDrag;
        fluidDragVecPosY.z = up.z * dotPosY * sa_y * viscosityDrag;

        fluidDragVecNegY.x = up.x * dotNegY * sa_y * viscosityDrag;
        fluidDragVecNegY.y = up.y * dotNegY * sa_y * viscosityDrag;
        fluidDragVecNegY.z = up.z * dotNegY * sa_y * viscosityDrag;

        myRigidbody.AddForceAtPosition(fluidDragVecPosY * 2, ypos_face_center);

        Vector3 fluidDragVecPosX, fluidDragVecNegX;

        float dotPosX, dotNegX;

        dotPosX = -right.x * pointVelPosX.x +
            -right.y * pointVelPosX.y +
            -right.z * pointVelPosX.z;

        dotNegX = -right.x * pointVelNegX.x +
            -right.y * pointVelNegX.y +
            -right.z * pointVelNegX.z;

        fluidDragVecPosX.x = right.x * dotPosX * sa_x * viscosityDrag;
        fluidDragVecPosX.y = right.y * dotPosX * sa_x * viscosityDrag;
        fluidDragVecPosX.z = right.z * dotPosX * sa_x * viscosityDrag;

        fluidDragVecNegX.x = right.x * dotNegX * sa_x * viscosityDrag;
        fluidDragVecNegX.y = right.y * dotNegX * sa_x * viscosityDrag;
        fluidDragVecNegX.z = right.z * dotNegX * sa_x * viscosityDrag;

        myRigidbody.AddForceAtPosition(fluidDragVecPosX + fluidDragVecNegX, xpos_face_center);

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
}
