using UnityEngine;


public enum Interaction_Type
{
    START_GAME,
    END_GAME,
    UPDATE_TURN,

    COUNT
}


public class DeskInteraction : MonoBehaviour
{
    public Interaction_Type type;

    private void OnMouseDown()
    {
        if (GI.player.game_stopped) 
        { 
            return; 
        }

        if (type == Interaction_Type.UPDATE_TURN)
        {
            if (GI.card_system.is_player_turn)
            {
                GI.card_system.update_turn();
            }
        }
        else if (type == Interaction_Type.END_GAME)
        {
            GI.card_system.end_game();
        }
        else if (type == Interaction_Type.START_GAME)
        {
            GI.card_system.start_game();
        }
    }
}
