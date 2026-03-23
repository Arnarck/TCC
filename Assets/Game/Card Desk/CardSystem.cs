using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class CardSystem : NetworkBehaviour
{
    public GameObject cardPrefab;
    public Transform[] cardsSpawnPoints;

    private void Awake()
    {
        GI.cardSystem = this;
    }

    public override void OnStartServer()
    {
        GameObject card = Instantiate(cardPrefab, cardsSpawnPoints[15].position, cardsSpawnPoints[15].rotation);
        NetworkServer.Spawn(card, connectionToClient);
    }

    public void DestroyCard(GameObject card)
    {
        NetworkServer.Destroy(card.gameObject);
    }
}