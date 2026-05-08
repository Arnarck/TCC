using UnityEngine;
using System.Collections;
using Mirror;

public enum Card_Type
{
    IMPROVE,
    CARD_2,
    CARD_3,
    CARD_4,
    CARD_5,
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

public enum Ability_Type
{
    IMPROVE_ANOTHER_CARD_BY_X_POINTS,
    REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS,
    STEAL_ANOTHER_PLAYER_CARD, // @TODO: Conditions to activate card abilities.
    STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE,

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
                    transform.RotateAround(GI.cardSystem.transform.position, Vector3.up, GI.cardSystem.transform.eulerAngles.y);
                }
            }

        }
    }

    void OnRevealChanged(bool oldValue, bool newValue)
    {
        visual.SetActive(newValue);
    }
}
