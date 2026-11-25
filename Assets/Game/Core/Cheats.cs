using UnityEngine;

public class Cheats : MonoBehaviour
{
    public GameObject cheats_panel;

    public void toggle_cheats_panel()
    {
        if (cheats_panel.activeInHierarchy)
        {
            cheats_panel.SetActive(false);
        }
        else
        {
            cheats_panel.SetActive(true);
        }
    }

    public void win()
    {
        GI.player_card_game.win();
    }

    public void lose()
    {
        GI.player_card_game.lose();
    }

    public void kill_boss()
    {
        GI.boss.take_damage(GI.boss.max_health);
        GI.boss.kill_boss();
    }

    // Used in UI/HUD
    public void play_button_click()
    {
        GI.audio_player.play(Audio_Type.BUTTON_CLICK);
    }
}
