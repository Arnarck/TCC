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
    public GameObject card_game_hud;
    public TextMeshProUGUI boss_attack_text;

    [Header("FIRST PERSON")]
    public GameObject first_person_hud;
    public GameObject interaction_panel;
    public TextMeshProUGUI interaction_text;

    [Header("INTERNAL")]
    public float boss_attack_text_t;

    private void Awake()
    {
        GI.player_hud = this;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (boss_attack_text_t > 0f)
        {
            boss_attack_text_t -= dt;
            if (boss_attack_text_t <= 0f)
            {
                boss_attack_text.enabled = false;
            }
        }
    }

    public void update_player_health_text()
    {
        player_health_text.text = "Chips: " + GI.player_card_game.health;
    }

    public void update_boss_health_text()
    {
        boss_health_text.text = "Boss's Chips: " + GI.boss.health;
    }

    public void update_actions_remaining_text()
    {
        actions_remaining_text.text = "Actions Remaining: " + GI.player_card_game.actions_remaining;
    }

    public void show_boss_attack_text(string text)
    {
        boss_attack_text.enabled = true;
        boss_attack_text.text = text;
        boss_attack_text_t = 2f;
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

    public void start_memorization_phase()
    {
        card_game_hud.SetActive(false);
    }

    public void end_memorization_phase()
    {
        card_game_hud.SetActive(true);
    }

    public void show_card_game_hud()
    {
        card_game_hud.SetActive(true);
        first_person_hud.SetActive(false);
    }

    public void show_first_person_hud()
    {
        first_person_hud.SetActive(true);
        card_game_hud.SetActive(false);
    }

    public void show_interaction_message(string message)
    {
        interaction_panel.SetActive(true);
        interaction_text.text = message;
    }

    public void hide_interaction_message()
    {
        interaction_panel.SetActive(false);
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
        if (GI.card_system.playing_card_game)
        {
            GI.player_card_game.resume_game();
        }
        else
        {
            GI.player_first_person.resume_game();
        }
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
