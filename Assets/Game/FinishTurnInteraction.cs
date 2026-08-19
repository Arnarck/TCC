using UnityEngine;

public class FinishTurnInteraction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        // @TODO: return if game is stopped
        if (GI.card_system.is_player_turn)
        {
            GI.card_system.update_turn();
        }
    }
}
