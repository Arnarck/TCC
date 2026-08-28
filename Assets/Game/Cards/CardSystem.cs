using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// @Note: 'Command' attributes will not work here. Since the server spawns this object, it has no client to address authority to it.
// The objects needs authority (an client as a owner) to make 'Command' attributes work, so the server can know at which client the
// object belongs to.
// That's why 'SpawnCard' and 'DestroyCard' are only called inside 'Command' attributes or inside the NetworkManager. Only the server
// can call this object's methods.
public class CardSystem : MonoBehaviour
{
    public const int MAX_CARDS_IN_DESK = 16;

    public Transform cards_parent;
    public GameObject[] cards_prefabs;
    public Transform[] cards_spawn_points;

    [Header("INTERNAL")]
    public int round_count; // A round is a player turn + a boss turn
    public bool is_player_turn;
    public bool is_memorization_phase;
    public float memorization_phase_t;
    public float memorization_time = 10f;
    public Card[] cards_in_desk;

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
        start_game();
    }

    private void Update()
    {
        if (GI.player.game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (is_memorization_phase)
        {
            memorization_phase_t -= dt;
            if (memorization_phase_t <= 0f)
            {
                is_memorization_phase = false;
                GI.player.enable_gameplay_camera_view();
                GI.player_hud.end_memorization_phase();
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
        card.transform.position = spawn_point.position;
        card.transform.rotation = spawn_point.rotation;
        card.is_in_desk = true;
    }

    public void start_game()
    {
        round_count = 0;

        // Spawn random cards to desk
        spawn_cards_in_desk();

        // Start player and boss
        GI.player.start_game();
        GI.boss.start_game();

        // Spawn cards to player hand
        Card card_to_hand_1 = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)]).GetComponent<Card>();
        Card card_to_hand_2 = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)]).GetComponent<Card>();
        GI.player.add_card_to_hand(card_to_hand_1, 0);
        GI.player.add_card_to_hand(card_to_hand_2, 1);

        start_memorization_phase();

        __start_player_turn();
    }

    public void update_turn()
    {
        if (is_player_turn)
        {
            // Switch to boss turn
            is_player_turn = false;
            GI.boss.start_turn();
            GI.player_hud.hide_player_turn_message();
            GI.player_hud.show_boss_turn_message();
        }
        else
        {
            // Switch to player turn
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
        memorization_phase_t = 10f;
        GI.player.enable_memorization_phase_camera_view();
        GI.player_hud.start_memorization_phase();
    }

    public void spawn_cards_in_desk()
    {
        cards_in_desk = new Card[cards_spawn_points.Length];
        for (int i = 0; i < cards_spawn_points.Length; i++)
        {
            GameObject go = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)], cards_parent);
            add_card_to_desk(go.GetComponent<Card>(), i);
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
        GI.player.start_turn();
        GI.player_hud.hide_boss_turn_message();
        GI.player_hud.show_player_turn_message();
    }
}