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
}
