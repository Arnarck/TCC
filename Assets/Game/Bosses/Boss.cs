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
    ADD_CHIPS,                                      // Deep Pockets
    DEMOTE_CHARACTERS_FROM_FAMILY_X,                // Destroy Reputation
    REPLACE_PLAYER_CARD,                            // Bad Company
    STEAL_PLAYER_POINTS,                            // Light Paws

    // Witch
    SPAWN_DWARF_IN_PLAYER_HAND,                     // Little Infestation
    DEMOTE_CARDS_IN_A_COLUMN,                       // Poison Pie
    PROMOTE_CARD_IN_PLAYER_HAND,                    // Irresistible Cake
    SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND,            // Growing Infestation

    // Kame
    DEMOTE_CARD_BY_X_POINTS,
    BLOW_UP,
    BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN,
    PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES, //
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
    public BossCard[] boss_card_list;
    public Transform[] cards_spawn_points;

    [Header("INTERNAL")]
    public int health;
    public int previous_health;
    public int replace_player_cards_t;
    public int demote_cards_in_column_t;
    public int blow_up_t;
    public float finish_turn_t;
    public int current_turn;
    public Boss_Abilities cheat_ability_to_use;
    public BossCard[] cards_in_desk;

    void Awake()
    {
        GI.boss = this;
    }

    private void Start()
    {
        cards_in_desk = new BossCard[cards_spawn_points.Length];
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

                // Create list of possible abilities to choose
                List<BossCard> available_abilities = new List<BossCard>(); // @TODO: Cache this if we have performance problems
                for (int i = 0; i < cards_in_desk.Length; i++)
                {
                    if (cards_in_desk[i])
                    {
                        available_abilities.Add(cards_in_desk[i]);
                    }
                }

                BossCard card_to_use = available_abilities[Random.Range(0, available_abilities.Count)];
                switch (card_to_use.ability_type)
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
                    case Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND:
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

                remove_card_from_desk(card_to_use);
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

        current_turn = 0;
    }

    public void start_turn()
    {
        current_turn++;
        finish_turn_t = 2f;

        int half_health = max_health / 2;
        if (health <= half_health && previous_health > half_health)
        {
            animator.SetTrigger("EnterPhase2");
            finish_turn_t += 3.5f;
        }

        // Add cards to desk
        switch (type)
        {
            case Boss_Type.CAT:
                {
                    if (card_count_in_desk() < 3)
                    {
                        if (current_turn % 2 == 0)
                        {
                            // Place two cards
                            spawn_random_between_two_cards(Boss_Abilities.ADD_CHIPS, Boss_Abilities.STEAL_PLAYER_POINTS);

                            if (card_count_in_desk() < 3)
                            {
                                if (is_card_in_desk(Boss_Abilities.DEMOTE_CHARACTERS_FROM_FAMILY_X))
                                    spawn_card_in_desk(Boss_Abilities.REPLACE_PLAYER_CARD);
                                else if (is_card_in_desk(Boss_Abilities.REPLACE_PLAYER_CARD))
                                    spawn_card_in_desk(Boss_Abilities.DEMOTE_CHARACTERS_FROM_FAMILY_X);
                                else
                                {
                                    if (Random.Range(0, 2) == 0)
                                        spawn_card_in_desk(Boss_Abilities.DEMOTE_CHARACTERS_FROM_FAMILY_X); // Destroy Reputation
                                    else
                                        spawn_card_in_desk(Boss_Abilities.REPLACE_PLAYER_CARD); // Bad Company
                                }
                            }
                        }
                        else
                        {
                            // Place one card
                            spawn_random_between_two_cards(Boss_Abilities.ADD_CHIPS, Boss_Abilities.STEAL_PLAYER_POINTS);
                        }
                    }
                } break;
            case Boss_Type.WITCH:
                {
                    if (card_count_in_desk() < 5)
                    {
                        if (current_turn % 2 == 0)
                        {
                            // Place two cards
                            bool spawned_first_card = false;
                            if (!is_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND))
                            {
                                spawn_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND);
                                spawned_first_card = true;
                            }

                            int card_count_to_spawn = 2;
                            if (spawned_first_card)
                                card_count_to_spawn = 1;
                            for (int i = 0; i < card_count_to_spawn; i++)
                            {
                                if (card_count_in_desk() < 5)
                                    spawn_witch_cards();
                            }
                        }
                        else
                        {
                            // Place one card
                            if (!is_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND))
                                spawn_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND);
                            else
                            {
                                spawn_witch_cards();
                            }
                        }
                    }
                } break;
            default: break;
        }
    }

    public void spawn_witch_cards()
    {
        if (is_only_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND))
        {
            int random_value = Random.Range(0, 3);
            if (random_value == 0)
                spawn_card_in_desk(Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND);
            else if (random_value == 1)
                spawn_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);
            else
                spawn_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN);
        }
        else if (is_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND) && (
                 is_card_in_desk(Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND) ||
                 is_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND) ||
                 is_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN)))
        {
            if (!is_card_in_desk(Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND))
                spawn_card_in_desk(Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND);
            else if (!is_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND))
                spawn_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);
            else if (!is_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN))
                spawn_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN);
            else
                spawn_random_between_two_cards(Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND,
                                               Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);

        }
        else
            spawn_random_between_two_cards(Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND,
                                           Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);
    }

    public bool is_only_card_in_desk(Boss_Abilities ability)
    {
        bool only_card_in_desk = false;
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i])
            {
                if (cards_in_desk[i].ability_type == ability)
                    only_card_in_desk = true;
                else
                    return false;
            }
        }

        return only_card_in_desk;
    }
    
    public bool is_card_in_desk(Boss_Abilities ability)
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i] && cards_in_desk[i].ability_type == ability)
            {
                return true;
            }
        }

        return false;
    }

    public void spawn_card_in_desk(Boss_Abilities ability)
    {
        // Find the prefab
        GameObject card_to_spawn = null;
        for (int i = 0; i < boss_card_list.Length; i++)
        {
            if (boss_card_list[i].ability_type == ability)
            {
                card_to_spawn = boss_card_list[i].gameObject;
                break;
            }
        }

        // Spawn in first available position
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (!cards_in_desk[i])
            {
                BossCard card = Instantiate(card_to_spawn).GetComponent<BossCard>();
                card.transform.position = cards_spawn_points[i].position;
                card.transform.rotation = cards_spawn_points[i].rotation;

                cards_in_desk[i] = card;
                break;
            }
        }
    }

    public void remove_card_from_desk(BossCard card)
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i] == card)
            {
                cards_in_desk[i] = null;
                card.destroy();

                break;
            }
        }
    }

    public int card_count_in_desk()
    {
        int count = 0;
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i])
            {
                count++;
            }
        }

        return count;
    }

    public void spawn_random_between_two_cards(Boss_Abilities ability_1, Boss_Abilities ability_2)
    {
        if (Random.Range(0, 2) == 0)
        {
            spawn_card_in_desk(ability_1);
        }
        else
        {
            spawn_card_in_desk(ability_2);
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
