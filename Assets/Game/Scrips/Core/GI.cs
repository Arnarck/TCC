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
                    if      (cardType == Card_Type.DWARF) { return "Essa carta e destruida depois de jogada ou descartada"; }
                    else if (cardType == Card_Type.FROG)  { return "(Requer: Princesa (1)): Se transforme de volta em principe"; }

                    Debug.LogError("No ability description created for " + cardType + " card!");
                    return "";
                }
            case Ability_Type.IMPROVE_ANOTHER_CARD_BY_X_POINTS:                      return "Escolha um Personagem na sua mao e ele sera aprimorado em 5 fichas";
            case Ability_Type.REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS:                return "Escolha um Personagem na mao de outro jogador e o rebaixe em 5 fichas";
            case Ability_Type.STEAL_ANOTHER_PLAYER_CARD:                             return "Escolha outro jogador e roube uma de suas cartas";
            case Ability_Type.STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE: return "Escolha outro jogador, roube 5 fichas dele e as de ao jogador com menos";
            case Ability_Type.SPAWN_DWARVES_IN_PLAYER_HAND_UNTIL_ITS_FULL:           return "Escolha um jogador e crie anoes em sua mao ate que ela esteja cheia";
            case Ability_Type.TURN_A_PLAYER_CARD_INTO_A_FROG:                        return "Escolha outro jogador e transforme um dos personagem aleatorio em sua mao em um sapo";
            case Ability_Type.SHUFFLE_ADJACENT_CARDS:                                return "Escolha uma carta da mesa, embaralhe ela e as adjacentes";
        }

        Debug.LogError("No ability description created for " + abilityType + " ability!");
        return "";
    }
}
