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

    private void OnMouseDown()
    {
        if (GI.player_card_game.game_stopped) 
        { 
            return; 
        }

        if (type == Interaction_Type.UPDATE_TURN && GI.card_system.playing_card_game && GI.card_system.is_player_turn)
        {
            GI.card_system.update_turn();
        }
        else if (type == Interaction_Type.START_OR_END_GAME && GI.card_system.playing_card_game)
        {
            GI.card_system.end_game();
        }
    }
}
