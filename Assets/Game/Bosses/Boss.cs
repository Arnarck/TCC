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
    SPAWN_DWARF_IN_PLAYER_HAND,
    DEMOTE_CARDS_IN_A_COLUMN,
    PROMOTE_CARD_IN_PLAYER_HAND,
    SPAWN_ANNOYING_DWARF_TO_PLAYER_HAND,

    // Kame
    DEMOTE_CARD_BY_X_POINTS,
    BLOW_UP,
    BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN,
    PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES,
    DESTROY_A_CARD_FROM_PLAYER_HAND,

    COUNT
}


public class Boss : MonoBehaviour
{
    public Boss_Type type;
    public Animator animator;
    public int max_health;
    public MultiCardSelector demote_cards_collider;
    public MultiCardSelector blow_up_cards_collider;
    public GameObject bad_apple_card_prefab;
    public GameObject dwarf_card_prefab;
    public GameObject annoying_dwarf_card_prefab;

    [Header("INTERNAL")]
    public int health;
    public int previous_health;
    public int replace_player_cards_t;
    public int demote_cards_in_column_t;
    public int blow_up_t;
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
                // Replace Player Card ability
                if (replace_player_cards_t > 0)
                {
                    replace_player_cards_t -= 1;
                    if (replace_player_cards_t <= 0)
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
                    }
                }

                // Demote Cards In Column ability
                if (demote_cards_in_column_t > 0)
                {
                    demote_cards_in_column_t -= 1;
                    if (demote_cards_in_column_t < 1)
                    {
                        int points_to_remove = 10;
                        demote_cards_collider.demote_cards_inside_collider(points_to_remove);
                        GI.player_hud.show_boss_attack_text("Demoted cards in a columnn by " + points_to_remove + " points");
                    }
                }

                // Blow Up
                if (blow_up_t > 0)
                {
                    blow_up_t -= 1;
                    if (blow_up_t < 1)
                    {
                        GI.player_card_game.lose();
                    }
                }

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
                            if (replace_player_cards_t < 1)
                            {
                                replace_player_cards_t = 2;
                            }
                        } break;
                    case Boss_Abilities.STEAL_PLAYER_POINTS:
                        {
                            int points_to_steal = 10;

                            GI.player_card_game.take_damage(points_to_steal);
                            add_health(points_to_steal);

                            GI.player_hud.show_boss_attack_text("Stolen " + points_to_steal + " points from player");
                        } break;
                    case Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND:
                        {
                            bool success = spawn_card_in_player_hand(dwarf_card_prefab);
                            if (success)
                            {
                                GI.player_hud.show_boss_attack_text("Spawned a Dwarf in player's hand");
                            }

                        } break;
                    case Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN:
                        {
                            if (demote_cards_in_column_t < 1)
                            {
                                demote_cards_in_column_t = 2;
                                demote_cards_collider.gameObject.SetActive(true);
                                demote_cards_collider.cards_inside_collider.Clear();

                                Transform[] columns_spawn_points = GI.card_system.columns_spawn_points;
                                demote_cards_collider.transform.position = 
                                    columns_spawn_points[Random.Range(0, columns_spawn_points.Length)].position;
                            }

                        } break;
                    case Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND:
                        {
                            bool points_added = false;
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                Card card = GI.player_card_game.cards_in_hand[i];
                                if (card)
                                {
                                    card.add_points(1);
                                    points_added = true;
                                    break;
                                }
                            }

                            if (points_added)
                            {
                                GI.player_hud.show_boss_attack_text("Added 1 point to a player's card");
                            }
                        } break;
                    case Boss_Abilities.SPAWN_ANNOYING_DWARF_TO_PLAYER_HAND:
                        {
                            bool success = spawn_card_in_player_hand(annoying_dwarf_card_prefab);
                            if (success)
                            {
                                GI.player_hud.show_boss_attack_text("Spawned an Annoying Dwarf in player's hand");
                            }

                        } break;
                    case Boss_Abilities.DEMOTE_CARD_BY_X_POINTS:
                        {
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                Card card = GI.player_card_game.cards_in_hand[i];
                                if (card)
                                {
                                    card.remove_points(2);
                                    GI.player_hud.show_boss_attack_text("Removed 2 points from a player's card");

                                    break;
                                }
                            }
                        } break;
                    case Boss_Abilities.BLOW_UP:
                        {
                            if (blow_up_t < 1)
                            {
                                blow_up_t = 4;
                            }
                            break;
                        }
                    case Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN:
                        {
                            if (!blow_up_cards_collider.gameObject.activeInHierarchy)
                            {
                                blow_up_cards_collider.gameObject.SetActive(true);
                                blow_up_cards_collider.cards_inside_collider.Clear();

                                Transform[] columns_spawn_points = GI.card_system.columns_spawn_points;
                                blow_up_cards_collider.transform.position =
                                    columns_spawn_points[Random.Range(0, columns_spawn_points.Length)].position;
                            }
                        } break;
                    case Boss_Abilities.PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES:
                        {
                            List<Card> available_cards = new List<Card>();
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                Card card = GI.player_card_game.cards_in_hand[i];
                                if (card)
                                {
                                    available_cards.Add(card);
                                }
                            }

                            Family_Type family_to_increase_points = available_cards[Random.Range(0, available_cards.Count)].family_type;
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                Card card = GI.player_card_game.cards_in_hand[i];
                                if (card)
                                {
                                    if (card.family_type == family_to_increase_points)
                                    {
                                        card.add_points(5);
                                    }
                                    else
                                    {
                                        card.remove_points(10);
                                    }
                                }
                            }

                            GI.player_hud.show_boss_attack_text("Added 5 points to " + family_to_increase_points.ToString() + ". Removed" +
                                "10 points for the other families");
                        } break;
                    case Boss_Abilities.DESTROY_A_CARD_FROM_PLAYER_HAND:
                        {
                            List<Card> available_cards = new List<Card>();
                            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                            {
                                available_cards.Add(GI.player_card_game.cards_in_hand[i]);
                            }

                            Card card_to_remove = available_cards[Random.Range(0, available_cards.Count)];
                            GI.player_card_game.remove_card_from_hand(card_to_remove);
                            card_to_remove.destroy();
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

    public bool spawn_card_in_player_hand(GameObject card_prefab)
    {
        for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
        {
            if (!GI.player_card_game.cards_in_hand[i])
            {
                Card card = Instantiate(card_prefab).GetComponent<Card>();

                GI.player_card_game.add_card_to_hand(card, i);
                return true;
            }
        }

        return false;
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
