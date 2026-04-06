using UnityEngine;
using System.Collections;
using Mirror;
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
    ABILITY_1,
    ABILITY_2,
    ABILITY_3,

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
