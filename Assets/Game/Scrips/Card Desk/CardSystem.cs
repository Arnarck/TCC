using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

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
    [SyncVar]
    public bool isMemorizationPhase = true;
    public float memorizeTime = 10f;

    [Header("INTERNAL")]
    [SyncVar(hook = nameof(UpdateCurrentRound))]public int currentRound;
    public bool localPlayerSpawned; // We need this because since player spawns in the network, it can spawns at any time
    public List<Card> cardsInDesk;


    private void Awake()
    {
        GI.cardSystem = this;
        GI.cardList = cardList;
    }

    [Server]
    public void ServerUpdateCurrentRound(int value)
    {
        currentRound = value;
    }

    public void UpdateCurrentRound(int oldValue, int newValue)
    {
        currentRound = newValue;
        GI.playerHUD.UpdateCurrentRound(currentRound);
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
        Card card = go.GetComponent<Card>();
        card.type = type;
        switch (type)
        {
            case Card_Type.CARD_1: card.points = 10; break;
            case Card_Type.CARD_2: card.points = 20; break;
            case Card_Type.CARD_3: card.points = 30; break;
            case Card_Type.CARD_4: card.points = 20; break;
            case Card_Type.CARD_5: card.points = 30; break;
        }

        cardsInDesk.Add(card);

    }
    [Server]
    public void StartMemorizationPhase()
    {
        StartCoroutine(MemorizationPhase());
    }

    IEnumerator MemorizationPhase()
    {
        foreach (Card card in cardsInDesk)
        {
            card.isRevealed = true;
        }

        yield return new WaitForSeconds(memorizeTime);

        foreach (Card card in cardsInDesk)
        {
            card.isRevealed = false;
        }

        isMemorizationPhase = false;
    }
    [Server]
    public void DestroyCard(GameObject card)
    {
        cardsInDesk.Remove(card.GetComponent<Card>());
        NetworkServer.Destroy(card.gameObject);
    }
}