using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class LobbyUI : NetworkBehaviour
{
    public GameObject lobbyPanel;
    public TextMeshProUGUI lobbyPlayersText;
    public TextMeshProUGUI textButton;
    public Button readyButton;

    private LobbyManager lobbyManager;
    private PlayerController localPlayerController;

    void Start()
    {
        lobbyManager = GI.networkManager.GetComponent<LobbyManager>();
        localPlayerController = NetworkClient.localPlayer.GetComponent<PlayerController>();

        if (isLocalPlayer)
        {
            readyButton.onClick.AddListener(OnReadyClick);

        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        lobbyManager = GI.networkManager.GetComponent<LobbyManager>();

        lobbyManager.players.OnAdd += OnLobbyAdd;
        lobbyManager.players.OnSet += OnLobbyUpdated;
        lobbyManager.players.OnRemove += OnLobbyUpdated;

        RefreshLobby();
    }

    void OnLobbyAdd(int index)
    {
        RefreshLobby();
    }

    void OnLobbyUpdated(int index, LobbyPlayerData oldValue)
    {
        RefreshLobby();
    }

    void RefreshLobby()
    {
        string list = "";

        foreach (var p in lobbyManager.players)
        {
            string ready = p.isReady ? "Ok" : "X";
            list += $"{p.playerName} [{ready}]\n";
        }

        lobbyPlayersText.text = list;
    }


    void OnReadyClick()
    {
        if (localPlayerController == null) return;

        bool current = IsLocalReady();
        bool newState = !current;
        localPlayerController.CmdSetReady(newState);

        if (textButton != null)
            textButton.text = newState ? "UNREADY" : "READY";
    }

    bool IsLocalReady()
    {
        if (lobbyManager == null) return false;
        foreach (var p in lobbyManager.players)
        {
            if (p.netId == netId && p.isReady) return true;
        }
        return false;
    }
}