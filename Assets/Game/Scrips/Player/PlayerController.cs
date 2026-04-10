using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public PlayerHUD playerHUD;
    public Transform cameraPointWhenChoosingCards;
    public Camera playerCamera;
    public GameObject playerModel;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    [SyncVar(hook = nameof(UpdateScore))] public int score;
    [SyncVar] public float currentTurn_t;
    [SyncVar] public bool spectatorMode;
    [SyncVar] public bool gameStopped;
    public bool canCollectCardThisTurn;
    public bool isChoosingCards;
    public Vector3 cameraStartPosition;
    public Quaternion cameraStartRotation;
    public List<Card> selectedCards;
    public List<Card> cardsInHand;
    private TrioSystem trioSystem = new TrioSystem();


    private void Start()
    {
        if (isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
            cameraStartPosition = playerCamera.transform.position;
            cameraStartRotation = playerCamera.transform.rotation;

            Quaternion rotation = transform.rotation;
            rotation.x = 0f;
            rotation.z = 0f;
            GI.cardSystem.transform.rotation = rotation;
            GI.cardSystem.localPlayerSpawned = true;

            playerHUD.UpdateScore();


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
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
                    {
                        Card card = hit.collider.gameObject.GetComponent<Card>();
                        CmdTryToSelectCard(card.gameObject);
                    }
                }

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
                    if (isChoosingCards)
                    {
                        playerCamera.transform.position = cameraPointWhenChoosingCards.position;
                        playerCamera.transform.rotation = cameraPointWhenChoosingCards.rotation;
                    }
                    else
                    {
                        playerCamera.transform.position = cameraStartPosition;
                        playerCamera.transform.rotation = cameraStartRotation;
                    }
                }
            }

            // Update current turn timer
            if (currentTurn_t > 0f)
            {
                currentTurn_t -= Time.deltaTime;
                if (currentTurn_t <= 0f && isServer) // Only the server can call this function because the client can cheat the timer
                {
                    CmdEndCurrentTurn();
                }

                // Update time in the client
                if (isLocalPlayer)
                {
                    playerHUD.UpdateCurrentTurnTime();
                }
            }
        }
    }

    [Command]
    public void CmdEndCurrentTurn()
    {
        currentTurn_t = 0f;
        GI.networkManager.UpdatePlayerTurn();

        TargetEndCurrentTurn();
    }

    [TargetRpc]
    public void TargetEndCurrentTurn()
    {
        currentTurn_t = 0f;
        playerHUD.endCurrentTurnButton.interactable = false;
        playerHUD.currentTurnTimeText.enabled = false;
    }

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
        // Prevents from stolling a card from other player's hand (although it's a good idea)
        for (int i = 0; i < GI.networkManager.players.Count; i++)
        {
            // @TODO: Cache 'PlayerController' in network manager?
            NetworkConnectionToClient conn = GI.networkManager.players[i];
            if (conn != connectionToClient && conn.identity.GetComponent<PlayerController>().cardsInHand.Contains(card))
            {
                return;
            }
        }

        // Selects/Deselects the card
        if (cardsInHand.Contains(card))
        {
            if (selectedCards.Contains(card))
            {
                selectedCards.Remove(card);
                TargetDeselectCard(go);
            }
            else
            {
                selectedCards.Add(card);
                TargetSelectCard(go);
            }
        }
        else
        {
            // Selects a card from the desk and spawns it
            SpawnCardInHand(card.type, go);
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
    public void SpawnCardInHand(Card_Type type, GameObject cardToRemoveFromDesk)
    {
        if (GI.networkManager.GetCurrentPlayerTurn() != connectionToClient.connectionId)
        {
            return;
        }

        if (!canCollectCardThisTurn)
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
        canCollectCardThisTurn = false;
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

        cardsInHand.Remove(card);
        NetworkServer.Destroy(card.gameObject);

        // Reorder card's position
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
        canCollectCardThisTurn = true;
        currentTurn_t = timer;

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
    void ServerScoreTrio(GameObject c1, GameObject c2, GameObject c3, int clientScore)
    {
        Card card1 = c1.GetComponent<Card>();
        Card card2 = c2.GetComponent<Card>();
        Card card3 = c3.GetComponent<Card>();

        int serverScore = trioSystem.CalculateScore(card1, card2, card3);
        score += serverScore;
        Debug.Log("Score do trio (server): " + serverScore);

        GI.cardSystem.deckManager.AddCard(card1.type);
        GI.cardSystem.deckManager.AddCard(card2.type);
        GI.cardSystem.deckManager.AddCard(card3.type);

        ServerRemoveCardFromHand(c1);
        ServerRemoveCardFromHand(c2);
        ServerRemoveCardFromHand(c3);

        selectedCards.Clear();
    }

    [Command]
    void CmdCheckForTrio()
    {
        if (GI.networkManager.GetCurrentPlayerTurn() != connectionToClient.connectionId)
        {
            return;
        }

        if (selectedCards.Count != 3)
        {
            return;
        }

        if (trioSystem.TryFindTrio(selectedCards, out Card a, out Card b, out Card c))
        {
            Debug.Log("TRIO!");

            int score = trioSystem.CalculateScore(a, b, c);

            ServerScoreTrio(a.gameObject, b.gameObject, c.gameObject, score);
        }
    }
}
