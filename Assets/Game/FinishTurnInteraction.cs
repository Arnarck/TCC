using UnityEngine;

public class FinishTurnInteraction : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (GI.player.game_stopped) 
        { 
            return; 
        }

        if (GI.card_system.is_player_turn)
        {
            GI.card_system.update_turn();
        }
    }
}
