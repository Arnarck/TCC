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
    public GameObject tutorial_menu;
    public GameObject back_to_main_menu_panel;
    public GameObject card_game_hud;
    public TextMeshProUGUI boss_attack_text;
    public Image pause_button_image;
    public Sprite resumed_sprite;
    public Sprite paused_sprite;

    [Header("BOSS CARD PREVIEW HUD")]
    public GameObject boss_card_preview_hud;
    public GameObject boss_card_families_to_disable_group;
    public TextMeshProUGUI boss_card_keyword_description_text;
    public TextMeshProUGUI boss_card_disable_price_text;
    public Image[] boss_card_families_to_disable_image;

    [Header("CARD PREVIEW HUD")]
    public GameObject card_preview_hud;
    public GameObject ability_requirements_panel;
    public TextMeshProUGUI card_preview_keyword_description_text;
    public Image[] card_preview_ability_requirements_image;
    public Sprite[] families_sprite;
    [TextArea(3, 15)]public string[] keywords_description;

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

    public void show_card_preview_hud(Card card)
    {
        card_preview_hud.SetActive(true);
        card_preview_keyword_description_text.text = keywords_description[(int)card.keyword_type];

        if (card.families_in_trio_to_activate_ability.Length < 1)
        {
            ability_requirements_panel.SetActive(false);
        }
        else
        {
            ability_requirements_panel.SetActive(true);
            for (int i = 0; i < card_preview_ability_requirements_image.Length; i++)
            {
                if (i < card.families_in_trio_to_activate_ability.Length)
                {
                    card_preview_ability_requirements_image[i].enabled = true;
                    card_preview_ability_requirements_image[i].sprite = families_sprite[(int)card.families_in_trio_to_activate_ability[i]];
                }
                else
                {
                    card_preview_ability_requirements_image[i].enabled = false;
                }
            }
        }

    }

    public void hide_card_preview_hud()
    {
        card_preview_hud.SetActive(false);
    }

    public void show_boss_card_preview_hud(BossCard boss_card)
    {
        boss_card_preview_hud.SetActive(true);

        boss_card_keyword_description_text.text = keywords_description[(int)boss_card.keyword_type];

        if (boss_card.disable_type == Boss_Card_Disable_Type.CHIPS)
        {
            boss_card_disable_price_text.enabled = true;
            boss_card_disable_price_text.text = "<color=yellow>" + boss_card.cost_to_disable_ability + "</color> Chips";

            boss_card_families_to_disable_group.SetActive(false);
        }
        else if (boss_card.disable_type == Boss_Card_Disable_Type.FAMILY)
        {
            boss_card_disable_price_text.enabled = false;
            boss_card_families_to_disable_group.SetActive(true);

            for (int i = 0; i < boss_card_families_to_disable_image.Length; i++)
            {
                if (i < boss_card.current_family_to_disable || i >= boss_card.families_to_disable_card.Length)
                {
                    boss_card_families_to_disable_image[i].enabled = false;
                }
                else
                {
                    boss_card_families_to_disable_image[i].enabled = true;
                    boss_card_families_to_disable_image[i].sprite = families_sprite[(int)boss_card.families_to_disable_card[i]];
                }
            }
        }
    }

    public void hide_boss_card_preview_hud()
    {
        boss_card_preview_hud.SetActive(false);
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
        pause_button_image.sprite = paused_sprite;
    }

    public void show_tutorial_menu()
    {
        GI.player_card_game.pause_game();

        pause_ui.SetActive(true);
        tutorial_menu.SetActive(true);
    }

    public void hide_tutorial_menu()
    {
        GI.player_card_game.unpause_game();

        pause_ui.SetActive(false);
        tutorial_menu.SetActive(false);
    }

    public void hide_pause()
    {
        pause_ui.SetActive(false);
        pause_menu.SetActive(false);
        tutorial_menu.SetActive(false);
        pause_button_image.sprite = resumed_sprite;
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

    public void show_pause_menu()
    {
        pause_menu.SetActive(true);
        back_to_main_menu_panel.SetActive(false);
    }

    public void show_back_to_main_menu_panel()
    {
        pause_menu.SetActive(false);
        back_to_main_menu_panel.SetActive(true);
    }

    public void hide_back_to_main_menu_panel()
    {
        back_to_main_menu_panel.SetActive(false);
        pause_menu.SetActive(true);
    }

    // @Luana
    // Button on the HUD
    public void toggle_pause()
    {
        if (GI.player_card_game.game_stopped)
        {
            GI.player_card_game.resume_game();
            
        }
        else
        {
            GI.player_card_game.stop_game();
        }
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

    public void back_to_main_menu()
    {
        SceneManager.LoadScene(0);
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
