using UnityEngine;
using System.Collections;

public enum Card_Type
{
    IMPROVE,
    DWARF,
    FROG,
    PRINCESS,
    PRINCE,
    CARD_6,
    CARD_7,

    COUNT
}

public enum Family_Type
{
    FAMILY_1,
    FAMILY_2,
    FAMILY_3,
    FAMILY_4,

    COUNT // Used as a way of knowing how many elements there is in this enum.
}
public enum CharacterType
{
    NONE,
    TURTLE,
    DOLL,
    CAT,
    GOLDILOCKS
}
public static class CharacterDatabase
{
    public static Family_Type GetFamily(CharacterType character)
    {
        switch (character)
        {
            case CharacterType.TURTLE:
                return Family_Type.FAMILY_1;

            case CharacterType.DOLL:
                return Family_Type.FAMILY_2;

            case CharacterType.CAT:
                return Family_Type.FAMILY_3;

            case CharacterType.GOLDILOCKS:
                return Family_Type.FAMILY_4;
        }

        return Family_Type.FAMILY_1;
    }
}
public enum Ability_Type
{
    NONE,
    IMPROVE_ANOTHER_CARD_BY_X_POINTS,
    REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS,
    STEAL_ANOTHER_PLAYER_CARD, // @TODO: Conditions to activate card abilities.
    STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE,
    SPAWN_DWARVES_IN_PLAYER_HAND_UNTIL_ITS_FULL,
    TURN_A_PLAYER_CARD_INTO_A_FROG,
    SHUFFLE_ADJACENT_CARDS,

    COUNT
}

public class Card : MonoBehaviour
{
    public Card_Type type;
    public int points;
    public Family_Type family_type;
    public Ability_Type ability_type;
    public GameObject visual;

    public bool is_revealed = true;

    [Header("INTERNAL")]
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
        visual.GetComponent<ToTurn>().Active();
        is_revealed = !is_revealed;
    }

    public void make_trio()
    {
        disable_t = 2f;
    }
}

