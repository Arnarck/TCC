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
    [SyncVar(hook = nameof(UpdateCurrentRound))] public int currentRound;
    public bool localPlayerSpawned; // We need this because since player spawns in the network, it can spawns at any time
    public List<Card> cardsInDesk;

    [Header("Deck")]
    public DeckManager deckManager;

    private Card[] deskSlots; // novo


    private void Awake()
    {
        GI.cardSystem = this;
        GI.cardList = cardList;
        deskSlots = new Card[MAX_CARDS_IN_DESK];
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
    public void SpawnCard(Card_Type type, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_CARDS_IN_DESK)
            return;
        if (deskSlots[slotIndex] != null)
        {
            Debug.LogWarning($"Slot {slotIndex} já ocupado.");
            return;
        }

        Transform spawnPoint = cardsSpawnPoints[slotIndex];
        GameObject go = Instantiate(GI.cardList.GetCardPrefab(type), spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(go);
        Card card = go.GetComponent<Card>();
        card.type = type;
        switch (type)
        {
            case Card_Type.IMPROVE: card.points = 10; break;
            case Card_Type.CARD_2: card.points = 20; break;
            case Card_Type.CARD_3: card.points = 30; break;
            case Card_Type.CARD_4: card.points = 20; break;
            case Card_Type.CARD_5: card.points = 30; break;
            case Card_Type.CARD_6: card.points = 10; break;
            case Card_Type.CARD_7: card.points = 20; break;
        }
        deskSlots[slotIndex] = card;
    }
    [Server]
    public void SpawnCardFromDeck(int slotIndex)
    {
        if (!deckManager.HasCards())
        {
            Debug.Log("Deck vazio, não é possível spawnar carta na mesa.");
            return;
        }
        Card_Type type = deckManager.DrawCard();
        SpawnCard(type, slotIndex);
    }
    public IEnumerator RunMemorizationPhase()
    {
        isMemorizationPhase = true;
        for (int i = 0; i < GI.networkManager.players.Count; i++)
        {
            // Shows 'memorization phase panel' for all players
            PlayerController player = GI.networkManager.players[i].identity.GetComponent<PlayerController>();
            player.playerHUD.TargetShowMemorizationPhasePanel();
            player.playerHUD.TargetHideMainHUD();
            player.isChoosingCards = true;
        }

        // Revela todas as cartas atualmente na mesa
        foreach (Card card in deskSlots)
        {
            if (card != null)
                card.isRevealed = true;
        }

        yield return new WaitForSeconds(memorizeTime);

        // Esconde todas as cartas
        foreach (Card card in deskSlots)
        {
            if (card != null)
                card.isRevealed = false;
        }

        isMemorizationPhase = false;
        for (int i = 0; i < GI.networkManager.players.Count; i++)
        {
            // Hides 'memorization phase panel' for all players
            PlayerController player = GI.networkManager.players[i].identity.GetComponent<PlayerController>();
            player.playerHUD.TargetHideMemorizationPhasePanel();
            player.playerHUD.TargetShowMainHUD();
            player.isChoosingCards = false;
        }
    }

    [Server]
    public void StartMemorizationPhase()
    {
        StartCoroutine(RunMemorizationPhase());
    }

    [Server]
    public void DestroyCard(GameObject cardGO)
    {
        Card card = cardGO.GetComponent<Card>();
        for (int i = 0; i < deskSlots.Length; i++)
        {
            if (deskSlots[i] == card)
            {
                deskSlots[i] = null;
                break;
            }
        }
        NetworkServer.Destroy(cardGO);
    }
    [Server]
    public void RefillTableFromDeck()
    {
        for (int i = 0; i < deskSlots.Length; i++)
        {
            if (deskSlots[i] == null && deckManager.HasCards())
            {
                SpawnCardFromDeck(i);
            }
        }
        Debug.Log($"Mesa preenchida. Slots vazios restantes: {GetEmptySlotCount()}");
    }

    [Server]
    public void FillInitialTable()
    {
        for (int i = 0; i < deskSlots.Length; i++)
        {
            if (deskSlots[i] == null && deckManager.HasCards())
            {
                SpawnCardFromDeck(i);
            }
        }
    }

    public List<Card> GetCardsInDesk()
    {
        List<Card> list = new List<Card>();
        foreach (var c in deskSlots)
            if (c != null) list.Add(c);
        return list;
    }

    private int GetEmptySlotCount()
    {
        int count = 0;
        foreach (var c in deskSlots) if (c == null) count++;
        return count;
    }
    [Server]
    public int GetSlotIndex(Card card)
    {
        for (int i = 0; i < deskSlots.Length; i++)
        {
            if (deskSlots[i] == card)
                return i;
        }
        return -1;
    }
    [Server]
    public IEnumerator SwapCard(
    Card handCard,
    Card deskCard,
    NetworkConnectionToClient conn,
    Transform[] handSlots,
    List<Card> handList)
    {
        if (GI.networkManager.GetCurrentPlayerTurn() != conn.connectionId)
        {
            yield break;
        }

        int deskIndex = GetSlotIndex(deskCard);
        if (deskIndex == -1) yield break;

        int handIndex = handList.IndexOf(handCard);
        if (handIndex == -1) yield break;

        deskSlots[deskIndex] = null;
        handList.RemoveAt(handIndex);

        NetworkServer.Destroy(deskCard.gameObject);
        NetworkServer.Destroy(handCard.gameObject);


        GameObject newHandGO = Instantiate(
            GI.cardList.GetCardPrefab(deskCard.type),
            handSlots[handIndex].position,
            handSlots[handIndex].rotation
        );

        NetworkServer.Spawn(newHandGO, conn);

        Card newHandCard = newHandGO.GetComponent<Card>();


        handList.Insert(handIndex, newHandCard);


        for (int i = 0; i < handList.Count; i++)
        {
            handList[i].transform.position = handSlots[i].position;
            handList[i].transform.rotation = handSlots[i].rotation;
        }


        Transform spawnPoint = cardsSpawnPoints[deskIndex];

        GameObject newDeskGO = Instantiate(
            GI.cardList.GetCardPrefab(handCard.type),
            spawnPoint.position,
            spawnPoint.rotation
        );

        NetworkServer.Spawn(newDeskGO);

        Card newDeskCard = newDeskGO.GetComponent<Card>();
        deskSlots[deskIndex] = newDeskCard;

        newDeskCard.isRevealed = true;

        yield return new WaitForSeconds(5f);

        if (newDeskCard != null)
            newDeskCard.isRevealed = false;
    }
}