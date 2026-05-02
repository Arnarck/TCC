using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DeckManager : NetworkBehaviour
{
    [SerializeField] private int copiesPerType = 3;
    private List<Card_Type> deck = new List<Card_Type>();

    [Server]
    public void InitializeDeck()
    {
        deck.Clear();
        int typeCount = (int)Card_Type.COUNT;
        for (int t = 0; t < typeCount; t++)
        {
            for (int i = 0; i < copiesPerType; i++)
            {
                deck.Add((Card_Type)t);
            }
        }
        Shuffle();
        Debug.Log($"Deck inicializado com {deck.Count} cartas.");
    }

    [Server]
    public void Shuffle()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }
    }

    [Server]
    public bool HasCards() => deck.Count > 0;

    [Server]
    public Card_Type DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("Deck vazio! Não é possível comprar carta.");
            return Card_Type.IMPROVE; // fallback
        }
        Card_Type drawn = deck[0];
        deck.RemoveAt(0);
        return drawn;
    }

    [Server]
    public void AddCard(Card_Type type)
    {
        deck.Add(type);
        // Shuffle();
    }
}