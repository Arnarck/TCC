using UnityEngine;

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

public class Card : MonoBehaviour
{
    public Card_Type type;
    public int points;
    public Family_Type familyType;
    public Ability_Type abilityType;

    // @TODO: Sometimes this works, sometimes it doesn't. Investigate why, and solve it.
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CardSystem>(out CardSystem cardSystem))
        {
            // Makes the cards rotate towards player's camera. So all 4 players will look the cards from the front side,
            // intead of looking from sideways.
            // Now, this was the easiest way that i found of doing it.
            // Mirror does not support spawn something across the network with a parent object. So, when the cards spawns
            // in the clients, they have no parents.
            transform.RotateAround(cardSystem.transform.position, Vector3.up, cardSystem.transform.eulerAngles.y);
        }
    }
}
