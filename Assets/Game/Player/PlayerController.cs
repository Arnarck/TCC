using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    public const int MAX_CARDS_IN_HAND = 5;

    public GameObject cardPrefab;

    public Camera playerCamera;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    public List<Card> cardsInHand;

    private void Start()
    {
        // @TODO:
        // Make a base for spawn card in desk (maybe a function that checks if the space is available)
        // Add turn system.
        if (isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (isLocalPlayer && Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
            {
                SpawnCardInHand(hit.collider.GetComponent<Card>().type, hit.collider.gameObject);
            }
        }
    }

    [Command]
    public void SpawnCardInHand(Card_Type type, GameObject cardToRemoveFromDesk)
    {
        // @TODO:
        // Update cards visual when a card is discarded
        // Remove card from last position if someone try to spawn more than 5 cards.
        GI.cardSystem.DestroyCard(cardToRemoveFromDesk);

        int spawnIndex = cardsInHand.Count;
        if (spawnIndex >= MAX_CARDS_IN_HAND)
        {
            Debug.Assert(false, "Player's hand is already full of cards. Can't add a new one. The last one will be overwritten.");
            spawnIndex = MAX_CARDS_IN_HAND - 1;
        }

        GameObject go = Instantiate(GI.cardList.GetCardPrefab(type), cardsSpawnPoints[spawnIndex].position, 
                                    cardsSpawnPoints[spawnIndex].rotation);
        NetworkServer.Spawn(go, connectionToClient);

        cardsInHand.Add(go.GetComponent<Card>());
    }
}
