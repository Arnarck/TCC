using UnityEngine;
using Utp;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class CardNetworkManager : RelayNetworkManager
{
    [Header("Card Game")]
    public GameObject cardDeskPrefab;
    public CardList cardList;
    public int anteStartPrice;

    [Header("Card Game - INTERNAL")]
    public int currentRound;
    public int antePrice;
    public int currentPlayerTurnIndex;
    public bool gameStarted;
    public List<NetworkConnectionToClient> players;
    public List<NetworkConnectionToClient> spectators;

    public DeckManager deckManagerPrefab;
    private DeckManager deckManager;

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

    public override void Awake()
    {
        base.Awake();

        players = new List<NetworkConnectionToClient>();
        spectators = new List<NetworkConnectionToClient>();
        GI.networkManager = this;

        antePrice = anteStartPrice;
    }

    public override void OnStartServer()
    {
        GameObject deckObj = Instantiate(deckManagerPrefab.gameObject);
        NetworkServer.Spawn(deckObj);
        deckManager = deckObj.GetComponent<DeckManager>();
        deckManager.InitializeDeck();

        GameObject cardDesk = Instantiate(cardDeskPrefab);
        NetworkServer.Spawn(cardDesk);
        GI.cardSystem.deckManager = deckManager;
        GI.cardSystem.FillInitialTable();


    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        players.Add(conn);

        if (players.Count >= 2 && !gameStarted)
        {
            gameStarted = true;
            GI.cardSystem.StartMemorizationPhase();
            StartCoroutine(WaitForMemorizationAndStartGame());
        }
    }
    IEnumerator WaitForMemorizationAndStartGame()
    {
        while (GI.cardSystem.isMemorizationPhase)
            yield return null;
        UpdatePlayerTurn();
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        // Update the rounds if a player get disconnected from the match
        players.Remove(conn);
        spectators.Remove(conn);
        if (currentPlayerTurnIndex >= players.Count)
        {
            UpdateRound();
        }
    }

    // @TODO: Only update player turn after memorization phase
    [Server]
    public void UpdatePlayerTurn()
    {
        currentPlayerTurnIndex++;
        if (currentPlayerTurnIndex >= players.Count)
        {
            UpdateRound();
        }

        // Update the turn in the HUD
        for (int i = 0; i < players.Count; i++)
        {
            PlayerController player = players[i].identity.GetComponent<PlayerController>();
            if (i == currentPlayerTurnIndex)
            {
                player.playerHUD.TargetDisplayTurn("Your Turn");
            }
            else
            {
                player.playerHUD.TargetDisplayTurn("Player " + (i + 1) + " turn");
            }
        }

        if (players.Count > 0)
        {
            players[currentPlayerTurnIndex].identity.GetComponent<PlayerController>().ServerStartCurrentTurn(30f);
        }
    }

    [Server]
    public void UpdateRound()
    {
        currentRound++;
        currentPlayerTurnIndex = 0;

        GI.cardSystem.ServerUpdateCurrentRound(currentRound);



        if (currentRound % 3 == 0)
        {
            StartCoroutine(HandleAnteRound());
        }
        // Check for ante round
        if (currentRound > 0 && currentRound % 3 == 0)
        {
            // Charge ante and disconnect players that don't have enough score
            for (int i = 0; i < players.Count; i++)
            {
                PlayerController player = players[i].identity.GetComponent<PlayerController>();
                if (player.score >= antePrice)
                {
                    player.score -= antePrice;
                }
                else
                {
                    // Spectator Mode
                    spectators.Add(players[i]);
                    players.Remove(players[i]);

                    player.ServerEnterSpectatorMode();
                    i--;
                }
            }

            // Check for win/lose conditions
            if (players.Count == 1)
            {
                players[0].identity.GetComponent<PlayerController>().ServerWin();
                for (int i = 0; i < spectators.Count; i++)
                {
                    spectators[i].identity.GetComponent<PlayerController>().ServerLose();
                }
            }
            else if (players.Count == 0)
            {
                for (int i = 0; i < spectators.Count; i++)
                {
                    spectators[i].identity.GetComponent<PlayerController>().ServerLose();
                }
            }
        }
    }

    [Server]
    IEnumerator HandleAnteRound()
    {
        PauseAllPlayers();

        for (int i = 0; i < players.Count; i++)
        {
            PlayerController player = players[i].identity.GetComponent<PlayerController>();

            if (player.score >= antePrice)
            {
                player.score -= antePrice;
            }
            else
            {
                spectators.Add(players[i]);
                players.Remove(players[i]);

                player.ServerEnterSpectatorMode();
                i--;
            }
        }

        GI.cardSystem.RefillTableFromDeck();

        GI.cardSystem.StartMemorizationPhase();

        yield return new WaitForSeconds(GI.cardSystem.memorizeTime);

        ResumeAllPlayers();

        currentPlayerTurnIndex = -1;

        UpdatePlayerTurn();
    }
    [Server]
    void PauseAllPlayers()
    {
        foreach (var p in players)
        {
            p.identity.GetComponent<PlayerController>().TargetPauseTurn();
        }
    }
    [Server]
    void ResumeAllPlayers()
    {
        foreach (var p in players)
        {
            p.identity.GetComponent<PlayerController>().ServerStartCurrentTurn(30f);
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