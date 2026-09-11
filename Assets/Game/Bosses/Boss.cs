using UnityEngine;
using System.Collections.Generic;


public enum Boss_Type
{
    CAT,
    WITCH,
    KAME,

    COUNT
}

public enum Boss_Abilities
{
    // Cat
    ADD_CHIPS,
    DEMOTE_CHARACTERS_FROM_FAMILY_X,
    REPLACE_PLAYER_CARD,
    STEAL_PLAYER_POINTS,

    // Witch

    // Kame

    COUNT
}


public class Boss : MonoBehaviour
{
    public Boss_Type type;
    public Animator animator;
    public int max_health;
    public GameObject bad_apple_card_prefab;

    [Header("INTERNAL")]
    public int health;
    public int previous_health;
    public float finish_turn_t;
    public Boss_Abilities cheat_ability_to_use;

    void Awake()
    {
        GI.boss = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (GI.player_card_game.game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (finish_turn_t > 0f)
        {
            finish_turn_t -= dt;
            if (finish_turn_t <= 0f)
            {
                switch (cheat_ability_to_use)
                {
                    case Boss_Abilities.ADD_CHIPS:
                        {
                            add_health(10);
                            GI.player_hud.show_boss_attack_text("Added 10 health");
                        } break;
                    case Boss_Abilities.DEMOTE_CHARACTERS_FROM_FAMILY_X:
                        {
                            Family_Type[] families_in_trio = GI.player_card_game.families_in_trio;
                            Family_Type family_to_reduce_points = families_in_trio[Random.Range(0, GI.player_card_game.families_in_trio.Length)];

                            bool points_removed = false;
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                Card card = GI.player_card_game.cards_in_hand[i];
                                if (card && card.family_type == family_to_reduce_points)
                                {
                                    card.remove_points(2);
                                    points_removed = true;
                                }
                            }

                            if (points_removed)
                            {
                                GI.player_hud.show_boss_attack_text("Removed 2 points from family " + family_to_reduce_points.ToString());
                            }
                        } break;
                    case Boss_Abilities.REPLACE_PLAYER_CARD:
                        {
                            // Fill cards
                            List<Card> available_cards = new List<Card>();
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                Card card = GI.player_card_game.cards_in_hand[i];
                                if (card)
                                {
                                    available_cards.Add(card);
                                }
                            }

                            // Remove from hand
                            Card card_to_remove = available_cards[Random.Range(0, available_cards.Count)];
                            int index = GI.player_card_game.remove_card_from_hand(card_to_remove);
                            card_to_remove.destroy();

                            // Add Bad Apple to hand
                            Card bad_apple_card = Instantiate(bad_apple_card_prefab).GetComponent<Card>();
                            GI.player_card_game.add_card_to_hand(bad_apple_card, index);

                            GI.player_hud.show_boss_attack_text("Added Bad Apple to player's hand");
                        } break;
                    case Boss_Abilities.STEAL_PLAYER_POINTS:
                        {
                            int points_to_steal = 10;

                            GI.player_card_game.take_damage(points_to_steal);
                            add_health(points_to_steal);

                            GI.player_hud.show_boss_attack_text("Stolen " + points_to_steal + " points from player");
                        } break;
                    default: break;
                }

                previous_health = health;
                GI.card_system.update_turn();
            }
        }
    }

    public void init()
    {
        gameObject.SetActive(true);
    }

    public void start_game()
    {
        health = max_health;
        GI.player_hud.update_boss_health_text();
    }

    public void start_turn()
    {
        finish_turn_t = 2f;

        int half_health = max_health / 2;
        if (health <= half_health && previous_health > half_health)
        {
            animator.SetTrigger("EnterPhase2");
            finish_turn_t += 3.5f;
        }
    }

    public void add_health(int amount)
    {
        health += amount;
        if (health > max_health)
        {
            health = max_health;
        }

        GI.player_hud.update_boss_health_text();
    }

    public void take_damage(int amount)
    {
        health -= amount*3; // @TODO: Remove the '*3'
        if (health <= 0)
        {
            health = 0;
            GI.player_card_game.win();
        }

        GI.player_hud.update_boss_health_text();
    }
}
