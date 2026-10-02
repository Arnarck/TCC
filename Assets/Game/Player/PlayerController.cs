using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public Camera player_camera;
    public Transform[] trio_spawn_points;
    public Transform[] cards_spawn_points;
    public List<Card> selected_cards;
    public Card[] cards_in_hand;

    [Header("CAMERA ANIMATION")]
    public Transform camera_points_parent;
    public float update_camera_point_animation_speed;

    [Header("INTERNAL")]
    public bool game_started;
    public int health;
    public int current_trio_card_to_disable;
    public int actions_remaining;
    public bool game_stopped;
    public int current_camera_point;
    public bool game_over;
    public bool applying_trio_card_abilities;
    public bool do_update_camera_point_animation;
    public bool improve_upcoming_pinocchios;
    public float disable_trio_cards_t;
    public int previous_camera_point;
    public float update_camera_point_t;
    public Vector3 camera_start_position;
    public Quaternion camera_start_rotation;
    public List<Card> cards_in_trio;
    public List<Card_Type> card_types_in_trio;
    public Transform[] camera_points;
    public Family_Type[] families_in_trio;

    void Awake()
    {
        GI.player_card_game = this;
    }

    private void Start()
    {
        resume_game();

        families_in_trio = new Family_Type[3];

        camera_points = new Transform[camera_points_parent.childCount];
        for (int i = 0; i < camera_points.Length; i++)
        {
            camera_points[i] = camera_points_parent.GetChild(i);
        }
    }

    private void Update()
    {
        if (game_over)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (game_stopped) { resume_game(); }
            else              { stop_game(); }
        }

        if (GI.card_system.is_memorization_phase || !GI.card_system.is_player_turn || game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        { // Update Camera
            if (Input.GetKeyDown(KeyCode.W))
            {
                update_camera_point(current_camera_point + 1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                update_camera_point(current_camera_point - 1);
            }

            if (update_camera_point_t > 0f)
            {
                update_camera_point_t -= dt*update_camera_point_animation_speed;
                if (update_camera_point_t <= 0f)
                {
                    update_camera_point_t = 0f;
                }

                Transform start  = camera_points[previous_camera_point];
                Transform target = camera_points[current_camera_point];

                player_camera.transform.position =    Vector3.Lerp(start.position, target.position, 1f - update_camera_point_t);
                player_camera.transform.rotation = Quaternion.Lerp(start.rotation, target.rotation, 1f - update_camera_point_t);
            }
        }

        { // Select Card
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = player_camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
                {
                    CardCollider card_collider = hit.collider.gameObject.GetComponent<CardCollider>();
                    Card card          = card_collider.card;
                    BossCard boss_card = card_collider.boss_card;

                    if (card)
                    {
                        if (card.is_in_desk)
                        {
                            if (selected_cards.Count > 0)
                            {
                                // Swap card in hand with card in the desk
                                Card card_to_move_to_desk = selected_cards[selected_cards.Count - 1];
                                int index_in_hand = remove_card_from_hand(card_to_move_to_desk);

                                int index_in_desk = GI.card_system.remove_card_from_desk(card);
                                add_card_to_hand(card, index_in_hand, true);

                                GI.card_system.add_card_to_desk(card_to_move_to_desk, index_in_desk);
                                decrease_actions_remaining();
                            }
                            else if (has_available_space_in_hand())
                            {
                                // Add card to hand
                                int index_in_desk = GI.card_system.remove_card_from_desk(card);
                                card.vfx_steal.pontoA = GI.card_system.cards_spawn_points[index_in_desk];

                                int first_available_index = -1;
                                for (int i = 0; i < cards_in_hand.Length; i++)
                                {
                                    if (cards_in_hand[i] == null)
                                    {
                                        first_available_index = i;
                                        break;
                                    }
                                }
                                add_card_to_hand(card, first_available_index, animate: true);
                                decrease_actions_remaining();
                            }
                        }
                        else if (!card.is_in_desk && is_card_in_hand(card))
                        {
                            if (selected_cards.Contains(card))
                            {
                                deselect_card(card);
                            }
                            else
                            {
                                select_card(card);
                            }
                        }
                    }
                    else if (boss_card && boss_card.disable_type == Boss_Card_Disable_Type.CHIPS)
                    {
                        if (health >= boss_card.cost_to_disable_ability)
                        {
                            boss_card.disable_ability();
                            decrease_actions_remaining();

                            take_damage(boss_card.cost_to_disable_ability);

                            int available_index = get_first_available_index_in_hand();
                            if (boss_card.ability_type == Boss_Abilities.DEMOTE_CARD_BY_X_POINTS && available_index > -1)
                            {
                                Card dizziness_card = Instantiate(GI.boss.dizzinnes_card_prefab).GetComponent<Card>();
                                add_card_to_hand(dizziness_card, available_index);
                            }
                        }
                    }
                }
            }
        }

        if (disable_trio_cards_t > 0f)
        {
            disable_trio_cards_t -= dt;
            if (disable_trio_cards_t <= 0f)
            {
                current_trio_card_to_disable++;

                Card card = cards_in_trio[current_trio_card_to_disable];
                card.disable_from_trio();

                // Enable next card (if there is any)
                if (current_trio_card_to_disable < cards_in_trio.Count - 1)
                {
                    disable_trio_cards_t = 2f;
                }
                else
                {
                    applying_trio_card_abilities = false;
                    if (GI.boss.health <= 0)
                    {
                        GI.boss.maybe_kill_boss();
                    }
                    else
                    {
                        maybe_update_turn();
                    }
                }
            }
        }

        { // Make Trio
            if (Input.GetKeyDown(KeyCode.Space) && selected_cards.Count == 3 && actions_remaining > 0)
            {
                // Place cards in desk
                for (int i = 0; i < 3; i++)
                {
                    Card card = selected_cards[0];
                    remove_card_from_hand(card);

                    if (i == 0 && GI.boss.is_card_in_desk(Boss_Abilities.DESTROY_TRIO_CARD_ON_THE_LEFT))
                    {
                        card.destroy();
                    }
                    else
                    {
                        cards_in_trio.Add(card);
                        card_types_in_trio.Add(card.type);
                        families_in_trio[i] = card.family_type;

                        update_trio_card_position(card);
                    }
                }

                bool can_make_secret_interactions = true;
                for (int i = 0; i < cards_in_trio.Count; i++)
                {
                    Card card = cards_in_trio[i];

                    // Disable boss cards abilities
                    for (int j = 0; j < GI.boss.cards_in_desk.Length; j++)
                    {
                        BossCard boss_card = GI.boss.cards_in_desk[j];
                        if (boss_card && 
                            boss_card.disable_type == Boss_Card_Disable_Type.FAMILY &&
                            cards_in_trio[i].family_type == boss_card.families_to_disable_card[boss_card.current_family_to_disable])
                        {
                            boss_card.current_family_to_disable++;
                            if (boss_card.current_family_to_disable >= boss_card.families_to_disable_card.Length)
                            {
                                boss_card.disable_ability();
                            }
                        }
                    }

                    // Interactions
                    if (can_make_secret_interactions)
                    {
                        if (card.type == Card_Type.BIG_BAD_WOLF && (card_types_in_trio.Contains(Card_Type.WOODEN_HOUSE_PIG) ||
                                                                    card_types_in_trio.Contains(Card_Type.BRICK_HOUSE_PIG) ||
                                                                    card_types_in_trio.Contains(Card_Type.STRAW_HOUSE_PIG)))
                        {
                            can_make_secret_interactions = false;

                            card.improve_points(card.attack_amount);
                            card.ignore_card_ability = true;

                            int index = card_types_in_trio.IndexOf(Card_Type.WOODEN_HOUSE_PIG);
                            if (index == -1) index = card_types_in_trio.IndexOf(Card_Type.BRICK_HOUSE_PIG);
                            if (index == -1) index = card_types_in_trio.IndexOf(Card_Type.STRAW_HOUSE_PIG);

                            destroy_card_from_trio(index);

                            if (index < i)
                            {
                                i--;
                            }
                        }
                        else if (card.type == Card_Type.LITTLE_RED_RIDING_HOOD && card_types_in_trio.Contains(Card_Type.BIG_BAD_WOLF) &&
                                has_available_space_in_hand())
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            Card card_to_spawn = Instantiate(GI.card_system.granny_card_prefab).GetComponent<Card>();
                            add_card_to_hand(card_to_spawn, get_first_available_index_in_hand());

                            destroy_card_from_trio(card_types_in_trio.IndexOf(Card_Type.BIG_BAD_WOLF));
                        }
                        else if (card.type == Card_Type.GEPETTO && card_types_in_trio.Contains(Card_Type.BAD_WITCH) &&
                                                                   card_types_in_trio.Contains(Card_Type.WOODEN_HOUSE_PIG))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            Card card_to_spawn = Instantiate(GI.card_system.pinocchio_card_prefab).GetComponent<Card>();
                            add_card_to_hand(card_to_spawn, get_first_available_index_in_hand());
                        }
                        else if (card.type == Card_Type.BAD_WITCH && card_types_in_trio.Contains(Card_Type.SLEEPING_BEAUTY))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            int sleeping_beauty_index = card_types_in_trio.IndexOf(Card_Type.SLEEPING_BEAUTY);
                            cards_in_trio[sleeping_beauty_index].remove_points(cards_in_trio[sleeping_beauty_index].attack_amount);

                            sabotage_random_boss_ability();
                        }
                        else if (card.type == Card_Type.GRANNY && card_types_in_trio.Contains(Card_Type.BRICK_HOUSE_PIG))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            card.improve_points(4);

                            int card_index = card_types_in_trio.IndexOf(Card_Type.BRICK_HOUSE_PIG);
                            cards_in_trio[card_index].improve_points(4);
                        }
                        else if (card.type == Card_Type.GRANNY && card_types_in_trio.Contains(Card_Type.LITTLE_RED_RIDING_HOOD))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            int card_index = card_types_in_trio.IndexOf(Card_Type.LITTLE_RED_RIDING_HOOD);
                            cards_in_trio[card_index].improve_points(6);
                        }
                        else if (card.type == Card_Type.PRINCESS_AND_FROG && (card_types_in_trio.Contains(Card_Type.SLEEPING_BEAUTY) ||
                                                                              card_types_in_trio.Contains(Card_Type.CINDERELLA)))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            card.destroy();

                            spawn_card_in_trio(GI.card_system.human_frog_card_prefab, i);
                        }
                        else if (card.type == Card_Type.BAD_WITCH && card_types_in_trio.Contains(Card_Type.HUMAN_FROG))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            // Remove Human Frog
                            int index_to_remove = card_types_in_trio.IndexOf(Card_Type.HUMAN_FROG);
                            cards_in_trio[index_to_remove].destroy();

                            // Spawn Princess and Frog
                            spawn_card_in_trio(GI.card_system.get_card_prefab(Card_Type.PRINCESS_AND_FROG), index_to_remove);

                            // Sabotage Boss ability
                            sabotage_random_boss_ability();
                        }
                        else if (card.type == Card_Type.GEPETTO && card_types_in_trio.Contains(Card_Type.PINOCCHIO))
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            int card_index = card_types_in_trio.IndexOf(Card_Type.PINOCCHIO);
                            cards_in_trio[card_index].improve_points(3);

                            improve_upcoming_pinocchios = true;
                        }
                        else if (card.type == Card_Type.BAD_WITCH && card_types_in_trio.Contains(Card_Type.PINOCCHIO) && 
                                                                     has_available_space_in_hand())
                        {
                            can_make_secret_interactions = false;
                            card.ignore_card_ability = true;

                            Card card_to_spawn = Instantiate(GI.card_system.pinocchio_card_prefab).GetComponent<Card>();
                            card_to_spawn.attack_1 = Attack_Type.DAMAGE_PLAYER;
                            add_card_to_hand(card_to_spawn, get_first_available_index_in_hand());
                        }
                    }

                    // Activate ability
                    activate_card_ability(cards_in_trio[i]);
                }

                disable_trio_cards_t = 1f;
                current_trio_card_to_disable = -1;
                applying_trio_card_abilities = true;

                // Reorder cards in hand
                reorder_cards_in_hand();

                // Activate Sleeping Beauty ability
                if (is_card_in_hand(Card_Type.SLEEPING_BEAUTY))
                {
                    for (int i = 0; i < cards_in_hand.Length; i++)
                    {
                        Card current_card = cards_in_hand[i];
                        if (current_card && current_card.type == Card_Type.SLEEPING_BEAUTY)
                        {
                            activate_card_ability(current_card);
                        }
                    }
                }

                decrease_actions_remaining();
            }
        }
    }

    public void update_camera_point(int next_point, bool update_immediately = false)
    {
        previous_camera_point = current_camera_point;
        current_camera_point = Mathf.Clamp(next_point, 0, camera_points.Length - 1);

        if (update_immediately)
        {
            player_camera.transform.position = camera_points[current_camera_point].position;
            player_camera.transform.rotation = camera_points[current_camera_point].rotation;
        }
        else
        {
            do_update_camera_point_animation = true;
            update_camera_point_t = 1f;
        }
    }

    public void sabotage_random_boss_ability()
    {
        BossCard card_to_sabotage = GI.boss.get_a_random_card_from_desk();
        if (card_to_sabotage)
        {
            card_to_sabotage.turn_off(1);
        }
    }

    public void spawn_card_in_trio(GameObject card_prefab, int index)
    {
        Card card_to_spawn = Instantiate(card_prefab).GetComponent<Card>();

        cards_in_trio[index]      = card_to_spawn;
        card_types_in_trio[index] = card_to_spawn.type;
        families_in_trio[index]   = card_to_spawn.family_type;
        update_trio_card_position(card_to_spawn);
    }

    public void destroy_card_from_trio(int index)
    {
        cards_in_trio[index].destroy();
        cards_in_trio.RemoveAt(index);
        card_types_in_trio.RemoveAt(index);
        families_in_trio[index] = Family_Type.COUNT;
    }

    public int get_first_available_index_in_hand()
    {
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            if (!cards_in_hand[i])
            {
                return i;
            }
        }

        return -1;
    }

    public void reorder_cards_in_hand()
    {
        int first_available_index = -1;
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            if (cards_in_hand[i] == null && first_available_index < 0)
            {
                // Sets the first available index
                first_available_index = i;
            }
            else if (cards_in_hand[i] != null && first_available_index >= 0)
            {
                // Moves the card to the first available index
                Card card = cards_in_hand[i];
                cards_in_hand[first_available_index] = card;
                cards_in_hand[i] = null;

                Transform spawn_point = cards_spawn_points[first_available_index];
                card.transform.position = spawn_point.position;
                card.transform.rotation = spawn_point.rotation;

                i = first_available_index;
                first_available_index = -1;
            }
        }
    }

    public bool is_family_type_in_trio(Family_Type family)
    {
        for (int i = 0; i < cards_in_trio.Count; i++)
        {
            if (cards_in_trio[i].family_type == family)
            {
                return true;
            }
        }

        return false;
    }

    public int family_count_in_trio(Family_Type family)
    {
        int count = 0;
        for (int i = 0; i < cards_in_trio.Count; i++)
        {
            if (cards_in_trio[i].family_type == family)
            {
                count++;
            }
        }

        return count;
    }

    public void activate_card_ability(Card card)
    {
        int card_index = cards_in_trio.IndexOf(card);
        card.do_attack();

        if (card.ignore_card_ability) { return; }

        switch (card.type)
        {
            case Card_Type.STRAW_HOUSE_PIG:
                {
                    if (is_family_type_in_trio(Family_Type.PAW) && is_family_type_in_trio(Family_Type.LOTUS))
                    {
                        for (int i = card_index + 1; i < cards_in_trio.Count; i++)
                        {
                            cards_in_trio[i].improve_points(2);
                        }
                    }
                } break;
            case Card_Type.WOODEN_HOUSE_PIG:
                {
                    if (family_count_in_trio(Family_Type.PAW) == 2)
                    {
                        if (card_index < cards_in_trio.Count - 1)
                        {
                            cards_in_trio[card_index + 1].swap_attacks();
                        }
                        else
                        {
                            card.swap_attacks();
                        }
                    }
                } break;
            case Card_Type.BRICK_HOUSE_PIG:
                {
                    card.improve_points(2);
                    if (card_index > 0)
                    {
                        for (int i = card_index - 1; i >= 0; i--)
                        {
                            cards_in_trio[i].remove_points(2);
                        }
                    }
                }
                break;
            case Card_Type.UGLY_DUCK:
                {
                    if (!cards_in_trio.Contains(card))
                    {
                        card.improve_points(2);
                    }
                } break;
            case Card_Type.GEPETTO:
                {
                    if (family_count_in_trio(Family_Type.LOTUS) == 2)
                    {
                        if (card_index == 1)
                        {
                            cards_in_trio[0].improve_points(2);
                            cards_in_trio[2].improve_points(2);
                        }
                    }
                } break;
            case Card_Type.BAD_WITCH:
                {
                    if (is_family_type_in_trio(Family_Type.PAW) && 
                        is_family_type_in_trio(Family_Type.CANDY) &&
                        is_family_type_in_trio(Family_Type.LOTUS))
                    {
                        BossCard boss_card = GI.boss.get_a_random_card_from_desk();
                        if (boss_card)
                        {
                            boss_card.turn_off(1);

                            if      (boss_card.attack_1 == Attack_Type.HEAL_PLAYER) boss_card.swap_attack(ref boss_card.attack_1);
                            else if (boss_card.attack_2 == Attack_Type.HEAL_PLAYER) boss_card.swap_attack(ref boss_card.attack_2);
                        }
                    }
                } break;
            case Card_Type.PUSS_IN_BOOTS:
                {
                    // Swap cards in hand
                    for (int i = 0; i < cards_in_hand.Length; i++)
                    {
                        if (cards_in_hand[i])
                        {
                            cards_in_hand[i].swap_attacks();
                        }
                    }

                    // Swap cards in trio
                    for (int i = 0; i < cards_in_trio.Count; i++)
                    {
                        if (cards_in_trio[i] != card)
                        {
                            cards_in_trio[i].swap_attacks();
                        }
                    }
                } break;
            case Card_Type.LITTLE_RED_RIDING_HOOD:
                {
                    Family_Type family_to_promote = Family_Type.LOTUS;

                    for (int i = 0; i < cards_in_hand.Length; i++)
                    {
                        Card current_card = cards_in_hand[i];
                        if (current_card && current_card.family_type == family_to_promote)
                        {
                            current_card.improve_points(2);
                        }
                    }
                } break;
            case Card_Type.CINDERELLA:
                {
                    if (!cards_in_trio.Contains(card))
                    {
                        card.add_points_temporarily(8, 3);
                    }
                } break;
            case Card_Type.BIG_BAD_WOLF:
                {
                    // Demote cards in hand
                    int points_to_improve = 0;
                    for (int i = 0; i < cards_in_hand.Length; i++)
                    {
                        if (cards_in_hand[i])
                        {
                            cards_in_hand[i].remove_points(1);
                            points_to_improve++;
                        }
                    }

                    // Demote cards in trio
                    for (int i = 0; i < cards_in_trio.Count; i++)
                    {
                        if (cards_in_trio[i] != card)
                        {
                            cards_in_trio[i].remove_points(1);
                            points_to_improve++;
                        }
                    }

                    card.improve_points(points_to_improve);
                } break;
            case Card_Type.SLEEPING_BEAUTY:
                {
                    if (!cards_in_trio.Contains(card))
                    {
                        card.improve_points_after_a_trio_to_self_demote_after_turn(4);
                    }
                } break;
            case Card_Type.PRINCESS_AND_FROG:
                {
                    if (!is_family_type_in_trio(Family_Type.PAW))
                    {
                        for (int i = 0; i < cards_in_trio.Count; i++)
                        {
                            cards_in_trio[i].improve_points(2);
                        }
                    }
                } break;
            case Card_Type.GRANNY:
                {
                    if (family_count_in_trio(Family_Type.LOTUS) == 3)
                    {
                        for (int i = 0; i < GI.card_system.cards_in_desk.Length; i++)
                        {
                            Card card_in_desk = GI.card_system.cards_in_desk[i];
                            if (card_in_desk)
                            {
                                card_in_desk.improve_points(2);
                            }
                        }
                    }
                } break;
            case Card_Type.PINOCCHIO:
                {
                    sabotage_random_boss_ability();
                } break;
            case Card_Type.HUMAN_FROG:
                {
                    if (family_count_in_trio(Family_Type.LOTUS) == 3)
                    {
                        card.improve_points(5);
                    }
                    else
                    {
                        if (card_index > 0) { cards_in_trio[card_index - 1].remove_points(5); }
                        if (card_index < 2) { cards_in_trio[card_index + 1].remove_points(5); }
                    }
                } break;
            case Card_Type.USELESS_DWARF:
                {
                    // Nothing
                } break;
            case Card_Type.ANNOYING_DWARF:
                {
                    // Implemented at decrease_actions_remaining()
                }
                break;
            case Card_Type.DIZZINESS:
                {
                    // Nothing
                } break;
            default: Debug.Assert(false, "ability not implemented for " + card.type); break;
        }
    }

    public void update_trio_card_position(Card card)
    {
        int index = cards_in_trio.IndexOf(card);

        card.transform.position = trio_spawn_points[index].position;
        card.transform.rotation = trio_spawn_points[index].rotation;
    }

    public void init()
    {
        gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void start_game()
    {
        // Clears data from previous match
        if (game_started)
        {
            for (int i = 0; i < cards_in_hand.Length; i++)
            {
                if (cards_in_hand[i])
                {
                    cards_in_hand[i].destroy();
                    cards_in_hand[i] = null;
                }
            }
            selected_cards.Clear();

            for (int i = 0; i < cards_in_trio.Count; i++)
            {
                cards_in_trio[i].destroy();
            }
            cards_in_trio.Clear();
        }

        game_started = true;

        cards_in_hand = new Card[MAX_CARDS_IN_HAND];

        camera_start_position = player_camera.transform.position;
        camera_start_rotation = player_camera.transform.rotation;

        health = 100;
        GI.player_hud.update_player_health_text();
        applying_trio_card_abilities = false;
    }

    public void start_turn()
    {
        actions_remaining = 2;
        GI.player_hud.update_actions_remaining_text();

        // Promote all UGLY DUCKS once
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            Card card = cards_in_hand[i];
            if (card && card.type == Card_Type.UGLY_DUCK && !card.promoted_at_start_of_turn)
            {
                activate_card_ability(card);
                card.promoted_at_start_of_turn = true;
            }
        }

        // Reset trio data
        cards_in_trio.Clear();
        card_types_in_trio.Clear();
        for (int i = 0; i < families_in_trio.Length; i++)
        {
            families_in_trio[i] = Family_Type.COUNT;
        }
    }

    public void add_health(int amount)
    {
        health += amount;
        GI.player_hud.update_player_health_text();
    }

    public void take_damage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            lose();
        }

        GI.player_hud.update_player_health_text();
    }

    public void decrease_actions_remaining()
    {
        actions_remaining--;
        maybe_update_turn();

        // Demote Sleeping Beauty once
        if (actions_remaining < 1)
        {
            if (is_card_in_hand(Card_Type.SLEEPING_BEAUTY))
            {
                for (int i = 0; i < cards_in_hand.Length; i++)
                {
                    Card current_card = cards_in_hand[i];
                    if (current_card && current_card.type == Card_Type.SLEEPING_BEAUTY && current_card.has_improved_after_a_trio)
                    {
                        current_card.remove_points(2);
                        current_card.has_improved_after_a_trio = false;
                    }
                }
            }

            if (is_card_in_hand(Card_Type.ANNOYING_DWARF))
            {
                take_damage(2);
            }
        }

        GI.player_hud.update_actions_remaining_text();
    }

    public void maybe_update_turn()
    {
        if (actions_remaining <= 0 && !applying_trio_card_abilities)
        {
            GI.card_system.update_turn();
        }
    }

    public bool has_available_space_in_hand()
    {
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            if (cards_in_hand[i] == null)
            {
                return true;
            }
        }

        return false;
    }

    public bool is_card_in_hand(Card card)
    {
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            if (cards_in_hand[i] == card)
            {
                return true;
            }
        }

        return false;
    }

    public bool is_card_in_hand(Card_Type type)
    {
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            Card card = cards_in_hand[i];
            if (card && card.type == type)
            {
                return true;
            }
        }

        return false;
    }

    public void select_card(Card card)
    {
        card.select_card.Active();
        selected_cards.Add(card);
    }

    public void deselect_card(Card card)
    {
        if (!selected_cards.Contains(card)) 
        { 
            return; 
        }

        card.select_card.Active();
        selected_cards.Remove(card);
    }

    public void add_card_to_hand(Card card, int index, bool animate = false)
    {
        card.add_to_player_hand();
        cards_in_hand[index] = card;

        if (animate)
        {
            card.vfx_steal.pontoB = cards_spawn_points[index];
            card.vfx_steal.Active();
        }
        else
        {
            card.transform.parent   = cards_spawn_points[index];
            card.transform.position = cards_spawn_points[index].position;
            card.transform.rotation = cards_spawn_points[index].rotation;
        }

        if (card.type == Card_Type.CINDERELLA)
        {
            activate_card_ability(card);
        }

        if (card.type == Card_Type.PINOCCHIO && improve_upcoming_pinocchios)
        {
            card.improve_points(3);
        }
    }

    public int remove_card_from_hand(Card card)
    {
        deselect_card(card);
        for (int i = 0; i < cards_in_hand.Length; i++)
        {
            if (cards_in_hand[i] == card)
            {
                cards_in_hand[i] = null;
                return i;
            }
        }

        return -1;
    }

    public void enable_memorization_phase_camera_view()
    {
        update_camera_point(2, update_immediately: true);
    }

    public void enable_gameplay_camera_view()
    {
        update_camera_point(1);
    }

    public void stop_game()
    {
        Time.timeScale = 0f;
        game_stopped = true;

        GI.player_hud.show_pause();
    }

    public void resume_game()
    {
        Time.timeScale = 1f;
        game_stopped = false;

        GI.player_hud.hide_pause();
    }

    public void win()
    {
        Time.timeScale = 0f;
        game_over = true;

        GI.player_hud.show_win();
    }

    public void lose()
    {
        Time.timeScale = 0f;
        game_over = true;

        GI.player_hud.show_lose();
    }
}
