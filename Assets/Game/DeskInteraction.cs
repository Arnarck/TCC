using UnityEngine;


public enum Interaction_Type
{
    START_OR_END_GAME,
    UPDATE_TURN,

    COUNT
}


public class DeskInteraction : MonoBehaviour
{
    public Interaction_Type type;

    private void OnMouseEnter()
    {
        if (GI.player.game_stopped)
        {
            return;
        }

        if (type == Interaction_Type.START_OR_END_GAME && !GI.card_system.playing_card_game)
        {
            GI.player_hud.show_interaction_message("Press Mouse 0 to play");
        }
    }

    private void OnMouseExit()
    {
        if (GI.player.game_stopped)
        {
            return;
        }

        if (type == Interaction_Type.START_OR_END_GAME && !GI.card_system.playing_card_game)
        {
            GI.player_hud.hide_interaction_message();
        }
    }

    private void OnMouseDown()
    {
        if (GI.player.game_stopped) 
        { 
            return; 
        }

        if (type == Interaction_Type.UPDATE_TURN && GI.card_system.playing_card_game)
        {
            if (GI.card_system.is_player_turn)
            {
                GI.card_system.update_turn();
            }
        }
        else if (type == Interaction_Type.START_OR_END_GAME)
        {
            if (GI.card_system.playing_card_game)
            {
                GI.card_system.end_game();
            }
            else
            {
                GI.card_system.start_game();
            }
        }
    }
}
