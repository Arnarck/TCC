using UnityEngine;

public class CardCollider : MonoBehaviour
{
    public Card card;
    public BossCard boss_card;

    private void OnMouseEnter()
    {
        if (card && !card.is_in_desk && !GI.player_card_game.cards_in_trio.Contains(card))
        {
            card.ability_description_canvas.SetActive(true);
            card.ability_description_text.text = card.ability_description;

        }
    }

    private void OnMouseExit()
    {
        if (card && !card.is_in_desk)
        {
            card.disable_card_ability_description_canvas();
        }
    }
}
