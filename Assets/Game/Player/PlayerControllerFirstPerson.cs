using UnityEngine;

public class PlayerControllerFirstPerson : MonoBehaviour
{
    public Camera first_person_camera;
    public CharacterController character_controller;
    public Transform camera_transform;
    public float camera_sensitivity_x;
    public float camera_sensitivity_y;
    public float interaction_distance;
    public float friction;
    public float acceleration;

    [Header("INTERNAL")]
    public DeskInteraction current_interaction_selected;
    public Vector3 dp;
    public Vector3 ddp;
    public bool game_stopped;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (game_stopped) { resume_game(); }
            else              { stop_game(); }
        }

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

        { // Interaction
            Ray ray = first_person_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interaction_distance, 1 << 10))
            {
                current_interaction_selected = hit.collider.GetComponent<DeskInteraction>();
                GI.player_hud.show_interaction_message("Press Mouse 0 to play");
            }
            else
            {
                current_interaction_selected = null;
                GI.player_hud.hide_interaction_message();
            }
        }

        { // Input
            if (Input.GetMouseButtonDown(0) && current_interaction_selected)
            {
                if (current_interaction_selected.type == Interaction_Type.START_OR_END_GAME)
                {
                    GI.card_system.start_game();
                    current_interaction_selected = null;
                }
            }
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

        hide_cursor();
    }

    public void stop_game()
    {
        show_cursor();
        Time.timeScale = 0f;
        game_stopped = true;

        GI.player_hud.show_pause();
    }

    public void resume_game()
    {
        hide_cursor();
        Time.timeScale = 1f;
        game_stopped = false;

        GI.player_hud.hide_pause();
    }

    public void show_cursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void hide_cursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
