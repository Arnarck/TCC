using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI boss_health_text;
    public TextMeshProUGUI player_health_text;
    public TextMeshProUGUI actions_remaining_text;
    public GameObject player_turn_message;
    public GameObject boss_turn_message;
    public GameObject pause_ui;
    public GameObject pause_menu;
    public GameObject win_menu;
    public GameObject lose_menu;

    private void Awake()
    {
        GI.player_hud = this;
    }

    public void update_player_health_text()
    {
        player_health_text.text = "Chips: " + GI.player.health;
    }

    public void update_boss_health_text()
    {
        boss_health_text.text = "Boss's Chips: " + GI.boss.health;
    }

    public void update_actions_remaining_text()
    {
        actions_remaining_text.text = "Actions Remaining: " + GI.player.actions_remaining;
    }

    public void show_player_turn_message()
    {
        player_turn_message.SetActive(true);
    }

    public void show_boss_turn_message()
    {
        boss_turn_message.SetActive(true);
    }

    public void hide_player_turn_message()
    {
        player_turn_message.SetActive(false);
    }

    public void hide_boss_turn_message()
    {
        boss_turn_message.SetActive(false);
    }

    public void show_pause()
    {
        pause_ui.SetActive(true);
        pause_menu.SetActive(true);
    }

    public void hide_pause()
    {
        pause_ui.SetActive(false);
        pause_menu.SetActive(false);
    }

    public void show_win()
    {
        pause_ui.SetActive(true);
        win_menu.SetActive(true);
    }

    public void show_lose()
    {
        pause_ui.SetActive(true);
        lose_menu.SetActive(true);
    }

    public void resume()
    {
        GI.player.resume_game();
    }

    public void play_again()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
