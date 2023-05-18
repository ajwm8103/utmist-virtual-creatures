using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerController : MonoBehaviour
{
    [Header("Controls")]
    public float verticalSpeed = 4;
    public float horizontalSpeed = 4;
    public LayerMask layerMask;

    Vector2 rotation = Vector2.zero;
    public float speed = 3;
    private Camera mainCamera;
    TrainingManager tm;
    private bool trackingCursor = false;
    bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    // Start is called before the first frame update
    void Start()
    {
        tm = TrainingManager.instance;
        mainCamera = GetComponentInChildren<Camera>();
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

        float speedMultiplier = Input.GetKey(KeyCode.LeftControl) ? 2.5f : 1f;
        transform.position += Vector3.up * verticalDirection * verticalSpeed * speedMultiplier * Time.deltaTime;

        float forwardKey = Input.GetAxis("Vertical");
        float sideKey = Input.GetAxis("Horizontal");

        Vector3 v3 = transform.forward;
        v3.y = 0;
        v3.Normalize();
        if (v3 != Vector3.zero)
        {
            transform.position += v3 * Input.GetAxis("Vertical") * horizontalSpeed * speedMultiplier * Time.deltaTime;
            transform.position += -Vector3.Cross(v3, Vector3.up).normalized * Input.GetAxis("Horizontal") * horizontalSpeed * speedMultiplier * Time.deltaTime;
        }



        if (trackingCursor)
        {
            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");
            transform.eulerAngles = (Vector2)rotation * speed;
        }

        if (Input.GetKey(KeyCode.F)) {
            Creature bestCreature = tm.GetBestLivingCreature();
            if (bestCreature != null) {
                Vector3 com = bestCreature.GetCentreOfMass();
                if (com != null && !float.IsNaN(com.x) && !float.IsNaN(com.y) && !float.IsNaN(com.z))
                {
                    transform.position = bestCreature.GetCentreOfMass() + Vector3.up * 3;
                }
            }
        }



        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            trackingCursor = false;
        }

        bool noHoverManager = UIMouseHoverManager.instance == null;
        bool hoveringOverEmptySpace = !UIMouseHoverManager.instance.overUIElement && IsMouseOverGameWindow;
        if (Input.GetMouseButtonDown(0) && (noHoverManager || hoveringOverEmptySpace)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            trackingCursor = true;
        }

        if (Input.GetMouseButtonDown(1) && trackingCursor){
            DisplayStatsPanel dsp = DisplayStatsPanel.instance;
            if (dsp != null){
                // Calculate the center of the screen
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

                // Create a ray from the camera through the center of the screen
                Ray ray = mainCamera.ScreenPointToRay(screenCenter);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100f, layerMask))
                {
                    // Check if the hit object has a Segment component
                    Segment segment = hit.collider.gameObject.GetComponent<Segment>();
                    if (segment != null)
                    {
                        //Debug.Log(segment.creature);
                        dsp.UpdateCreatureStats(segment.creature);
                        CreatureViewerController.instance.SetCreature(segment.creature);
                    }
                }
            }
        }
    }
}
