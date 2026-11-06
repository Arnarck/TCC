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
        else if (boss_card && GI.player_card_game.boss_card_in_preview != boss_card)
        {
            boss_card.show_card_ability_description_canvas();
        }
    }

    private void OnMouseExit()
    {
        if (card && !card.is_in_desk)
        {
            card.disable_card_ability_description_canvas();
        }
        else if (boss_card)
        {
            boss_card.hide_card_ability_description_canvas();
        }
    }
}
