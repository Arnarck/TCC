using UnityEngine;
using Mirror;

[System.Serializable]
public struct LobbyPlayerData
{
    public uint netId;
    public string playerName;
    public bool isReady;
}

public class LobbyManager : NetworkBehaviour
{
    public readonly SyncList<LobbyPlayerData> players = new SyncList<LobbyPlayerData>();
    private CardNetworkManager networkManager;

    private void Awake()
    {
        networkManager = GetComponent<CardNetworkManager>();
    }

    [Server]
    public void AddPlayer(NetworkConnectionToClient conn, string playerName)
    {
        players.Add(new LobbyPlayerData
        {
            netId = conn.identity.netId,
            playerName = playerName,
            isReady = false
        });
    }

    [Server]
    public void RemovePlayer(uint netId)
    {
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i].netId == netId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    [Server]
    public void SetReady(uint netId, bool ready)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].netId == netId)
            {
                LobbyPlayerData data = players[i];
                data.isReady = ready;
                players[i] = data;
                break;
            }
        }
       // CheckReady();
    }

   [Server]
public void CheckReady()
{
    if (networkManager.gameStarted) return;

    // Mínimo de 2 jogadores para começar
    if (players.Count < 2) return;

    // Todos os jogadores no lobby precisam estar prontos
    foreach (var p in players)
        if (!p.isReady) return;

    // Se chegou aqui, inicia a partida
    networkManager.StartGameFromLobby();
}
}