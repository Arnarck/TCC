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
    DEMOTE_CARD_BY_X_POINTS,                        // Drinks
    BLOW_UP,                                        // T4
    BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN,    // Turtle Mine
    PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES, // Favorites
    DESTROY_A_CARD_FROM_PLAYER_HAND,                // Good Stuff

    // Phase 2
    REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW,   // Wonderland
    DESTROY_CARD_FROM_TABLE_AND_REPLACE_WITH_ANNOYING_DWARF, // Secret Ingredient
    DESTROY_TRIO_CARD_ON_THE_LEFT,                           // Tough Shell

    COUNT
}


public class Boss : MonoBehaviour
{
    public Boss_Type type;
    public Animator animator;
    public int max_health;
    public MultiCardSelector demote_cards_collider;
    public MultiCardSelector blow_up_cards_collider;
    public MultiCardSelector remove_points_cards_collider;
    public GameObject bad_apple_card_prefab;
    public GameObject dwarf_card_prefab;
    public GameObject annoying_dwarf_card_prefab;
    public GameObject dizzinnes_card_prefab;
    public BossCard[] boss_card_list;
    public Transform[] cards_spawn_points;

    [Header("INTERNAL")]
    public int health;
    public int previous_health;
    public int replace_player_cards_t;
    public int demote_cards_in_column_t;
    public int blow_up_t;
    public int current_turn;
    public bool is_phase_2;
    public bool is_alive;
    public float finish_turn_t;
    public float play_start_turn_animation_t;
    public float kill_boss_t;
    public float update_turn_t;
    public BossCard[] cards_in_desk;

    private void Start()
    {
        cards_in_desk = new BossCard[cards_spawn_points.Length];
        is_alive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (GI.player_card_game.game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (update_turn_t > 0f)
        {
            update_turn_t -= dt;
            if (update_turn_t <= 0f)
            {
                GI.card_system.update_turn();
            }
        }

        if (kill_boss_t > 0f)
        {
            kill_boss_t -= dt;
            if (kill_boss_t <= 0f)
            {
                gameObject.SetActive(false);
                GI.card_system.start_game();
            }
        }

        if (play_start_turn_animation_t > 0f)
        {
            play_start_turn_animation_t -= dt;
            if (play_start_turn_animation_t <= 0f)
            {
                animator.SetTrigger("StartTurn");
            }
        }


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

                        GI.player_hud.add_message_to_history_panel("<b>Bad Company: </b>A player card was replaced by a <b>Bad Apple</b>");
                    }
                    else
                    {
                        if (replace_player_cards_t == 1)
                        {
                            GI.player_hud.add_message_to_history_panel("<b>Bad Company: </b>A player card will be replaced by a " +
                                                                                   "<b>Bad Apple</b> in 1 turn");
                        }
                        else
                        {
                            GI.player_hud.add_message_to_history_panel("<b>Bad Company: </b>A player card will be replaced by a " +
                                                                       "<b>Bad Apple</b> in " + replace_player_cards_t + " turns");
                        }
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
                        GI.player_hud.add_message_to_history_panel("<b>Poison Pie: </b>All cards from a column in desk are " +
                                                                   "<b>DEMOTED</b> by " + points_to_remove + " points");

                        demote_cards_collider.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (demote_cards_in_column_t == 1)
                        {
                            GI.player_hud.add_message_to_history_panel("<b>Poison Pie: </b>A card column from desk desk will be " +
                                                                       "<b>DEMOTED</b> in 1 turn");
                        }
                        else
                        {
                            GI.player_hud.add_message_to_history_panel("<b>Poison Pie: </b>A card column from desk desk will be " +
                                                                       "<b>DEMOTED</b> in " + demote_cards_in_column_t + " turns");
                        }
                    }
                }

                // Blow Up
                if (blow_up_t > 0)
                {
                    blow_up_t -= 1;
                    if (blow_up_t < 1)
                    {
                        GI.player_card_game.lose();
                        GI.player_hud.add_message_to_history_panel("<b>T4: </b>Game Over");
                    }
                    else
                    {
                        if (blow_up_t == 1)
                        {
                            GI.player_hud.add_message_to_history_panel("<b>T4: </b>Game Over in 1 turn");
                        }
                        else
                        {
                            GI.player_hud.add_message_to_history_panel("<b>T4: </b>Game Over in " + blow_up_t + " turns");
                        }
                    }
                }

                // Create list of possible abilities to choose
                List<BossCard> available_abilities = new List<BossCard>(); // @TODO: Cache this if we have performance problems
                for (int i = 0; i < cards_in_desk.Length; i++)
                {
                    if (cards_in_desk[i] && 
                        cards_in_desk[i].turn_off_t <= 0 && 
                        cards_in_desk[i].ability_type != Boss_Abilities.DESTROY_TRIO_CARD_ON_THE_LEFT)
                    {
                        available_abilities.Add(cards_in_desk[i]);
                    }
                }

                // Use ability
                BossCard card_to_use = null;
                if (available_abilities.Count > 1)
                {
                    card_to_use = available_abilities[Random.Range(0, available_abilities.Count)];
                    card_to_use.do_attack();

                    // Play ability animation
                    if (card_to_use.ability_type == Boss_Abilities.ADD_CHIPS || 
                        card_to_use.ability_type == Boss_Abilities.STEAL_PLAYER_POINTS ||
                        card_to_use.ability_type == Boss_Abilities.REPLACE_PLAYER_CARD ||
                        card_to_use.ability_type == Boss_Abilities.BLOW_UP ||
                        card_to_use.ability_type == Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN)
                    {
                        animator.SetTrigger("SpecialAbility");
                    }
                    else
                    {
                        animator.SetTrigger("NormalAbility");
                    }

                    // Apply ability
                    switch (card_to_use.ability_type)
                    {
                        case Boss_Abilities.ADD_CHIPS:
                            {
                                add_health(5);
                                GI.player_hud.add_message_to_history_panel("<b>Deep Pockets: </b>Added 10 health");
                            }
                            break;
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
                                    GI.player_hud.add_message_to_history_panel("<b>Destroy Reputation: </b>Removed 2 points from cards " +
                                                                               "of Family " + 
                                                                               family_to_reduce_points.ToString().Replace('_', ' '));
                                }
                            }
                            break;
                        case Boss_Abilities.REPLACE_PLAYER_CARD:
                            {
                                if (replace_player_cards_t < 1)
                                {
                                    replace_player_cards_t = 2;
                                }

                                GI.player_hud.add_message_to_history_panel("<b>Bad Company: </b>A player card will be replaced by a " +
                                                                           "<b>Bad Apple</b> in " + replace_player_cards_t + " turns");
                            }
                            break;
                        case Boss_Abilities.STEAL_PLAYER_POINTS:
                            {
                                int points_to_steal = 5;

                                GI.player_card_game.take_damage(points_to_steal);
                                add_health(points_to_steal);

                                GI.player_hud.add_message_to_history_panel("<b>Light Paws: </b>Stolen " + points_to_steal + " points " +
                                                                           "from player");
                            }
                            break;
                        case Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND:
                            {
                                bool success = spawn_card_in_player_hand(dwarf_card_prefab);
                                if (success)
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Little Infestation: </b>Spawned a Dwarf in player's " +
                                                                               "hand");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Little Infestation: </b>No available cards in " +
                                                                               "player's hand. <b>Dwarf</b> will not spawn");
                                }

                            }
                            break;
                        case Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN:
                            {
                                if (demote_cards_in_column_t < 1)
                                {
                                    demote_cards_in_column_t = 2;
                                    demote_cards_collider.gameObject.SetActive(true);
                                    spawn_multicard_selector_in_desk(demote_cards_collider, GI.card_system.columns_spawn_points);
                                    GI.player_hud.add_message_to_history_panel("<b>Poison Pie: </b>A card column from desk desk will be " +
                                                                               "<b>DEMOTED</b> in " + demote_cards_in_column_t + " turns");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Poison Pie: </b>A card column from desk already is " +
                                                                               "<b>DEMOTED</b>. Ability will be ignored");
                                }
                            }
                            break;
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
                                    GI.player_hud.add_message_to_history_panel("<b>Irresistible Cake: </b>Added 1 point to a player's " +
                                                                               "card");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Irresistible Cake: </b>Player has no cards in hand." +
                                                                               " Ability will be ignored");
                                }
                            }
                            break;
                        case Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND:
                            {
                                bool success = spawn_card_in_player_hand(annoying_dwarf_card_prefab);
                                if (success)
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Growing Infestation: </b>Spawned an " +
                                                                               "<b>Annoying Dwarf</b> in player's hand");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Growing Infestation: </b>Player has no cards i hand." +
                                                                               " Ability will be ignored");
                                }

                            }
                            break;
                        case Boss_Abilities.DEMOTE_CARD_BY_X_POINTS:
                            {
                                bool success = false;
                                for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
                                {
                                    Card card = GI.player_card_game.cards_in_hand[i];
                                    if (card)
                                    {
                                        card.remove_points(3);
                                        GI.player_hud.add_message_to_history_panel("<b>Drinks: </b>Removed 3 points from a player's card");
                                        success = true;

                                        break;
                                    }
                                }

                                if (!success)
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Drinks: </b>Player has no cards in hand. Ability " +
                                                                               "will be ignored");
                                }
                            }
                            break;
                        case Boss_Abilities.BLOW_UP:
                            {
                                if (blow_up_t < 1)
                                {
                                    blow_up_t = 4;
                                    GI.player_hud.add_message_to_history_panel("<b>T4: </b>Game Over in " + blow_up_t + " turns");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>T4: </b>Another <b>T4</b> is in progress. Ability " +
                                                                               "will be ignored");
                                }
                            }
                            break;
                        case Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN:
                            {
                                if (!blow_up_cards_collider.gameObject.activeInHierarchy)
                                {
                                    blow_up_cards_collider.gameObject.SetActive(true);
                                    spawn_multicard_selector_in_desk(blow_up_cards_collider, GI.card_system.columns_spawn_points);

                                    GI.player_hud.add_message_to_history_panel("<b>Turtle Mine: A card column from desk is armed. " +
                                                                               "Selecting a card from this column will result in " +
                                                                               "Game Over");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Turtle Mine: Another card column from desk is already" +
                                                                               " armed. Ability will be ignored");
                                }
                            }
                            break;
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

                                GI.player_hud.add_message_to_history_panel("<b>Favorites: </b>Added 5 points to all cards from family " +
                                                                           "<b>" + family_to_increase_points.ToString().Replace('_', ' ') + 
                                                                           "</b> in player's hand. Removed 10 points from cards of " +
                                                                           "the other families");
                            } break;
                        case Boss_Abilities.DESTROY_A_CARD_FROM_PLAYER_HAND:
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

                                if (available_cards.Count > 0)
                                {
                                    Card card_to_remove = available_cards[Random.Range(0, available_cards.Count)];
                                    GI.player_card_game.remove_card_from_hand(card_to_remove);
                                    GI.player_card_game.reorder_cards_in_hand();
                                    card_to_remove.destroy();

                                    GI.player_hud.add_message_to_history_panel("<b>Good Stuff: </b>Destroyed <b>" + 
                                                                               card_to_remove.type.ToString().Replace('_', ' ') + 
                                                                               "<b> card from player's hand");
                                }
                                else
                                {
                                    GI.player_hud.add_message_to_history_panel("<b>Good Stuff: </b>Player has no cards in hand. Ability will " +
                                                                        "be ignored");
                                }


                            } break;
                        case Boss_Abilities.REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW:
                            {
                                remove_points_cards_collider.gameObject.SetActive(true);
                                spawn_multicard_selector_in_desk(remove_points_cards_collider, GI.card_system.rows_spawn_points);

                                GI.player_hud.add_message_to_history_panel("<b>Wonderland: </b>A card row from desk was cursed. " +
                                                                           "Selecting a card from this row will consume 5 Chips");
                            } break;
                        case Boss_Abilities.DESTROY_CARD_FROM_TABLE_AND_REPLACE_WITH_ANNOYING_DWARF:
                            {
                                List<Card> available_cards = new List<Card>();
                                for (int i = 0; i < GI.card_system.cards_in_desk.Length; i++)
                                {
                                    Card card_in_desk = GI.card_system.cards_in_desk[i];
                                    if (card_in_desk)
                                    {
                                        available_cards.Add(card_in_desk);
                                    }
                                }

                                // Remove card from desk
                                int index_to_remove = Random.Range(0, available_cards.Count);
                                Card card_to_remove_from_desk = available_cards[index_to_remove];
                                GI.card_system.remove_card_from_desk(card_to_remove_from_desk);
                                card_to_remove_from_desk.destroy();

                                // Spawn card
                                Card card_to_spawn = Instantiate(annoying_dwarf_card_prefab).GetComponent<Card>();
                                GI.card_system.add_card_to_desk(card_to_spawn, index_to_remove);

                                GI.player_hud.add_message_to_history_panel("<b>Secret Ingredient: </b>A card in desk was overwritten " +
                                                                           "by an Annoying Dwarf");
                            } break;
                        case Boss_Abilities.DESTROY_TRIO_CARD_ON_THE_LEFT:
                            {
                                // Implemented in PlayerController.cs
                            } break;
                        default: break;
                    }

                    remove_card_from_desk(card_to_use);
                }

                previous_health = health;
                update_turn_t = 2f;
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

    public void spawn_multicard_selector_in_desk(MultiCardSelector card_selector, Transform[] spawn_points)
    {
        card_selector.cards_inside_collider.Clear();
        remove_points_cards_collider.transform.position = spawn_points[Random.Range(0, spawn_points.Length)].position;
    }

    public List<BossCard> get_available_cards()
    {
        List<BossCard> available_cards = new List<BossCard>();
        for (int i = 0; i < GI.boss.cards_in_desk.Length; i++)
        {
            if (GI.boss.cards_in_desk[i])
            {
                available_cards.Add(GI.boss.cards_in_desk[i]);
            }
        }

        return available_cards;
    }

    public BossCard get_a_random_card_from_desk()
    {
        // Get available cards
        List<BossCard> available_cards = new List<BossCard>();
        for (int i = 0; i < GI.boss.cards_in_desk.Length; i++)
        {
            if (GI.boss.cards_in_desk[i])
            {
                available_cards.Add(GI.boss.cards_in_desk[i]);
            }
        }

        // Choose a random card
        if (available_cards.Count < 1)
        {
            return null;
        }
        else
        {
            int random_index = Random.Range(0, available_cards.Count);
            return available_cards[random_index];
        }
    }

    public void start_turn()
    {
        current_turn++;
        finish_turn_t = 3f;

        bool entered_phase_2 = false;
        int half_health = max_health / 2;
        if (health <= half_health && previous_health > half_health)
        {
            animator.SetTrigger("EnterPhase2");
            is_phase_2 = true;
            entered_phase_2 = true;
        }
        else
        {
            play_start_turn_animation_t = 0.5f;
        }

        // Add cards to desk
        if (entered_phase_2)
        {
            if (type == Boss_Type.CAT)
            {
                spawn_card_in_desk(Boss_Abilities.REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW);
            }
            else if (type == Boss_Type.WITCH)
            {
                spawn_card_in_desk(Boss_Abilities.DESTROY_CARD_FROM_TABLE_AND_REPLACE_WITH_ANNOYING_DWARF);
            }
            else
            {
                spawn_card_in_desk(Boss_Abilities.DESTROY_TRIO_CARD_ON_THE_LEFT);
            }
        }

        switch (type)
        {
            case Boss_Type.CAT:
                {
                    if (card_count_in_desk() < 3)
                    {
                        if (current_turn % 2 == 0)
                        {
                            // Place two cards
                            spawn_cat_first_card();

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
                            spawn_cat_first_card();
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
            case Boss_Type.KAME:
                {
                    if (card_count_in_desk() < 7)
                    {
                        if (current_turn == 1)
                        {
                            spawn_card_in_desk(Boss_Abilities.BLOW_UP);
                            spawn_card_in_desk(Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN);
                        }
                        else
                        {
                            // Spawn first card
                            spawn_first_kame_card();

                            // Spawn second card
                            bool spawned_second_card = false;
                            if (!is_card_in_desk(Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN))
                            {
                                int value2 = Random.Range(0, 2);
                                if (value2 == 0)
                                    spawned_second_card = spawn_card_in_desk(Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN);
                                else
                                    spawned_second_card = spawn_card_in_desk(Boss_Abilities.PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES);
                            }
                            else if (is_card_in_desk(Boss_Abilities.PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES))
                            {
                                spawned_second_card = spawn_card_in_desk(Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN);
                            }

                            if (!spawned_second_card && !is_card_in_desk(Boss_Abilities.BLOW_UP) && 
                                                         is_card_in_desk(Boss_Abilities.BLOW_UP_WHEN_SELECTING_A_CARD_FROM_A_COLUMN) &&
                                                         is_card_in_desk(Boss_Abilities.PROMOTE_A_FAMILY_AND_DEMOTE_ALL_OTHER_FAMILIES))
                            {
                                spawned_second_card = spawn_card_in_desk(Boss_Abilities.BLOW_UP);
                            }

                            if (!spawned_second_card)
                            {
                                spawn_first_kame_card();
                            }
                        }
                    }
                } break;
            default: break;
        }
    }

    public void spawn_cat_first_card()
    {
        if (!is_phase_2)
            spawn_random_between_two_cards(Boss_Abilities.ADD_CHIPS, Boss_Abilities.STEAL_PLAYER_POINTS);
        else
            spawn_random_between_two_cards(Boss_Abilities.STEAL_PLAYER_POINTS, Boss_Abilities.STEAL_PLAYER_POINTS);
    }

    public void spawn_first_kame_card()
    {
        int value = Random.Range(0, 2);
        if (value == 0)
        {
            if (!is_phase_2)
                spawn_card_in_desk(Boss_Abilities.DEMOTE_CARD_BY_X_POINTS);
            else
                spawn_card_in_desk(Boss_Abilities.DESTROY_A_CARD_FROM_PLAYER_HAND);
        }
        else
            spawn_card_in_desk(Boss_Abilities.DESTROY_A_CARD_FROM_PLAYER_HAND);
    }

    public void spawn_witch_cards()
    {
        Boss_Abilities dwarf_or_annoying_dwarf = Boss_Abilities.SPAWN_DWARF_IN_PLAYER_HAND;
        if (is_phase_2) { dwarf_or_annoying_dwarf = Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND; }

        if (is_only_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND))
        {
            int random_value = Random.Range(0, 3);
            if (random_value == 0)
                spawn_card_in_desk(dwarf_or_annoying_dwarf);
            else if (random_value == 1)
                spawn_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);
            else
                spawn_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN);
        }
        else if (is_card_in_desk(Boss_Abilities.PROMOTE_CARD_IN_PLAYER_HAND) && (
                 is_card_in_desk(dwarf_or_annoying_dwarf) ||
                 is_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND) ||
                 is_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN)))
        {
            if (!is_card_in_desk(dwarf_or_annoying_dwarf))
                spawn_card_in_desk(dwarf_or_annoying_dwarf);
            else if (!is_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND))
                spawn_card_in_desk(Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);
            else if (!is_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN))
                spawn_card_in_desk(Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN);
            else
                spawn_random_between_two_cards(dwarf_or_annoying_dwarf,
                                               Boss_Abilities.SPAWN_ANNOYING_DWARF_IN_PLAYER_HAND);

        }
        else
            spawn_random_between_two_cards(dwarf_or_annoying_dwarf,
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

    public bool spawn_card_in_desk(Boss_Abilities ability)
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
                return true;
            }
        }

        return false;
    }

    public int get_card_in_desk_index(BossCard card)
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i] && cards_in_desk[i] == card)
            {
                return i;
            }
        }

        return -1;
    }

    public void remove_card_from_desk(BossCard card, bool remove_ongoing_ability = false)
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i] == card)
            {
                cards_in_desk[i] = null;

                if (remove_ongoing_ability)
                {
                    switch (card.ability_type)
                    {
                        case Boss_Abilities.REPLACE_PLAYER_CARD:
                            {
                                replace_player_cards_t = 0;
                            } break;
                        case Boss_Abilities.DEMOTE_CARDS_IN_A_COLUMN:
                            {
                                demote_cards_in_column_t = 0;
                                demote_cards_collider.gameObject.SetActive(false);
                            } break;
                        case Boss_Abilities.BLOW_UP:
                            {
                                blow_up_t = 0;
                            } break;
                        case Boss_Abilities.REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW:
                            {
                                remove_points_cards_collider.gameObject.SetActive(false);
                            } break;
                        default: break;
                    }
                }

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
        if (!is_alive) { return; }

        health += amount;
        if (health > max_health)
        {
            health = max_health;
        }

        GI.player_hud.update_boss_health_text();
    }

    public void take_damage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            is_alive = false;
        }

        if (amount >= 5)
        {
            animator.SetTrigger("TakeMoreDamage");
        }
        else if (amount > 0) 
        { 
            animator.SetTrigger("TakeDamage");
        }

        GI.player_hud.update_boss_health_text();
    }

    public void kill_boss()
    {
        if (GI.card_system.current_boss_index < GI.card_system.bosses_in_game.Length - 1)
        {
            animator.SetTrigger("Lose");
            kill_boss_t = 8.2f;
        }
        else
        {
            GI.player_card_game.win();
        }
    }
}
