using UnityEngine;
using Utp;
using Mirror;
using System.Collections.Generic;

public class CardNetworkManager : RelayNetworkManager
{
    [Header("Card Game")]
    public GameObject cardDeskPrefab;

    public override void OnStartServer()
    {
        GameObject cardDesk = Instantiate(cardDeskPrefab);
        NetworkServer.Spawn(cardDesk);

        GI.cardSystem.SpawnCard();
        GI.cardSystem.SpawnCard();
    }
}