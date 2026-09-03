using UnityEngine;

public class PlayerControllerFirstPerson : MonoBehaviour
{
    public CharacterController character_controller;
    public Transform camera_transform;
    public float camera_sensitivity_x;
    public float camera_sensitivity_y;
    public float friction;
    public float acceleration;

    [Header("INTERNAL")]
    public Vector3 dp;
    public Vector3 ddp;

    private void Awake()
    {
        GI.player_first_person = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        { // Movement
            ddp = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) { ddp.z += 1f; }
            if (Input.GetKey(KeyCode.S)) { ddp.z -= 1f; }
            if (Input.GetKey(KeyCode.A)) { ddp.x -= 1f; }
            if (Input.GetKey(KeyCode.D)) { ddp.x += 1f; }

            ddp.Normalize();

            ddp *= acc();
            ddp -= friction * dp;

            dp += ddp * dt;
            Vector3 delta = (dp * dt) + ddp * (dt * dt * 0.5f);

            Vector3 move_amount = transform.right*delta.x + transform.forward*delta.z;
            character_controller.Move(move_amount);
        }

        { // Camera
            float amount_x =  Input.GetAxis("Mouse X")*camera_sensitivity_x*dt;
            float amount_y = -Input.GetAxis("Mouse Y")*camera_sensitivity_y*dt;
            transform.Rotate(Vector3.up*amount_x);
            camera_transform.Rotate(Vector3.right*amount_y);

            // Converts 0 - 360 to -180 - 180 (which makes easier to clamp the rotation later on)
            float camera_y_rotation = camera_transform.localEulerAngles.x;
            if (camera_y_rotation >= 180f)
            {
                camera_y_rotation -= 360f;
            }

            // Clamps the rotation angle
            camera_y_rotation = Mathf.Clamp(camera_y_rotation, -60f, 60f);

            // Converts -180 - 180 back to 0 - 360 to apply the rotation
            if (camera_y_rotation < 0f)
            {
                camera_y_rotation += 360f;
            }

            // Apply rotation
            camera_transform.localRotation = Quaternion.Euler(camera_y_rotation, 0f, 0f);
        }
    }

    public float acc()
    {
        return acceleration;
    }

    public void init()
    {
        camera_transform.gameObject.SetActive(true);
        gameObject.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
