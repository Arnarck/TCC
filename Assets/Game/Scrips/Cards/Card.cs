using UnityEngine;
using System.Collections;
using Mirror;

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

public class Card : NetworkBehaviour
{
    public Card_Type type;
    public int points;
    public Family_Type familyType;
    public Ability_Type abilityType;
    public GameObject visual;
    public BoxCollider boxCollider;
    public LayerMask cardSystemMask;

    [SyncVar(hook = nameof(OnRevealChanged))]
    public bool isRevealed = true;

    [Header("INTERNAL")]
    public bool spawnedInDesk;
    public bool collidedWithCardSystem;
    [SyncVar]public int improvedPoints;


    private void Update()
    {
        if (GI.cardSystem.localPlayerSpawned && !collidedWithCardSystem)
        {
            // Checks Collision against other objects
            Collider[] results = new Collider[1];
            if (Physics.OverlapBoxNonAlloc(transform.position, boxCollider.size*0.5f, results, transform.rotation, cardSystemMask) > 0)
            {
                collidedWithCardSystem = true;
                if (results[0].gameObject.layer == 7) // CardSystem layer
                {
                    // Makes the cards rotate towards player's camera. So all 4 players will look the cards from the front side,
                    // intead of looking from sideways.
                    spawnedInDesk = true;
                    transform.RotateAround(GI.cardSystem.transform.position, Vector3.up, GI.cardSystem.transform.eulerAngles.y);
                    visual.GetComponent<ToTurn>().Active();
                    
                }
            }

        }
    }

    void OnRevealChanged(bool oldValue, bool newValue)
    {
        //visual.SetActive(newValue);//VITOR MEXEU AQ
        visual.GetComponent<ToTurn>().Active();// @VITOR
    }

    public override void OnStartClient()
    {
        //if(spawnedInDesk) visual.GetComponent<ToTurn>().Active();//VITOR MEXEU AQ 

        //isso aq é bug ein pessoal so pra mostrar pro lipas  @TODO
        
    }

    private void OnMouseEnter()
    {
        if (!GI.playerHUD.lobbyPanel.activeSelf && GI.playerHUD.player.cardsInHand.Contains(this))
        {
            GI.playerHUD.ShowCardAbilityPanel(GI.GetCardAbilityDescription(type, abilityType), points + improvedPoints, gameObject);
        }
    }

    private void OnMouseExit()
    {
        GI.playerHUD.HideCardAbilityPanel();
    }
}
