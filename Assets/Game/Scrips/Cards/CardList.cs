using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Card_List_Item
{
    public Card_Type type;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "CardList", menuName = "Scriptable Objects/CardList")]
public class CardList : ScriptableObject
{
    public Card_List_Item[] cards;

    [ContextMenu("Update Cards Type")]
    public void UpdateCardsType()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].type = cards[i].prefab.GetComponent<Card>().type;
        }
    }

    public GameObject GetCardPrefab(Card_Type type)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].type == type)
            {
                return cards[i].prefab;
            }
        }

        Debug.Assert(false, "Card of type '" + type + "' was not found on the card list.");
        return null;
    }
}