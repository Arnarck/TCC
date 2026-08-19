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
    public bool is_player_turn;
    public bool is_memorization_phase;
    public float memorization_time = 10f;
    public Card[] cards_in_desk;


    private void Awake()
    {
        is_memorization_phase = false;
        GI.card_system = this;
    }

    private void Start()
    {
        start_game();
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
        // Spawn random cards
        cards_in_desk = new Card[cards_spawn_points.Length];
        for (int i = 0; i < cards_spawn_points.Length; i++)
        {
            GameObject go = Instantiate(cards_prefabs[Random.Range(0, cards_prefabs.Length)], cards_parent);
            add_card_to_desk(go.GetComponent<Card>(), i);
        }

        is_player_turn = true;
        GI.player.start_turn();
    }

    public void update_turn()
    {
        if (is_player_turn)
        {
            is_player_turn = false;
            GI.boss.start_turn();
        }
        else
        {
            is_player_turn = true;
            GI.player.start_turn();
        }
    }
}