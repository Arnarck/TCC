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

    private CharacterType currentSelectedCharacter; 

    void Start()
    {
        if (isLocalPlayer)
        {
            localPlayerController = NetworkClient.localPlayer?.GetComponent<PlayerController>();
            if (localPlayerController != null)
            {
                // Inicializa o estado da UI com o personagem atual (provavelmente NONE)
                HandleLocalCharacterChanged(localPlayerController.selectedCharacter);
                localPlayerController.OnLocalCharacterChanged += HandleLocalCharacterChanged;
                readyButton.onClick.AddListener(OnReadyClick);
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
        string charName = ((CharacterType)p.characterId) == CharacterType.NONE ? "..." : ((CharacterType)p.characterId).ToString();
        list += $"{p.playerName} ({charName}) [{ready}]\n";
    }
    lobbyPlayersText.text = list;

    // Bloqueio de personagens já escolhidos
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
        currentSelectedCharacter = newChar;

        turtleButton.image.color = Color.white;
        dollButton.image.color = Color.white;
        catButton.image.color = Color.white;
        goldilocksButton.image.color = Color.white;

        if (newChar != CharacterType.NONE)
        {
            Color highlight = Color.green;
            switch (newChar)
            {
                case CharacterType.TURTLE: turtleButton.image.color = highlight; break;
                case CharacterType.DOLL: dollButton.image.color = highlight; break;
                case CharacterType.CAT: catButton.image.color = highlight; break;
                case CharacterType.GOLDILOCKS: goldilocksButton.image.color = highlight; break;
            }
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
        if (localPlayerController == null) return;
        if (currentSelectedCharacter == CharacterType.TURTLE)
            localPlayerController.CmdSelectCharacter(CharacterType.NONE);
        else
            localPlayerController.CmdSelectCharacter(CharacterType.TURTLE);
    }

    public void SelectDOLL()
    {
        if (localPlayerController == null) return;
        if (currentSelectedCharacter == CharacterType.DOLL)
            localPlayerController.CmdSelectCharacter(CharacterType.NONE);
        else
            localPlayerController.CmdSelectCharacter(CharacterType.DOLL);
    }

    public void SelectCAT()
    {
        if (localPlayerController == null) return;
        if (currentSelectedCharacter == CharacterType.CAT)
            localPlayerController.CmdSelectCharacter(CharacterType.NONE);
        else
            localPlayerController.CmdSelectCharacter(CharacterType.CAT);
    }

    public void SelectGOLDILOCKS()
    {
        if (localPlayerController == null) return;
        if (currentSelectedCharacter == CharacterType.GOLDILOCKS)
            localPlayerController.CmdSelectCharacter(CharacterType.NONE);
        else
            localPlayerController.CmdSelectCharacter(CharacterType.GOLDILOCKS);
    }

}