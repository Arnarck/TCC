using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public Transform camera_memorization_phase_view;
    public Camera player_camera;
    public Transform[] trio_spawn_points;
    public Transform[] cards_spawn_points;
    public List<Card> selected_cards;
    public Card[] cards_in_hand;
    public List<Card> cards_in_trio;

    [Header("INTERNAL")]
    public int health;
    public int actions_remaining;
    public bool game_stopped;
    public bool game_over;
    public Vector3 camera_start_position;
    public Quaternion camera_start_rotation;

    public List<Ability_Type> abilities_to_apply;
    public Ability_Type current_ability;

    void Awake()
    {
        GI.player_card_game = this;
    }

    private void Start()
    {
        resume_game();
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

        { // Select Card
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = player_camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 6))
                {
                    CardCollider card_collider = hit.collider.gameObject.GetComponent<CardCollider>();
                    Card card = card_collider.card;
                    if (card.is_in_desk)
                    {
                        if (selected_cards.Count > 0)
                        {
                            // Swap card in hand with card in the desk
                            Card card_to_move_to_desk = selected_cards[selected_cards.Count - 1];
                            int index_in_hand = remove_card_from_hand(card_to_move_to_desk);

                            int index_in_desk = GI.card_system.remove_card_from_desk(card);
                            add_card_to_hand(card, index_in_hand);

                            GI.card_system.add_card_to_desk(card_to_move_to_desk, index_in_desk);
                            decrease_actions_remaining();
                        }
                        else if (has_available_space_in_hand())
                        {
                            // Add card to hand
                            GI.card_system.remove_card_from_desk(card);

                            int first_available_index = -1;
                            for (int i = 0; i < cards_in_hand.Length; i++)
                            {
                                if (cards_in_hand[i] == null)
                                {
                                    first_available_index = i;
                                    break;
                                }
                            }
                            add_card_to_hand(card, first_available_index);
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
            }
        }

        // Trio
        if (Input.GetKeyDown(KeyCode.Space) && selected_cards.Count == 3 && actions_remaining > 0)
        {
            // Place cards in desk
            for (int i = 0; i < 3; i++)
            {
                Card card = selected_cards[0];
                remove_card_from_hand(card);

                cards_in_trio.Add(card);
                card.make_trio();

                Transform trio_spawn_point = trio_spawn_points[i];
                card.transform.position = trio_spawn_point.position;
                card.transform.rotation = trio_spawn_point.rotation;

                GI.boss.take_damage(card.points);
            }

            // Reorder cards in hand
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

            decrease_actions_remaining();
        }
    }

    public void init()
    {
        gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void start_game()
    {
        current_ability = Ability_Type.NONE;
        cards_in_hand = new Card[MAX_CARDS_IN_HAND];

        camera_start_position = player_camera.transform.position;
        camera_start_rotation = player_camera.transform.rotation;

        health = 100;
        GI.player_hud.update_player_health_text();
    }

    public void start_turn()
    {
        actions_remaining = 2;
        GI.player_hud.update_actions_remaining_text();
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
        if (actions_remaining <= 0)
        {
            GI.card_system.update_turn();
        }

        GI.player_hud.update_actions_remaining_text();
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

    public void add_card_to_hand(Card card, int index)
    {
        card.is_in_desk = false;
        cards_in_hand[index] = card;

        card.transform.parent   = cards_spawn_points[index];
        card.transform.position = cards_spawn_points[index].position;
        card.transform.rotation = cards_spawn_points[index].rotation;
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
        player_camera.transform.position = camera_memorization_phase_view.transform.position;
        player_camera.transform.rotation = camera_memorization_phase_view.transform.rotation;
    }

    public void enable_gameplay_camera_view()
    {
        player_camera.transform.position = camera_start_position;
        player_camera.transform.rotation = camera_start_rotation;
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
