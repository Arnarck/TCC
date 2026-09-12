using UnityEngine;
using System.Collections.Generic;

public class MultiCardSelector : MonoBehaviour
{
    public bool blow_up_if_a_card_is_selected;

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
            if (blow_up_if_a_card_is_selected)
            {
                card_collider.card.blow_up_if_selected = true;
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        CardCollider card_collider;
        if (collider.gameObject.TryGetComponent(out card_collider) && card_collider.card.is_in_desk)
        {
            cards_inside_collider.Remove(card_collider.card);
            if (blow_up_if_a_card_is_selected)
            {
                card_collider.card.blow_up_if_selected = false;
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
