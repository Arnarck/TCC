using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public GameObject cardPrefab;

    public Camera playerCamera;
    public Transform[] cardsSpawnPoints;

    [Header("INTERNAL")]
    public CardSystem cardSystem;

    private void Start()
    {
        // @TODO:
        // Get card from the list instead of spawning directly from a prefab (pooling?)
        // Some way to identify cards (an enum? Use scriptable objects?)
        // Make a base for spawn card in desk (maybe a function that checks if the space is available)
        // Add a list and make the cards be spawned from there, instead of a prefab reference in NetworkManager.
        // Add turn system.
        if (isLocalPlayer)
        {
            playerCamera.gameObject.SetActive(true);
            cardSystem = FindAnyObjectByType<CardSystem>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 6))
            {
                SpawnCardInHand();
                GI.cardSystem.DestroyCard(hit.collider.gameObject);
            }
        }
    }

    [Command]
    public void SpawnCardInHand()
    {
        GameObject card = Instantiate(cardPrefab, cardsSpawnPoints[0].position, cardsSpawnPoints[0].rotation);
        NetworkServer.Spawn(card, connectionToClient);
    }
}
