using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerController : MonoBehaviour
{
    [Header("Controls")]
    public float verticalSpeed = 4;
    public float horizontalSpeed = 4;

    Vector2 rotation = Vector2.zero;
    public float speed = 3;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        bool upKey = Input.GetKey(KeyCode.E);
        bool downKey = Input.GetKey(KeyCode.Q);
        int verticalDirection = 0;
        if (upKey)
        {
            verticalDirection = 1;
        }
        else if (downKey)
        {
            verticalDirection = -1;
        }
        transform.position += Vector3.up * verticalDirection * verticalSpeed * Time.deltaTime;

        float forwardKey = Input.GetAxis("Vertical");
        float sideKey = Input.GetAxis("Horizontal");

        Vector3 v3 = transform.forward;
        v3.y = 0;
        v3.Normalize();
        if (v3 != Vector3.zero)
        {
            transform.position += v3 * Input.GetAxis("Vertical") * horizontalSpeed * Time.deltaTime;
            transform.position += -Vector3.Cross(v3, Vector3.up).normalized * Input.GetAxis("Horizontal") * horizontalSpeed * Time.deltaTime;
        }
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        transform.eulerAngles = (Vector2)rotation * speed;
    }
}
