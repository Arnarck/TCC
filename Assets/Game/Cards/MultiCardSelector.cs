using UnityEngine;
using System.Collections.Generic;

public class MultiCardSelector : MonoBehaviour
{
    public Boss_Abilities ability;

    [Header("INTERNAL")]
    public List<Card> cards_inside_collider;

    private void OnEnable()
    {
        cards_inside_collider.Clear();
    }

    private void OnTriggerEnter(Collider collider)
    {
        CardCollider card_collider;
        if (collider.gameObject.TryGetComponent(out card_collider) && card_collider.card.is_in_desk)
        {
            cards_inside_collider.Add(card_collider.card);
            if (ability == Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN)
            {
                card_collider.card.blow_up_if_selected = true;
            }
            else if (ability == Boss_Abilities.REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW)
            {
                card_collider.card.remove_player_points_when_selected = true;
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        CardCollider card_collider;
        if (collider.gameObject.TryGetComponent(out card_collider) && card_collider.card.is_in_desk)
        {
            cards_inside_collider.Remove(card_collider.card);
            if (ability == Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN)
            {
                card_collider.card.blow_up_if_selected = false;
            }
            else if (ability == Boss_Abilities.REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW)
            {
                card_collider.card.remove_player_points_when_selected = false;
            }
        }
    }

    public void demote_cards_inside_collider(int amount)
    {
        for (int i = 0; i < cards_inside_collider.Count; i++)
        {
            cards_inside_collider[i].remove_points(amount);
        }
    }
}
