using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public PlayerHUD playerHUD;
    public Transform cameraStartPoint;
    public Transform cameraPointWhenChoosingCards;
    public Camera playerCamera;
    public GameObject playerModel;
    public LayerMask mouseClickMasks;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    [SyncVar(hook = nameof(UpdateScore))] public int score;
    [SyncVar] public int actionsRemaining;
    [SyncVar(hook = nameof(OnRespectF1Changed))] public int respectF1 = 0;
    [SyncVar(hook = nameof(OnRespectF2Changed))] public int respectF2 = 0;
    [SyncVar(hook = nameof(OnRespectF3Changed))] public int respectF3 = 0;
    [SyncVar(hook = nameof(OnRespectF4Changed))] public int respectF4 = 0;
    [SyncVar] public float currentTurn_t;
    [SyncVar] public bool spectatorMode;
    [SyncVar] public bool gameStopped;
    [SyncVar(hook = nameof(UpdateIsChoosingCards))] public bool isChoosingCards;

    public List<Ability_Type> abilitiesToApply;
    public Ability_Type currentAbility;
    public int pointsToChooseToReduce;
    public int pointsToChooseToImprove;
    public bool canSelectOtherPlayer;
    public int scoreToStolenFromAnotherPlayer;
    public PlayerController selectedPlayer;

    public Card[] cardsInTrio;

    public List<Card> selectedCards;
    public List<Card> cardsInHand;
    private TrioSystem trioSystem = new TrioSystem();


    private void Start()
    {
        currentAbility = Ability_Type.COUNT;

        if (isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);

            Quaternion rotation = transform.rotation;
            rotation.x = 0f;
            rotation.z = 0f;
            GI.cardSystem.transform.rotation = rotation;
            GI.cardSystem.localPlayerSpawned = true;

            playerHUD.UpdateScore();

            CmdSetInitialRespect(Family_Type.FAMILY_1);

            CmdSpawnCardInHand();
            CmdSpawnCardInHand();

        }
        else
        {
            playerHUD.Hide();
        }
    }

    private void Update()
    {
        if (GI.cardSystem.isMemorizationPhase)
        {
            return;
        }
        if (gameStopped)
        {
            return;
        }

        if (!spectatorMode)
        {
            if (isLocalPlayer)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Select card
                    Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, mouseClickMasks))
                    {
                        if (hit.collider.gameObject.layer == 6)
                        {
                            Card card = hit.collider.gameObject.GetComponent<Card>();
                            CmdTryToSelectCard(card.gameObject);
                        }
                        else if (hit.collider.gameObject.layer == 8 && canSelectOtherPlayer)
                        {
                            CmdSelectPlayer(hit.collider.GetComponent<NetworkIdentity>().netId);
                        }
                    }

                }

                /* // Remove card
                 if (Input.GetMouseButtonDown(1))
                 {
                     Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                     if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
                     {
                         Card card = hit.collider.gameObject.GetComponent<Card>();
                         CmdTryToRemoveCardFromHand(card.gameObject);
                     }
                 }*/

                // @TODO: Check with design a official way of making a trio (button? mouse click in item in desk?).
                if (Input.GetKeyDown(KeyCode.C))
                {
                    CmdCheckForTrio();
                }

                // Switch camera position
                // @TODO: Align with design to know when and how the camera will change position.
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isChoosingCards = !isChoosingCards;
                    ToggleCameraPosition(isChoosingCards);
                }
            }

            // Update current turn timer
            if (currentTurn_t > 0f)
            {
                currentTurn_t -= Time.deltaTime;
                if (currentTurn_t <= 0f && isServer) // Only the server can call this function because the client can cheat the timer
                {
                    ServerEndCurrentTurn();
                }

                // Update time in the client
                if (isLocalPlayer)
                {
                    playerHUD.UpdateCurrentTurnTime();
                }
            }
        }
    }

    public void CmdSelectPlayer(uint netId)
    {
        // Search for the selected player netId
        for (int i = 0; i < GI.networkManager.players.Count; i++)
        {
            if (GI.networkManager.players[i].identity.netId == netId)
            {
                selectedPlayer = GI.networkManager.players[i].identity.GetComponent<PlayerController>();
            }
        }

        // Apply or progresses abilities
        switch (currentAbility)
        {
            case Ability_Type.STEAL_ANOTHER_PLAYER_CARD:
                {
                    playerHUD.TargetShowMessage("Now select a card from player's hand.", 1f);
                } break;
            case Ability_Type.STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE:
                {
                    __ServerStealScoreFromSelectedPlayerAndGiveToPlayerWithLowestScore(selectedPlayer);
                    selectedPlayer = null;
                    ServerActivateNextCardAbility();
                } break;
            default: break;
        }

        canSelectOtherPlayer = false;
    }

    public void UpdateIsChoosingCards(bool oldValue, bool newValue)
    {
        ToggleCameraPosition(isChoosingCards);
    }

    public void ToggleCameraPosition(bool choosingCards)
    {
        if (choosingCards)
        {
            playerCamera.transform.position = cameraPointWhenChoosingCards.position;
            playerCamera.transform.rotation = cameraPointWhenChoosingCards.rotation;
        }
        else
        {
            playerCamera.transform.position = cameraStartPoint.position;
            playerCamera.transform.rotation = cameraStartPoint.rotation;
        }
    }

    [Command]
    public void CmdEndCurrentTurn()
    {
        ServerEndCurrentTurn();
    }

    [Server]
    public void ServerEndCurrentTurn()
    {
        currentTurn_t = 0f;
        GI.networkManager.UpdatePlayerTurn();

        if (currentAbility != Ability_Type.COUNT)
        {
            for (int a = 0; a < 3; a++) // '3' because we can have a maximum of three card abilities in a trio.
            {
                switch (currentAbility)
                {
                    case Ability_Type.IMPROVE_ANOTHER_CARD_BY_X_POINTS:
                        {
                            // Improves the cards in hand in order if the time's over
                            if (cardsInHand.Count > 0)
                            {
                                int randomCard = Random.Range(0, cardsInHand.Count);
                                cardsInHand[randomCard].improvedPoints += pointsToChooseToImprove;
                            }
                        } break;
                    case Ability_Type.REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS:
                        {
                            if (GI.networkManager.players.Count > 1) // '> 1' to ignore current player
                            {
                                List<int> possiblePlayers = new List<int>();
                                // Adds possible players to randomly select
                                for (int i = 0; i < GI.networkManager.players.Count; i++)
                                {
                                    NetworkConnectionToClient conn = GI.networkManager.players[i];
                                    if (conn != connectionToClient)
                                    {
                                        possiblePlayers.Add(i);
                                    }
                                }

                                // Search for a random player with cards in hand and reduce the points of a random card.
                                while (possiblePlayers.Count > 0)
                                {
                                    int randomIndex = Random.Range(0, possiblePlayers.Count);
                                    int randomPlayer = possiblePlayers[randomIndex];
                                    possiblePlayers.RemoveAt(randomIndex);

                                    PlayerController player = GI.networkManager.players[randomPlayer].identity.GetComponent<PlayerController>();
                                    if (player.cardsInHand.Count > 0)
                                    {
                                        int randomCardIndex = Random.Range(0, player.cardsInHand.Count);
                                        player.cardsInHand[randomCardIndex].improvedPoints -= pointsToChooseToReduce;

                                        break;
                                    }
                                }

                                possiblePlayers.Clear();
                            }
                        } break;
                    case Ability_Type.STEAL_ANOTHER_PLAYER_CARD:
                        {
                            if (selectedPlayer)
                            {
                                if (selectedPlayer.cardsInHand.Count > 0)
                                {
                                    // Steal card from player hand
                                    __ServerStealRandomCardFromPlayerHand(selectedPlayer);
                                }
                                else
                                {
                                    // Search for a player with available cards to steal from
                                    __ServerSearchForPlayerWithAvailableCardsAndStealFromHim();
                                }
                            }
                            else
                            {
                                // Search for a player with available cards to steal from
                                __ServerSearchForPlayerWithAvailableCardsAndStealFromHim();
                            }
                        } break;
                    case Ability_Type.STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE:
                        {
                            PlayerController playerToStealScoreFrom = null;
                            if (GI.networkManager.players.Count > 1)
                            {
                                // Gathers all players that can be chosen to have the score stolen from
                                int[] playersToChoose = new int[GI.networkManager.players.Count - 1];
                                int currentIndex = 0;
                                for (int i = 0; i < GI.networkManager.players.Count; i++)
                                {
                                    if (GI.networkManager.players[i].identity.connectionToClient != connectionToClient)
                                    {
                                        playersToChoose[currentIndex] = i;
                                        currentIndex++;
                                    }
                                }

                                // Choose a player to stole the score
                                int randomIndex = Random.Range(0, playersToChoose.Length);
                                int playerToChoose = playersToChoose[randomIndex];
                                playerToStealScoreFrom = GI.networkManager.players[playerToChoose].identity.GetComponent<PlayerController>();
                            }
                            else
                            {
                                break;
                            }

                            __ServerStealScoreFromSelectedPlayerAndGiveToPlayerWithLowestScore(playerToStealScoreFrom);
                        } break;
                    default: break;
                }

                // We don' want to end current turn inside 'ServerApplyNextAbility' here because it will run this function twice.
                ServerActivateNextCardAbility(maybeEndCurrentTurn: false);
                if (currentAbility == Ability_Type.COUNT) // No more abilities to use
                {
                    break;
                }
            }
        }

        selectedPlayer = null;
        TargetEndCurrentTurn();
    }

    [Server]
    public void __ServerStealScoreFromSelectedPlayerAndGiveToPlayerWithLowestScore(PlayerController playerToStealScoreFrom)
    {
        // Steal score from selected player
        int stolenScore = 0;
        if (playerToStealScoreFrom.score < scoreToStolenFromAnotherPlayer)
        {
            stolenScore = playerToStealScoreFrom.score;
            playerToStealScoreFrom.score = 0;
        }
        else
        {
            stolenScore = scoreToStolenFromAnotherPlayer;
            playerToStealScoreFrom.score -= scoreToStolenFromAnotherPlayer;
        }

        // Finds the player with lowest score
        int lowestScore = int.MaxValue;
        PlayerController playerWithLowestScore = null;
        for (int i = 0; i < GI.networkManager.players.Count; i++)
        {
            PlayerController player = GI.networkManager.players[i].identity.GetComponent<PlayerController>();
            if (player != this && player != playerToStealScoreFrom && player.score < lowestScore)
            {
                lowestScore = player.score;
                playerWithLowestScore = player;
            }
        }

        // Gives the stolen score to the player with lowest score
        playerWithLowestScore.score += stolenScore;
    }

    [Server]
    public void __ServerSearchForPlayerWithAvailableCardsAndStealFromHim() // Creativity for names has died a looong time ago
    {
        for (int i = 0; i < GI.networkManager.players.Count; i++)
        {
            NetworkConnectionToClient conn = GI.networkManager.players[i];
            PlayerController player = conn.identity.GetComponent<PlayerController>();
            if (conn != connectionToClient && player.cardsInHand.Count > 0)
            {
                __ServerStealRandomCardFromPlayerHand(player);
            }
        }
    }

    [Server]
    public void __ServerStealRandomCardFromPlayerHand(PlayerController player)
    {
        int randomIndex = Random.Range(0, player.cardsInHand.Count);
        Card card = player.cardsInHand[randomIndex];

        player.ServerRemoveCardFromHand(card.gameObject);
        if (cardsInHand.Count < MAX_CARDS_IN_HAND)
        {
            __SpawnCardInHand(card.type, cardsInHand.Count);
        }
    }

    [TargetRpc]
    public void TargetEndCurrentTurn()
    {
        currentTurn_t = 0f;
        playerHUD.endCurrentTurnButton.interactable = false;
        playerHUD.currentTurnTimeText.enabled = false;
        playerHUD.HideMessage();
    }

    /*  [Command]
      public void CmdTryToRemoveCardFromHand(GameObject go)
      {
          if (actionsRemaining <= 0)
          {
              return;
          }

          Card card = go.GetComponent<Card>();
          if (selectedCards.Contains(card))
          {
              ServerRemoveCardFromHand(go);
              ServerDecreaseActionsRemaining();
          }

      }*/

    [Command]
    public void CmdTryToSelectCard(GameObject go)
    {

        selectedCards.RemoveAll(c => c == null);
        if (GI.cardSystem.isMemorizationPhase)
        {
            //Debug.Log("Aguardando fim da memorização...");
            return;
        }

        Card card = go.GetComponent<Card>();

        // Apply abilities
        switch (currentAbility)
        {
            case Ability_Type.REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS:
                {
                    // Reduce other player card's points
                    for (int i = 0; i < GI.networkManager.players.Count; i++)
                    {
                        // Checks if the selected card came from a player's hand.
                        NetworkConnectionToClient conn = GI.networkManager.players[i];
                        if (conn != connectionToClient && conn.identity.GetComponent<PlayerController>().cardsInHand.Contains(card))
                        {
                            card.improvedPoints -= pointsToChooseToReduce;
                            ServerActivateNextCardAbility();

                            return;
                        }
                    }
                } break;
            case Ability_Type.IMPROVE_ANOTHER_CARD_BY_X_POINTS:
                {
                    if (cardsInHand.Contains(card))
                    {
                        // Improve card's points
                        card.improvedPoints += pointsToChooseToImprove;
                        ServerActivateNextCardAbility();

                        return;
                    }
                } break;
            case Ability_Type.STEAL_ANOTHER_PLAYER_CARD:
                {
                    if (selectedPlayer && selectedPlayer.cardsInHand.Contains(card))
                    {
                        // Remove card from the selected player and spawns it in current player's hand
                        selectedPlayer.ServerRemoveCardFromHand(card.gameObject);
                        if (cardsInHand.Count < MAX_CARDS_IN_HAND)
                        {
                            __SpawnCardInHand(card.type, cardsInHand.Count);
                        }

                        selectedPlayer = null;
                        ServerActivateNextCardAbility();
                        return;
                    }
                } break;
            default: break;
        }

        // Blocks other player's actions while using abilities
        if (currentAbility != Ability_Type.COUNT)
        {
            return;
        }

        // Selects/Deselects the card
        if (cardsInHand.Contains(card))
        {
            if (selectedCards.Contains(card))
            {
                // Deselect
                selectedCards.Remove(card);
                TargetDeselectCard(go);
            }
            else if (selectedCards.Count < 3)
            {
                // Select
                selectedCards.Add(card);
                TargetSelectCard(go);
            }
        }
        else
        {
            if (GI.networkManager.GetCurrentPlayerTurn() != connectionToClient.connectionId)
            {
                return;
            }
            if (selectedCards.Count == 0)
            {
                ServerSpawnCardInHand(card.type, go);
                return;
            }

            if (selectedCards.Count == 1)
            {
                Card handCard = selectedCards[0];

                StartCoroutine(GI.cardSystem.SwapCard(
                    handCard,
                    card,
                    connectionToClient,
                    cardsSpawnPoints,
                    cardsInHand
                ));

                selectedCards.Clear();
                ServerDecreaseActionsRemaining();
            }
        }

    }

    [TargetRpc]
    public void TargetSelectCard(GameObject go)
    {
        go.transform.position += go.transform.forward * 0.1f;
    }

    [TargetRpc]
    public void TargetDeselectCard(GameObject go)
    {
        go.transform.position -= go.transform.forward * 0.1f;
    }

    [Command]
    public void CmdSpawnCardInHand()
    {
        if (!GI.cardSystem.deckManager.HasCards())
        {
            Debug.Log("Deck vazio, não é possível comprar carta para a mão.");
            return;
        }
        Card_Type type = GI.cardSystem.deckManager.DrawCard();
        int spawnIndex = cardsInHand.Count;
        if (spawnIndex >= MAX_CARDS_IN_HAND) return;
        __SpawnCardInHand(type, spawnIndex);
    }

    [Server]
    public void ServerSpawnCardInHand(Card_Type type, GameObject cardToRemoveFromDesk)
    {
        if (GI.networkManager.GetCurrentPlayerTurn() != connectionToClient.connectionId)
        {
            return;
        }

        if (actionsRemaining <= 0)
        {
            return;
        }
        int spawnIndex = cardsInHand.Count;
        if (spawnIndex >= MAX_CARDS_IN_HAND)
        {
            Debug.Assert(false, "Player's hand is already full of cards. Can't add a new one.");
            return;
        }

        __SpawnCardInHand(type, spawnIndex);

        GI.cardSystem.DestroyCard(cardToRemoveFromDesk);
        ServerDecreaseActionsRemaining();
    }

    // Function made to eliminate duplicated code.
    // Those two underlines '__' means "i'm a internal function, but you can call me outside the script if you know what you're doing"
    [Server]
    public void __SpawnCardInHand(Card_Type type, int spawnIndex)
    {
        GameObject go = Instantiate(GI.cardList.GetCardPrefab(type), cardsSpawnPoints[spawnIndex].position,
                                    cardsSpawnPoints[spawnIndex].rotation);
        NetworkServer.Spawn(go, connectionToClient);

        cardsInHand.Add(go.GetComponent<Card>());
        if (!isLocalPlayer) // Prevents the host from spawning the card twice
        {
            TargetSpawnCardInHand(go);
        }
    }

    // 'cardsInHand' list is being updated in clients so we can reorder the cards easily from server and update it on the clients.
    [TargetRpc]
    public void TargetSpawnCardInHand(GameObject go)
    {
        cardsInHand.Add(go.GetComponent<Card>());
    }

    [Server]
    public void ServerRemoveCardFromHand(GameObject go)
    {
        Card card = go.GetComponent<Card>();
        if (!cardsInHand.Contains(card))
        {
            Debug.Assert(false, "Trying to remove a card that is not on player's hand. Maybe you are referencing a card from the desk.");
            return;
        }

        int index = cardsInHand.IndexOf(card);

        selectedCards.Remove(card);
        cardsInHand.Remove(card);
        NetworkServer.Destroy(card.gameObject);

        // Reorder card's position
        for (int i = index; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].transform.position = cardsSpawnPoints[i].position;
            cardsInHand[i].transform.rotation = cardsSpawnPoints[i].rotation;
        }

        if (!isLocalPlayer) // Prevents the host from reordering the cards twice
        {
            TargetRemoveCardFromHand(index);
        }
    }

    [TargetRpc]
    public void TargetRemoveCardFromHand(int index)
    {
        cardsInHand.RemoveAt(index);
        for (int i = index; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].transform.position = cardsSpawnPoints[i].position;
            cardsInHand[i].transform.rotation = cardsSpawnPoints[i].rotation;
        }
    }

    public void UpdateScore(int oldValue, int newValue)
    {
        score = newValue;
        playerHUD.UpdateScore();
    }

    [Server]
    public void ServerStartCurrentTurn(float timer)
    {
        currentTurn_t = timer;
        actionsRemaining = 2;
        TargetStartCurrentTurn();
    }

    [TargetRpc]
    public void TargetStartCurrentTurn()
    {
        playerHUD.endCurrentTurnButton.interactable = true;
        playerHUD.currentTurnTimeText.enabled = true;
    }
    [TargetRpc]
    public void TargetPauseTurn()
    {
        currentTurn_t = 0f;
        playerHUD.currentTurnTimeText.enabled = false;
    }
    [Server]
    public void ServerEnterSpectatorMode()
    {
        spectatorMode = true;
        playerModel.SetActive(false);
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            ServerRemoveCardFromHand(cardsInHand[i].gameObject);
            i--;
        }

        TargetEnterSpectatorMode();
    }

    [TargetRpc]
    public void TargetEnterSpectatorMode()
    {
        playerHUD.HideGameplayHUD();
        playerHUD.ShowSpectatorHUD();
    }

    [Server]
    public void ServerWin()
    {
        gameStopped = true;
        Time.timeScale = 0f;

        // Update Stats here
        TargetWin();
    }

    [TargetRpc]
    public void TargetWin()
    {
        Time.timeScale = 0f;
        playerHUD.ShowWin();
    }

    [Server]
    public void ServerLose()
    {
        // @TODO: ServerPause() function
        gameStopped = true;
        Time.timeScale = 0f;

        // Update stats here
        TargetLose();
    }

    [TargetRpc]
    public void TargetLose()
    {
        Time.timeScale = 0f;
        playerHUD.ShowLose();
    }


    //****************
    [Server]
    void ServerScoreTrio(GameObject c1, GameObject c2, GameObject c3)
    {
        Card card1 = c1.GetComponent<Card>();
        Card card2 = c2.GetComponent<Card>();
        Card card3 = c3.GetComponent<Card>();

        int trioScore = trioSystem.CalculateScore(card1, card2, card3);

        // Aplica bônus de respeito se for trio da mesma família
        SameFamilyRule familyRule = new SameFamilyRule();
        if (familyRule.IsValid(card1, card2, card3))
        {
            Family_Type family = card1.familyType; // todas iguais
            UpdateRespect_Server(family);
            int bonus = GetFamilyBonusScore(family);
            trioScore += bonus;
            Debug.Log($"Bônus de respeito da família {family}: +{bonus}");
        }

        score += trioScore;

        Debug.Log("Score atualizado: " + score);

        GI.cardSystem.deckManager.AddCard(card1.type);
        GI.cardSystem.deckManager.AddCard(card2.type);
        GI.cardSystem.deckManager.AddCard(card3.type);

        ServerRemoveCardFromHand(c1);
        ServerRemoveCardFromHand(c2);
        ServerRemoveCardFromHand(c3);
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].transform.position = cardsSpawnPoints[i].position;
            cardsInHand[i].transform.rotation = cardsSpawnPoints[i].rotation;
        }
        selectedCards.Clear();

        // Apply Abilities
        // @TODO:
        // Client applying abilities on ante round breaks the game.
        abilitiesToApply.Add(card1.abilityType);
        abilitiesToApply.Add(card2.abilityType);
        abilitiesToApply.Add(card3.abilityType);
        ServerActivateNextCardAbility();

        ServerDecreaseActionsRemaining();
    }

    [Server]
    public void ServerActivateNextCardAbility(bool maybeEndCurrentTurn = true)
    {
        activate_ability_start:

        if (abilitiesToApply.Count < 1)
        {
            currentAbility = Ability_Type.COUNT;
            if (maybeEndCurrentTurn)
            {
                ServerMaybeEndCurrentTurn();
            }

            return;
        }

        currentAbility = abilitiesToApply[0];
        abilitiesToApply.RemoveAt(0);

        switch (currentAbility)
        {
            case Ability_Type.IMPROVE_ANOTHER_CARD_BY_X_POINTS:
                {
                    if (cardsInHand.Count > 0)
                    {
                        pointsToChooseToImprove = 5;
                        ServerShowMessageToImproveCard();
                    }
                    else
                    {
                        // Go back to the beginning of the function.
                        goto activate_ability_start;
                    }
                } break;
            case Ability_Type.REDUCE_ANOTHER_PLAYER_CARD_BY_X_POINTS:
                {
                    pointsToChooseToReduce = 5;
                    playerHUD.TargetShowMessage("Choose a other player's card to reduce by 5 points.", 1f);
                } break;
            case Ability_Type.STEAL_ANOTHER_PLAYER_CARD:
                {
                    canSelectOtherPlayer = true;
                    playerHUD.TargetShowMessage("Select a player to steal a card from.", 1f);
                } break;
            case Ability_Type.STEAL_PLAYER_SCORE_AND_GIVE_TO_PLAYER_WITH_LESS_SCORE:
                {
                    canSelectOtherPlayer = true;
                    scoreToStolenFromAnotherPlayer = 5;
                    playerHUD.TargetShowMessage("Select a player to steal score from.", 1f);
                } break;
            default: break;
        }
    }

    public void ServerShowMessageToImproveCard()
    {
        playerHUD.TargetShowMessage("Choose a card to improve by 5 points.", 2f);
    }

    [Command]
    void CmdCheckForTrio()
    {
        if (GI.networkManager.GetCurrentPlayerTurn() != connectionToClient.connectionId)
            return;

        // Using a card ability
        if (currentAbility != Ability_Type.COUNT)
        {
            return;
        }

        selectedCards.RemoveAll(c => c == null || !cardsInHand.Contains(c));

        if (selectedCards.Count != 3)
        {
            return;

        }
        if (actionsRemaining <= 0)
        {
            return;
        }
        if (trioSystem.TryFindTrio(selectedCards, out Card a, out Card b, out Card c))
        {
            Debug.Log("TRIO!");
            ServerScoreTrio(a.gameObject, b.gameObject, c.gameObject);
        }
    }

    [Server]
    public void ServerDecreaseActionsRemaining()
    {
        actionsRemaining--;
        ServerMaybeEndCurrentTurn();
    }

    [Server]
    public void ServerMaybeEndCurrentTurn()
    {
        if (actionsRemaining <= 0 && currentAbility == Ability_Type.COUNT)
        {
            ServerEndCurrentTurn();
        }
    }

    //Retorna o valor real de respeito (0.0 a 6.0) para uma família.
    public float GetRespect(Family_Type family)
    {
        return GetRespectScaled(family) / 100f;
    }

    int GetRespectScaled(Family_Type family)
    {
        return family switch
        {
            Family_Type.FAMILY_1 => respectF1,
            Family_Type.FAMILY_2 => respectF2,
            Family_Type.FAMILY_3 => respectF3,
            Family_Type.FAMILY_4 => respectF4,
            _ => 0
        };
    }

    void SetRespectScaled(Family_Type family, int value)
    {
        value = Mathf.Max(0, value); // nunca negativo
        switch (family)
        {
            case Family_Type.FAMILY_1: respectF1 = value; break;
            case Family_Type.FAMILY_2: respectF2 = value; break;
            case Family_Type.FAMILY_3: respectF3 = value; break;
            case Family_Type.FAMILY_4: respectF4 = value; break;
        }
    }

    // Nível de respeito (0 a 6) – parte inteira do valor real.
    public int GetRespectLevel(Family_Type family)
    {
        return Mathf.Clamp(Mathf.FloorToInt(GetRespect(family)), 0, 6);
    }

    // Bônus de pontuação para trios dessa família (exemplo: +1 a +5).
    public int GetFamilyBonusScore(Family_Type family)
    {
        int level = GetRespectLevel(family);
        if (level < 2) return 0;
        // Exemplo: nível 2 = +1, 3 = +2, 4 = +3, 5 = +4, 6 = +5
        return level - 1;
    }

    [Server]
    void UpdateRespect_Server(Family_Type trioFamily)
    {
        const int GAIN = 50; // +0.5

        // ---- REGRA 1: perda de 0.25 nas três outras ----
        /*
        foreach (Family_Type f in System.Enum.GetValues(typeof(Family_Type)))
        {
            if (f == trioFamily) continue;
            int current = GetRespectScaled(f);
            SetRespectScaled(f, current - 25);
        }
        */

        // ---- REGRA 2: perda de 0.25 só na família com maior respeito (entre as outras) ----
        Family_Type? highestOther = null;
        int highestValue = -1;
        foreach (Family_Type f in System.Enum.GetValues(typeof(Family_Type)))
        {
            if (f == trioFamily) continue;
            int val = GetRespectScaled(f);
            if (val > highestValue)
            {
                highestValue = val;
                highestOther = f;
            }
        }
        if (highestOther.HasValue && highestValue > 0)
        {
            int newVal = Mathf.Max(0, highestValue - 25); // -0.25
            SetRespectScaled(highestOther.Value, newVal);
        }

        // Ganho na família do trio
        int currentTrio = GetRespectScaled(trioFamily);
        SetRespectScaled(trioFamily, currentTrio + GAIN);
    }


    void OnRespectF1Changed(int oldValue, int newValue)
    {
        playerHUD.UpdateRespectUI(Family_Type.FAMILY_1, newValue);
    }
    void OnRespectF2Changed(int oldValue, int newValue)
    {
        playerHUD.UpdateRespectUI(Family_Type.FAMILY_2, newValue);
    }
    void OnRespectF3Changed(int oldValue, int newValue)
    {
        playerHUD.UpdateRespectUI(Family_Type.FAMILY_3, newValue);
    }
    void OnRespectF4Changed(int oldValue, int newValue)
    {
        playerHUD.UpdateRespectUI(Family_Type.FAMILY_4, newValue);
    }


    [Command]
    public void CmdSetInitialRespect(Family_Type family)
    {
        int current = GetRespectScaled(family);
        SetRespectScaled(family, current + 100); // +1.0
    }
}
