using UnityEngine;
using Utp;
using Mirror;
using System.Collections.Generic;

public class CardNetworkManager : RelayNetworkManager
{
    [Header("Card Game")]
    public GameObject cardDeskPrefab;
    public CardList cardList;


    [ContextMenu("Fill Spawnable Prefabs With Cards")]
    public void FillSpawnablePrefabsWithCards()
    {
        for (int i = 0; i < cardList.cards.Length; i++)
        {
            GameObject prefab = cardList.cards[i].prefab;
            if (!spawnPrefabs.Contains(prefab))
            {
                spawnPrefabs.Add(prefab);
            }
        }
    }

    public override void OnStartServer()
    {
        GameObject cardDesk = Instantiate(cardDeskPrefab);
        NetworkServer.Spawn(cardDesk);

        GI.cardSystem.SpawnCard(Card_Type.CARD_1);
        GI.cardSystem.SpawnCard(Card_Type.CARD_2);
    }
}