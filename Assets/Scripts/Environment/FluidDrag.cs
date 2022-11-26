using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidDrag : MonoBehaviour
{
    public float viscosityDrag = 1f;
    private Rigidbody rigidBod;

    // Surface Areas for each pair of faces (neg x will be same as pos x):
    private float sa_x;
    private float sa_y;
    private float sa_z;
    private float C_D = 1.05f;

    // Use this for initialization
    void Start()
    {
        rigidBod = GetComponent<Rigidbody>();

        // Calculate surface areas for each face:
        sa_x = transform.localScale.y * transform.localScale.z;
        sa_y = transform.localScale.x * transform.localScale.z;
        sa_z = transform.localScale.x * transform.localScale.y;
    }

    float LinearDragFloat(float A, float dot)
    {
        return A * dot * viscosityDrag * C_D;
    }
    float QuadraticDragFloat(float A, float dot)
    {
        return A * dot * Mathf.Pow(viscosityDrag, 2) * C_D;
    }

    void FixedUpdate()
    {
        // F_drag = 0.5 * C_D * A * rho * v^2
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

        //=== FOR EACH FACE of rigidbody box: ----------------------------------------
        //=== Get Velocity: ---------------------------------------------
        //=== Apply Opposing Force ----------------------------------------

        // FRONT (posZ):
        Vector3 pointVelPosZ = rigidBod.GetPointVelocity(zpos_face_center);
        Vector3 fluidDragVecPosZ = -forward * LinearDragFloat(sa_z, Vector3.Dot(forward, pointVelPosZ));
        rigidBod.AddForceAtPosition(fluidDragVecPosZ * 2, zpos_face_center);  // Apply force at face's center, in the direction opposite the face normal
                                                                              // the multiplied by 2 is for the opposite symmetrical face to reduce # of computations

        // TOP (posY):
        Vector3 pointVelPosY = rigidBod.GetPointVelocity(ypos_face_center);
        Vector3 fluidDragVecPosY = -up * LinearDragFloat(sa_y, Vector3.Dot(up, pointVelPosY));
        rigidBod.AddForceAtPosition(fluidDragVecPosY * 2, ypos_face_center);

        // RIGHT (posX):
        Vector3 pointVelPosX = rigidBod.GetPointVelocity(xpos_face_center);
        Vector3 fluidDragVecPosX = -right * LinearDragFloat(sa_x, Vector3.Dot(right, pointVelPosX));
        rigidBod.AddForceAtPosition(fluidDragVecPosX * 2, xpos_face_center);

    }
}