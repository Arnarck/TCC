using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Splines;

public class CardSystem : MonoBehaviour
{
    public const int MAX_CARDS_IN_DESK = 16;

    public float memorization_time = 10f;
    public bool start_playing_game;
    public Transform cards_parent;
    public GameObject[] cards_prefabs;
    public Transform[] rows_spawn_points;
    public Transform[] columns_spawn_points;
    public Transform[] cards_spawn_points;
    public Transform deckPoint;

    [Header("INTERNAL")]
    public int round_count; // A round is a player turn + a boss turn
    public bool playing_card_game;
    public bool is_player_turn;
    public bool is_memorization_phase;
    public float memorization_phase_t;
    public Card[] cards_in_desk;

    [Header("SPLINES")]

    public SplineContainer spline_create;

    [ContextMenu("Fill Desk With Cards")]
    public void DEBUG_spawn_cards_in_desk()
    {
        cards_in_desk = new Card[cards_spawn_points.Length];
        for (int i = 0; i < cards_spawn_points.Length; i++)
        {
            GameObject go = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)], cards_spawn_points[i]);
            add_card_to_desk(go.GetComponent<Card>(), i);
        }
    }

    [ContextMenu("Remove Cards From Desk")]
    public void DEBUG_remove_cards_from_desk()
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            Card card = cards_in_desk[i];
            if (card != null)
            {
                DestroyImmediate(card.gameObject);
            }
        }
    }


    private void Awake()
    {
        is_memorization_phase = false;
        GI.card_system = this;
    }

    private void Start()
    {
        
        if (start_playing_game)
        {
            start_game();
        }
        else
        {
            end_game();
        }
    }

    private void Update()
    {
        if (GI.player_card_game.game_stopped || !playing_card_game)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (is_memorization_phase)
        {
            memorization_phase_t -= dt;
            if (memorization_phase_t <= 0f)
            {
                // Ends memorization phase
                is_memorization_phase = false;
                GI.player_card_game.enable_gameplay_camera_view();
                GI.player_hud.end_memorization_phase();
                
                // Animation
                for (int i = 0; i < cards_in_desk.Length; i++)
                {
                    cards_in_desk[i].to_turn.Active();
                }

                // Reorder phase 2 card ability position
                //@TODO: Player cancels the card
                if (GI.boss.type == Boss_Type.CAT 
                    && GI.boss.is_phase_2 && 
                    GI.boss.is_card_in_desk(Boss_Abilities.REMOVE_PLAYER_POINTS_WHEN_SELECTING_A_CARD_FROM_A_ROW))
                {
                    GI.boss.spawn_multicard_selector_in_desk(GI.boss.remove_points_cards_collider, rows_spawn_points);
                    GI.player_hud.show_boss_attack_text("A new row was selected to remove player points");
                }
            }
        }
    }

    public int remove_card_from_desk(Card card)
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            if (cards_in_desk[i] == card)
            {
                cards_in_desk[i] = null;
                return i;
            }
        }

        return -1;
    }

    public void add_card_to_desk(Card card, int index)
    {
        cards_in_desk[index] = card;

        Transform spawn_point = cards_spawn_points[index];
        card.transform.SetParent(cards_parent);
        //card.transform.position = spawn_point.position;   @VITOR
        card.transform.rotation = spawn_point.rotation;  
        
        card.distribute_cards(deckPoint, spawn_point);        //@VITOR
        card.add_to_desk();
    }

    public void start_game()
    {
        playing_card_game = true;
        round_count = 0;
        GI.player_card_game.init();
        GI.boss.init();
        GI.player_first_person.gameObject.SetActive(false);
        GI.player_hud.show_card_game_hud();


        // Spawn random cards to desk
        spawn_cards_in_desk();

        // Start player and boss
        GI.player_card_game.start_game();
        GI.boss.start_game();

        // Spawn cards in player hand
        Card card_to_hand_1 = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)]).GetComponent<Card>();
        Card card_to_hand_2 = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)]).GetComponent<Card>();
        GI.player_card_game.add_card_to_hand(card_to_hand_1, 0);
        GI.player_card_game.add_card_to_hand(card_to_hand_2, 1);

        start_memorization_phase();

        __start_player_turn();
    }

    public void end_game()
    {
        playing_card_game = false;
        GI.player_card_game.gameObject.SetActive(false);
        GI.boss.gameObject.SetActive(false);
        GI.player_first_person.init();
        GI.player_hud.show_first_person_hud();
    }

    public void update_turn()
    {
        if (is_player_turn)
        {
            // Player Turn ends. Switch to boss turn
            is_player_turn = false;
            GI.boss.start_turn();
            GI.player_hud.hide_player_turn_message();
            GI.player_hud.show_boss_turn_message();

            // Remove points after some turns
            for (int i = 0; i < GI.player_card_game.cards_in_hand.Length; i++)
            {
                Card current_card = GI.player_card_game.cards_in_hand[i];
                if (current_card && current_card.remove_points_t > 0)
                {
                    current_card.remove_points_t--;
                    if (current_card.remove_points_t <= 0)
                    {
                        current_card.remove_points(current_card.points_to_remove_after_x_turns);
                    }
                }
            }
        }
        else
        {
            // Boss turn ends. Switch to player turn
            // Decrease cards disabled time
            for (int i = 0; i < GI.boss.cards_in_desk.Length; i++)
            {
                BossCard current_card = GI.boss.cards_in_desk[i];
                if (current_card && current_card.turn_off_t > 0)
                {
                    current_card.turn_off_t--;
                }
            }

            round_count++;
            if (round_count % 3 == 0)
            {
                // Memorization Phase
                remove_cards_from_desk();
                spawn_cards_in_desk();
                start_memorization_phase();
            }

            __start_player_turn();
        }
    }

    public void start_memorization_phase()
    {
        is_memorization_phase = true;
        memorization_phase_t = memorization_time;
        GI.player_card_game.enable_memorization_phase_camera_view();
        GI.player_hud.start_memorization_phase();
    }

    public void spawn_cards_in_desk()
    {
        cards_in_desk = new Card[cards_spawn_points.Length];
        for (int i = 0; i < cards_spawn_points.Length; i++)
        {
            Card card_to_spawn = cards_prefabs[Random.Range(0, cards_prefabs.Length)].GetComponent<Card>();

            // Spawn a card
            GameObject go = Instantiate(card_to_spawn.gameObject, cards_parent);
            Card card = go.GetComponent<Card>();
            add_card_to_desk(card, i);

            card.to_turn.Active();
        }
    }

    public void remove_cards_from_desk()
    {
        for (int i = 0; i < cards_in_desk.Length; i++)
        {
            Card card = cards_in_desk[i];
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
    }

    public void __start_player_turn()
    {
        is_player_turn = true;
        GI.player_card_game.start_turn();
        GI.player_hud.hide_boss_turn_message();
        GI.player_hud.show_player_turn_message();
    }
}