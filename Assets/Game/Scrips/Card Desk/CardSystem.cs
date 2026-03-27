using UnityEngine;
using Mirror;
using System.Collections.Generic;

// @Note: 'Command' attributes will not work here. Since the server spawns this object, it has no client to address authority to it.
// The objects needs authority (an client as a owner) to make 'Command' attributes work, so the server can know at which client the
// object belongs to.
// That's why 'SpawnCard' and 'DestroyCard' are only called inside 'Command' attributes or inside the NetworkManager. Only the server
// can call this object's methods.
public class CardSystem : NetworkBehaviour
{
    public const int MAX_CARDS_IN_DESK = 16;

    public Rigidbody _rigidbody;
    public CardList cardList; // This reference is only to set it to GI.cardList, since 'Awake' apparently is not called in ScriptableObjects
    public GameObject cardPrefab;
    public Vector3 collisionHalfSize;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    public List<Card> cardsInDesk;

    private void Start()
    {
        // Prevents rigidbody from sleeping
        // We can make it sleep from times to times later if this become a optimization problem
        _rigidbody.sleepThreshold = 0f;
    }

    private void Awake()
    {
        GI.cardSystem = this;
        GI.cardList = cardList;
    }

    [Server]
    public void SpawnCard(Card_Type type)
    {
        int spawnIndex = cardsInDesk.Count;
        if (spawnIndex >= MAX_CARDS_IN_DESK)
        {
            Debug.Assert(false, "The desk is already full of cards. Can't add a new one.");
            return;
        }

        Transform spawnPoint = cardsSpawnPoints[spawnIndex];
        GameObject go = Instantiate(GI.cardList.GetCardPrefab(type), spawnPoint.position, spawnPoint.rotation);

        NetworkServer.Spawn(go, connectionToClient);
        cardsInDesk.Add(go.GetComponent<Card>());
    }

    [Server]
    public void DestroyCard(GameObject card)
    {
        cardsInDesk.Remove(card.GetComponent<Card>());
        NetworkServer.Destroy(card.gameObject);
    }
}