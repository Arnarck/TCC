using UnityEngine;

public enum Card_Type
{
    // Family 1
    WOODEN_HOUSE_PIG,
    PUSS_IN_BOOTS,
    BAD_WITCH,
    BIG_BAD_WOLF,

    // Family 2
    STRAW_HOUSE_PIG,
    PRINCESS_AND_FROG,
    SLEEPING_BEAUTY,
    UGLY_DUCK,
    HUMAN_FROG, // Invocable

    // Family 3
    BRICK_HOUSE_PIG,
    LITTLE_RED_RIDING_HOOD,
    GEPETTO,
    CINDERELLA,
    GRANNY, // Invocable
    PINOCCHIO, // Invocable

    COUNT
}

public enum Family_Type
{
    PAW,
    CANDY,
    LOTUS,

    COUNT // Used as a way of knowing how many elements there is in this enum.
}

public enum Ability_Type
{
    NONE,
    PROMOTE_CHARACTERS_ON_THE_RIGHT_BY_X,
    SWAP_WITH_CHARACTER_ON_THE_RIGHT,
    DEMOTE_CHARACTERS_ON_THE_LEFT_BY_X_AND_SELF_PROMOTE_BY_X,
    SELF_PROMOTE_BY_X_AT_THE_END_OF_TURN,
    PROMOTE_ADJACENT_CHARACTERS_BY_X,
    SABOTAGE_ENEMY_CARD_ABILITY,
    SWAP_CHARACTERS_IN_TRIO_AND_HAND,
    PROMOTE_ALL_CHARACTERS_FROM_FAMILY_X_IN_HAND,
    SELF_PROMOTE_BY_X_FOR_X_TURNS,
    DEMOTE_ALL_CHARACTERS_IN_HAND_AND_TRIO_BY_X_AND_SELF_PROMOTE_BY_X_FOR_EACH,
    SELF_PROMOTE_BY_X_AFTER_A_TRIO_SCORED_AND_SELF_DEMOTE_BY_X_AT_THE_END_OF_TURN,
    PROMOTE_ALL_CHARACTERS_IN_TRIO_BY_X,
    PROMOTE_ALL_CHARACTERS_ON_TABLE_BY_X,
    SABOTAGE_OPPONENT_ABILITY,
    DEMOTE_ADJACENT_CHARACTERS_BY_X,

    COUNT
}

public class Card : MonoBehaviour
{
    public Card_Type type;
    public int points;
    public Family_Type family_type;
    public Ability_Type ability_type;

    [Header("VFX")]
    public GameObject visual;
    public vfxSteal vfx_steal;
    public TeleportCard teleport_card;
    public ActiveCard active_card;
    public SelectCard select_card;
    public ToTurn to_turn;
    public vfxTransform vfx_transform;
    public vfxHandFull vfx_hand_full;
    public vfxShuffle vfx_shuffle;
    public vfxDistribute vfx_distribute;

    public bool is_revealed = true;

    [Header("INTERNAL")]
    public bool blow_up_if_selected;
    public int improved_points;
    public bool is_in_desk;
    public float disable_t;

    private void Update()
    {
        if (GI.player_card_game.game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (disable_t > 0f)
        {
            disable_t -= dt;
            if (disable_t <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void turn_card()
    {
        to_turn.Active();
        is_revealed = !is_revealed;
    }
    public void distribute_cards(Transform pointA, Transform pointB) //@VITOR
    {
        vfx_distribute.Active(pointA, pointB);
    }

    public void remove_points(int amount)
    {
        points -= amount;
        if (points < 0)
        {
            points = 0;
        }
    }

    public void add_to_player_hand()
    {
        if (blow_up_if_selected)
        {
            GI.player_card_game.lose();
            return;
        }

        is_in_desk = false;
    }

    public void add_to_desk()
    {
        is_in_desk = true;
    }

    public void disable_from_trio()
    {
        active_card.Active(transform.position, transform.rotation); // VFX
        disable_t = 2f;
    }

    public void add_points(int amount)
    {
        points += amount;
    }

    public void destroy()
    {
        gameObject.SetActive(false);
    }
}

