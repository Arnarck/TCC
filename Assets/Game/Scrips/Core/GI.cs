using UnityEngine;

/////////////////////
/// DOCUMENTATION ///
/////////////////////
/*
Game Instances class.
This class was made to hold script references so we don't need to do 'FindObject<>' all the time whenever we need a script reference.
And as a static class, it will persist across scenes, so we can use it to store data if we need.
 */

public static class GI
{
    public static CardSystem cardSystem;
    public static CardList cardList;
    public static CardNetworkManager networkManager;
    public static PlayerHUD playerHUD;

    public static string GetCardAbilityDescription(Card_Type cardType, Ability_Type abilityType)
    {
        switch (abilityType)
        {
            case Ability_Type.NONE:
                {
                    if      (cardType == Card_Type.DWARF) { return "This card is destroyed after being played or discarded"; }
                    else if (cardType == Card_Type.FROG)  { return "(Requires: Princess (1)): Transform back into a Prince"; }

                    Debug.LogError("No ability description created for " + cardType + " card!");
                    return "";
                }
            case Ability_Type.IMPROVE_ANOTHER_CARD_BY_X_POINTS:                      return "Choose a Character in your hand. It gains 5 chips";
            case Ability_Type.REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS:                return "Choose a Character in another player's hand. It loses 5 chips";
            case Ability_Type.STEAL_ANOTHER_PLAYER_CARD:                             return "Choose another player and steal one of their cards";
            case Ability_Type.STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE: return "Choose another player, steal 5 chips from them and give them to the player with the lowest score";
            case Ability_Type.SPAWN_DWARVES_IN_PLAYER_HAND_UNTIL_ITS_FULL:           return "Choose a player and create Dwarves in their hand until it is full";
            case Ability_Type.TURN_A_PLAYER_CARD_INTO_A_FROG:                        return "Choose another player and turn a random Character in their hand into a Frog";
            case Ability_Type.SHUFFLE_ADJACENT_CARDS:                                return "Choose a card on the table and shuffle it along with its adjacent cards";
        }

        Debug.LogError("No ability description created for " + abilityType + " ability!");
        return "";
    }
}
