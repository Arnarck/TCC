using UnityEngine;
using Utp;
using Mirror;
using System.Collections.Generic;

public class CardNetworkManager : RelayNetworkManager
{
    [Header("Card Game")]
    public GameObject cardDeskPrefab;
    public CardList cardList;

    [Header("Card Game - INTERNAL")]
    public int antePrice;
    public int currentPlayerTurnIndex;
    public bool gameStarted;
    public List<NetworkConnectionToClient> players;

    public override void Awake()
    {
        base.Awake();

        players = new List<NetworkConnectionToClient>();
        GI.networkManager = this;

        // @DELETE - Only for test while we don't have the 'rounds' feature
        antePrice = 4;
    }

    // @DELETE - Only for test while we don't have the 'rounds' feature
    [ServerCallback]
    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.S))
        {
            ChargeAnte();
        }
    }


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
        List<Card_Type> deck = new List<Card_Type>()
    {

        Card_Type.CARD_1, Card_Type.CARD_1, Card_Type.CARD_1,
        Card_Type.CARD_2, Card_Type.CARD_2, Card_Type.CARD_2,
        Card_Type.CARD_3, Card_Type.CARD_3, Card_Type.CARD_3,
        Card_Type.CARD_4, Card_Type.CARD_4, Card_Type.CARD_4,
        Card_Type.CARD_5, Card_Type.CARD_5, Card_Type.CARD_5
    };
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }

        foreach (var type in deck)
        {
            GI.cardSystem.SpawnCard(type);
        }

    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        players.Add(conn);

        if (players.Count >= 2 && !gameStarted)
        {
            gameStarted = true;

            Debug.Log("2 players conectados - iniciando memorização");

            GI.cardSystem.StartMemorizationPhase();

            UpdatePlayerTurn();
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        players.Remove(conn);
        if (currentPlayerTurnIndex >= players.Count)
        {
            // Increase round here
            currentPlayerTurnIndex = 0;
        }
    }

    [Server]
    public void UpdatePlayerTurn()
    {
        currentPlayerTurnIndex++;
        if (currentPlayerTurnIndex >= players.Count)
        {
            // Increase round here
            currentPlayerTurnIndex = 0;
        }
    }

    [Server]
    public void ChargeAnte()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerController player = players[i].identity.GetComponent<PlayerController>();
            if (player.score >= antePrice)
            {
                player.score -= antePrice;
            }
            else
            {
                // @TODO:
                // Mark players for disconnection instead of disconnecting directly here? Disconnection can happen at any time.
                // Show 'game over' screen if only one player survive - or none.
                // Should player spect the game?
                players[i].Disconnect();
            }
        }
    }

    [Server]
    public int GetCurrentPlayerTurn()
    {
        if (!gameStarted)
        {
            return -1;
        }

        return players[currentPlayerTurnIndex].connectionId;
    }
}