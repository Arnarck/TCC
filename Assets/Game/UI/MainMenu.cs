using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Menu_Type
{
    SPLASH_SCREEN,
    MAIN,
    SETTINGS,
    CREDITS,
    QUIT,

    COUNT,
}

public class MainMenu : MonoBehaviour
{
    public GameObject main_menu;
    public GameObject quit_menu;
    public GameObject settings_menu;

    [Header("MENU ANIMATION")]
    public AnimationCurve panel_to_settings_animation_curve;
    public AnimationCurve file_settings_animation_curve;
    public float archive_animation_speed;
    public RectTransform panel_archive;
    public RectTransform panel_archive_main_menu_point;
    public RectTransform panel_archive_settings_menu_point;
    public RectTransform[] archives_papers;
    public RectTransform[] archive_papers_end_point_settings_menu;
    public RectTransform[] archive_papers_end_point_main_menu;

    [Header("INTERNAL")]
    public float archive_animation_t;
    public float file_animation_t;
    public Menu_Type next_menu;
    public Vector3 panel_archive_start_position;
    public Quaternion panel_archive_start_rotation;
    public Vector3[] archives_start_position;
    public Quaternion[] archives_start_rotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        next_menu = Menu_Type.MAIN;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (file_animation_t > 0f)
        {
            if (next_menu == Menu_Type.SETTINGS)
            {
                do_file_animation(archive_papers_end_point_settings_menu, false);
                if (file_animation_t <= 0f)
                {
                    settings_menu.SetActive(true);
                }
            }
            else if (next_menu == Menu_Type.MAIN)
            {
                do_file_animation(archive_papers_end_point_main_menu, true);
                if (file_animation_t <= 0f)
                {
                    archive_animation_t = 1f;
                }
            }
        }

        if (archive_animation_t > 0f)
        {
            if (next_menu == Menu_Type.SETTINGS)
            {
                do_archive_animation(panel_archive_settings_menu_point, false);
                if (archive_animation_t <= 0f)
                {
                    file_animation_t = 1f;
                }
            }
            else if (next_menu == Menu_Type.MAIN)
            {
                do_archive_animation(panel_archive_main_menu_point, false);
                if (archive_animation_t <= 0f)
                {
                    main_menu.SetActive(true);
                }
            }
        }
    }

    public void do_file_animation(Transform[] end_point, bool invert_direction)
    {
        file_animation_t -= Time.deltaTime*archive_animation_speed;
        if (file_animation_t <= 0f)
        {
            file_animation_t = 0f;
        }

        float animation_t = file_animation_t;
        if (!invert_direction)
        {
            animation_t = 1f - animation_t;
        }

        for (int i = 0; i < archives_papers.Length; i++)
        {
            archives_papers[i].position = Vector3.Lerp(archives_start_position[i],
                                                       end_point[i].position,
                                                       file_settings_animation_curve.Evaluate(animation_t));

            archives_papers[i].rotation = Quaternion.Slerp(archives_start_rotation[i],
                                                           end_point[i].rotation,
                                                           file_settings_animation_curve.Evaluate(animation_t));
        }
    }

    public void do_archive_animation(Transform end_point, bool invert_animation)
    {
        archive_animation_t -= Time.deltaTime*archive_animation_speed;
        if (archive_animation_t <= 0f)
        {
            archive_animation_t = 0f;
        }

        float animation_t = archive_animation_t;
        if (invert_animation)
        {
            animation_t = 1f - animation_t;
        }

        panel_archive.position = Vector3.Lerp(panel_archive_start_position,
                                              end_point.position,
                                              panel_to_settings_animation_curve.Evaluate(1f - animation_t));
        panel_archive.rotation = Quaternion.Slerp(panel_archive_start_rotation,
                                                  end_point.rotation,
                                                  panel_to_settings_animation_curve.Evaluate(1f - animation_t));

        if (archive_animation_t <= 0f)
        {
            for (int i = 0; i < archives_papers.Length; i++)
            {
                archives_start_position[i] = archives_papers[i].position;
                archives_start_rotation[i] = archives_papers[i].rotation;
            }
        }
    }

    public void go_to_settings_menu()
    {
        main_menu.SetActive(false);

        next_menu = Menu_Type.SETTINGS;
        archive_animation_t = 1f;
        panel_archive_start_position = panel_archive.position;
        panel_archive_start_rotation = panel_archive.rotation;
    }

    public void go_to_quit_menu()
    {
        quit_menu.SetActive(true);
        main_menu.SetActive(false);
    }

    public void go_to_main_menu()
    {
        quit_menu.SetActive(false);
        settings_menu.SetActive(false);

        next_menu = Menu_Type.MAIN;
        file_animation_t = 1f;
        panel_archive_start_position = panel_archive.position;
        panel_archive_start_rotation = panel_archive.rotation;
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
