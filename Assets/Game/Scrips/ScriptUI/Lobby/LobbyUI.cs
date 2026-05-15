using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyUI : NetworkBehaviour
{public GameObject lobbyPanel;
    public TextMeshProUGUI lobbyPlayersText;
    public TextMeshProUGUI textButton;
    public Button readyButton;

    public Button turtleButton;
    public Button dollButton;
    public Button catButton;
    public Button goldilocksButton;

    private LobbyManager lobbyManager;
    private PlayerController localPlayerController;

    void Start()
    {
        if (isLocalPlayer)
        {
            localPlayerController = NetworkClient.localPlayer?.GetComponent<PlayerController>();
            if (localPlayerController != null)
            {
                localPlayerController.OnLocalCharacterChanged += HandleLocalCharacterChanged;
                readyButton.onClick.AddListener(OnReadyClick);
            }
            else
            {
                Debug.LogError("LobbyUI: localPlayerController não encontrado no Start.");
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        lobbyManager = GI.networkManager?.lobbyManager;
        if (lobbyManager != null)
        {
            lobbyManager.players.OnAdd += OnLobbyAdd;
            lobbyManager.players.OnSet += OnLobbyUpdated;
            lobbyManager.players.OnRemove += OnLobbyUpdated;
        }
        RefreshLobby();
    }

    void OnLobbyAdd(int index) => RefreshLobby();
    void OnLobbyUpdated(int index, LobbyPlayerData oldValue) => RefreshLobby();

    void RefreshLobby()
    {
        string list = "";
        foreach (var p in lobbyManager.players)
        {
            string ready = p.isReady ? "Ok" : "X";
            string charName = ((CharacterType)p.characterId).ToString();
            list += $"{p.playerName} ({charName}) [{ready}]\n";
        }
        lobbyPlayersText.text = list;

        // Bloqueia personagens já escolhidos por outros
        if (isLocalPlayer && localPlayerController != null)
        {
            HashSet<CharacterType> taken = new HashSet<CharacterType>();
            foreach (var p in lobbyManager.players)
            {
                if (p.netId != netId && p.characterId != 0)
                    taken.Add((CharacterType)p.characterId);
            }
            turtleButton.interactable = !taken.Contains(CharacterType.TURTLE);
            dollButton.interactable = !taken.Contains(CharacterType.DOLL);
            catButton.interactable = !taken.Contains(CharacterType.CAT);
            goldilocksButton.interactable = !taken.Contains(CharacterType.GOLDILOCKS);
        }
    }

    void HandleLocalCharacterChanged(CharacterType newChar)
    {
        // Reset cores
        turtleButton.image.color = Color.white;
        dollButton.image.color = Color.white;
        catButton.image.color = Color.white;
        goldilocksButton.image.color = Color.white;

        // Destaca selecionado
        Color highlight = Color.green;
        switch (newChar)
        {
            case CharacterType.TURTLE: turtleButton.image.color = highlight; break;
            case CharacterType.DOLL: dollButton.image.color = highlight; break;
            case CharacterType.CAT: catButton.image.color = highlight; break;
            case CharacterType.GOLDILOCKS: goldilocksButton.image.color = highlight; break;
        }

        readyButton.interactable = (newChar != CharacterType.NONE);
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


    
    public void SelectTURTLE()
{
    localPlayerController.CmdSelectCharacter(CharacterType.TURTLE);
}
 public void SelectDOLL()
{
    localPlayerController.CmdSelectCharacter(CharacterType.DOLL);
}
 public void SelectCAT()
{
    localPlayerController.CmdSelectCharacter(CharacterType.CAT);
}
 public void SelectGOLDILOCKS()
{
    localPlayerController.CmdSelectCharacter(CharacterType.GOLDILOCKS);
}


}