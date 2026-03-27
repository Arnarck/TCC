using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public Transform cameraPointWhenChoosingCards;
    public Camera playerCamera;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    public bool isChoosingCards;
    public Vector3 cameraStartPosition;
    public Quaternion cameraStartRotation;
    public Card selectedCard; // @TODO: This should have a 'SyncVar' attribute, but we'll add it when needed.
    public List<Card> cardsInHand;

    private void Start()
    {
        if (isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
            cameraStartPosition = playerCamera.transform.position;
            cameraStartRotation = playerCamera.transform.rotation;
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            // Select cards
            if (Input.GetMouseButtonDown(0))
            {
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
}
