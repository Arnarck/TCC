using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject main_menu;
    public GameObject quit_menu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void go_to_quit_menu()
    {
        quit_menu.SetActive(true);
        main_menu.SetActive(false);
    }

    public void go_to_main_menu()
    {
        main_menu.SetActive(true);
        quit_menu.SetActive(false);
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
