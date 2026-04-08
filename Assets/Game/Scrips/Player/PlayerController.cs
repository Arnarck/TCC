using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public PlayerHUD playerHUD;
    public Transform cameraPointWhenChoosingCards;
    public Camera playerCamera;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    [SyncVar(hook = nameof(UpdateScore))]public int score;
    public bool isChoosingCards;
    public Vector3 cameraStartPosition;
    public Quaternion cameraStartRotation;
    public Card selectedCard; // @TODO: This should have a 'SyncVar' attribute, but we'll add it when needed.
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
        }
        else
        {
            playerHUD.Hide();
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {

            if (Input.GetMouseButtonDown(0))
            {
                // Select cards
                if (GI.cardSystem.isMemorizationPhase)
                {
                    //Debug.Log("Aguardando fim da memorização...");
                    return;
                }
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
                {
                    Card card = hit.collider.gameObject.GetComponent<Card>();
                    if (cardsInHand.Contains(card))
                    {
                        // Selects a card from the hand
                        selectedCard = card;
                    }
                    else
                    {
                        // Selects a card from the desk and spawns it
                        CmdSpawnCardInHand(card.type, hit.collider.gameObject);
                    }
                }
                //*******
                if (cardsInHand.Count >= 3)
                {
                    CheckForTrio();
                }
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
    }

    [Command]
    public void CmdSpawnCardInHand(Card_Type type, GameObject cardToRemoveFromDesk)
    {
        if (GI.networkManager.GetCurrentPlayerTurn() != connectionToClient.connectionId)
        {
            return;
        }

        int spawnIndex = cardsInHand.Count;
        if (spawnIndex >= MAX_CARDS_IN_HAND)
        {
            Debug.Assert(false, "Player's hand is already full of cards. Can't add a new one.");
            return;
        }

        GI.cardSystem.DestroyCard(cardToRemoveFromDesk);

        GameObject go = Instantiate(GI.cardList.GetCardPrefab(type), cardsSpawnPoints[spawnIndex].position,
                                    cardsSpawnPoints[spawnIndex].rotation);
        NetworkServer.Spawn(go, connectionToClient);

        cardsInHand.Add(go.GetComponent<Card>());
        GI.networkManager.UpdatePlayerTurn();
    }

    [Command]
    public void CmdRemoveCardFromHand(GameObject go)
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

    [Command]
    public void CmdAddScore(int value)
    {
        score += value;
    }

    public void UpdateScore(int oldValue, int newValue)
    {
        score = newValue;
        playerHUD.UpdateScore();
    }


    //****************
    [Command]
    void CmdScoreTrio(GameObject c1, GameObject c2, GameObject c3, int clientScore)
    {
        Card card1 = c1.GetComponent<Card>();
        Card card2 = c2.GetComponent<Card>();
        Card card3 = c3.GetComponent<Card>();

        int serverScore = trioSystem.CalculateScore(card1, card2, card3);
        CmdAddScore(serverScore);

        Debug.Log("Score do trio (server): " + serverScore);

        CmdRemoveCardFromHand(c1);
        CmdRemoveCardFromHand(c2);
        CmdRemoveCardFromHand(c3);
    }
   void CheckForTrio()
    {
        if (trioSystem.TryFindTrio(cardsInHand, out Card a, out Card b, out Card c))
        {
            Debug.Log("TRIO!");

            int score = trioSystem.CalculateScore(a, b, c);

            CmdScoreTrio(a.gameObject, b.gameObject, c.gameObject, score);
        }
    }


}
