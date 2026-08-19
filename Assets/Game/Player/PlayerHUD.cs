using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI points_text;
    public TextMeshProUGUI actions_remaining_text;
    public GameObject player_turn_message;
    public GameObject boss_turn_message;

    private void Awake()
    {
        GI.player_hud = this;
    }

    public void update_points_text()
    {
        points_text.text = "Chips: " + GI.player.points;
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
}
