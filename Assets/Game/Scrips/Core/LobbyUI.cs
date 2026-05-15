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
        // Obtém referências
        lobbyManager = GI.networkManager.lobbyManager;
        localPlayerController = NetworkClient.localPlayer.GetComponent<PlayerController>();

        if (isLocalPlayer)
        {
            readyButton.onClick.AddListener(OnReadyClick);
           // lobbyPanel.SetActive(false); // começa escondido
         
        }
    }
public override void OnStartClient()
{
    base.OnStartClient();

     lobbyManager = GI.networkManager.lobbyManager;

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
 /*   void Update()
    {
        if (!isLocalPlayer || lobbyManager == null) return;

        // Monta lista de jogadores no lobby
        string list = "";
        foreach (var p in lobbyManager.players)
        {
            string ready = p.isReady ? "✔️" : "❌";
            list += $"{p.playerName} [{ready}]\n";
        }
        lobbyPlayersText.text = list;
    }*/

 
    void OnReadyClick()
    {
        if (localPlayerController == null) return;

        bool current = IsLocalReady();
        bool newState = !current;
        localPlayerController.CmdSetReady(newState);

        // Texto corrigido: mostra o estado oposto ao que acabou de ficar
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