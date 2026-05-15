using UnityEngine;
using Mirror;

[System.Serializable]
public struct LobbyPlayerData
{
    public uint netId;
    public string playerName;
    public bool isReady;
    public int characterId;
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
    }

    [Server]
    public void SetCharacter(uint netId, int characterId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].netId == netId)
            {
                LobbyPlayerData data = players[i];
                data.characterId = characterId;
                players[i] = data;
                break;
            }
        }
    }


}