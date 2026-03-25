using UnityEngine;
using Mirror;
using System.Collections.Generic;

// @Note: 'Command' attributes will not work here. Since the server spawns this object, it has no client to address authority to it.
// The objects needs authority (an client as a owner) to make 'Command' attributes work, so the server can know at which client the
// object belongs to.
// That's why 'SpawnCard' and 'DestroyCard' are only called inside 'Command' attributes or inside the NetworkManager. Only the server
// can call this object's methods.
public class CardSystem : NetworkBehaviour
{
    public const int MAX_CARDS_IN_DESK = 16;

    public CardList cardList; // This reference is only to set it to GI.cardList, since 'Awake' apparently is not called in ScriptableObjects
    public GameObject cardPrefab;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    public List<Card> cardsInDesk;

    private void Awake()
    {
        GI.cardSystem = this;
        GI.cardList = cardList;
    }

    [Server]
    public void SpawnCard(Card_Type type)
    {
        // @TODO: Remove card from last position if someone try to spawn more than 16 cards.

        int spawnIndex = cardsInDesk.Count;
        if (spawnIndex >= MAX_CARDS_IN_DESK)
        {
            Debug.Assert(false, "The desk is already full of cards. Can't add a new one. The last one will be overwritten.");
            spawnIndex = MAX_CARDS_IN_DESK - 1;

            // @TODO: Do we really need to make a fallback here?
            DestroyCard(cardsInDesk[spawnIndex].gameObject);
        }

        Transform spawnPoint = cardsSpawnPoints[spawnIndex];

        GameObject go = Instantiate(GI.cardList.GetCardPrefab(type), spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(go, connectionToClient);

        cardsInDesk.Add(go.GetComponent<Card>());
    }

    [Server]
    public void DestroyCard(GameObject card)
    {
        cardsInDesk.Remove(card.GetComponent<Card>());
        NetworkServer.Destroy(card.gameObject);
    }
}