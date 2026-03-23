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
    public int points;
    public Family_Type familyType;
    public Ability_Type abilityType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
