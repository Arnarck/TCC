using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : NetworkBehaviour
{
    public GameObject showStartMessagePanel;
    public GameObject showEndTurnMessagePanel;
    public GameObject showAnteTurnMessagePanel;
    public RectTransform cardPanel;
    public TextMeshProUGUI cardAbilityDescriptionText;
    public TextMeshProUGUI cardPointsText;
    public GameObject canvasWorldSpace;
    public PlayerController player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreTextWorldSpace;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI currentTurnTimeText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI roundsUntilAnteText;
    public TextMeshProUGUI antePriceText;
    public Button endCurrentTurnButton;
    public Button startGameButton;
    public GameObject gameplayHUD;
    public GameObject spectatorHUD;
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject memorizationPhasePanel;
    public GameObject mainHUD;
    public GameObject messagePanel;
    public TextMeshProUGUI messagePanelText;
    public TextMeshProUGUI[] trioScoreTexts;

    [Header("Scoreboard")]
    public TextMeshProUGUI scoreboardText;
    [Header("INTERNAL")]
    public GameObject currentCardShownInPanel;
    public float showStartMessage_t;
    public float showEndTurnMessage_t;
    public float showAnteTurnMessage_t;
    public float showMessage_t;
    public float[] showTrioScoreText_t;
    public float[] showTrioScoreTextDelay_t;

    [Header("Respect UI")]
    public TextMeshProUGUI respectF1Text;
    public TextMeshProUGUI respectF2Text;
    public TextMeshProUGUI respectF3Text;
    public TextMeshProUGUI respectF4Text;

    void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClick);

        showTrioScoreText_t = new float[trioScoreTexts.Length];
        showTrioScoreTextDelay_t = new float[trioScoreTexts.Length];

        if (isLocalPlayer)
        {
            canvasWorldSpace.SetActive(false);
        }
    }

    void OnStartGameClick()
    {
        if (!isServer) return;
        player.CmdHostStartGame();
    }
     public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            GI.playerHUD = this;
            UpdateCurrentRound(0);

            PlayerController.OnScoreChanged += OnAnyScoreChanged;
            RefreshScoreboard();
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (showMessage_t > 0f)
        {
            showMessage_t -= dt;
            if (showMessage_t <= 0f)
            {
                messagePanel.SetActive(false);
            }
        }

        for (int i = 0; i < showTrioScoreTextDelay_t.Length; i++)
        {
            if (showTrioScoreText_t[i] > 0f)
            {
                showTrioScoreText_t[i] -= dt;
                if (showTrioScoreText_t[i] <= 0f)
                {
                    trioScoreTexts[i].enabled = false;
                }
            }

            if (showTrioScoreTextDelay_t[i] > 0f)
            {
                showTrioScoreTextDelay_t[i] -= dt;
                if (showTrioScoreTextDelay_t[i] <= 0f)
                {
                    trioScoreTexts[i].enabled = true;
                    showTrioScoreText_t[i] = 1.5f;
                }
            }
        }

        if (!isLocalPlayer && GI.playerHUD)
        {
            canvasWorldSpace.transform.LookAt(GI.playerHUD.player.playerCamera.transform);
        }

        if (showStartMessage_t > 0f)
        {
            showStartMessage_t -= dt;
            if (showStartMessage_t <= 0f)
            {
                showStartMessage_t = 0f;
                showStartMessagePanel.SetActive(false);
            }
        }

        if (showEndTurnMessage_t > 0f)
        {
            showEndTurnMessage_t -= dt;
            if (showEndTurnMessage_t <= 0f)
            {
                showEndTurnMessage_t = 0f;
                showEndTurnMessagePanel.SetActive(false);
            }
        }

        if (showAnteTurnMessage_t > 0f)
        {
            showAnteTurnMessage_t -= dt;
            if (showAnteTurnMessage_t <= 0f)
            {
                showAnteTurnMessage_t = 0f;
                showAnteTurnMessagePanel.SetActive(false);
            }
        }
    }

    public void ShowTrioScoreText(int index, int score, float delay)
    {
        showTrioScoreText_t[index] = 0f;
        showTrioScoreTextDelay_t[index] = delay;

        trioScoreTexts[index].text = "+" + score;
    }

    public void UpdateScore()
    {
        scoreText.text = player.score.ToString();
        if (player.score < player.antePrice)
        {
            scoreText.color = Color.red;
            //scoreText.text += "<size=50%>(Not enough money!)";
        }
        else
        {
            scoreText.color = Color.green;
        }
    }

    public void UpdateWorldSpaceCanvasScore()
    {
        scoreTextWorldSpace.text = "Score: " + player.score;
    }

   public void UpdateCurrentRound(int value)
{
int round = value + 1;

int price = 4 + (((round - 1) / 5) * 5);

roundText.text = $"ROUND {round}";
antePriceText.text = "Price: " + price.ToString();

if (round % 5 == 0)
{
    roundsUntilAnteText.text = "ANTE ROUND";
}
else
{
    int roundsUntilAnte = 5 - (round % 5);
    roundsUntilAnteText.text = $"{roundsUntilAnte} rounds until ante";
}

}



    [TargetRpc]
    public void TargetDisplayTurn(string message)
    {
        turnText.text = message;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void HideGameplayHUD()
    {
        gameplayHUD.SetActive(false);
    }

    public void ShowSpectatorHUD()
    {
        spectatorHUD.SetActive(true);
    }

    [TargetRpc]
    public void TargetShowMainHUD()
    {
        mainHUD.SetActive(true);
        RefreshScoreboard();
    }

    [TargetRpc]
    public void TargetHideMainHUD()
    {
        mainHUD.SetActive(false);
    }

    [TargetRpc]
    public void TargetShowMemorizationPhasePanel()
    {
        memorizationPhasePanel.SetActive(true);
    }

    [TargetRpc]
    public void TargetHideMemorizationPhasePanel()
    {
        memorizationPhasePanel.SetActive(false);
    }

    [TargetRpc]
    public void TargetShowMessage(string message, float t)
    {
        messagePanel.SetActive(true);
        messagePanelText.text = message;
        showMessage_t = t + 1.5f; // @HACK to increase message time
    }

    public void HideMessage()
    {
        messagePanel.SetActive(false);
    }

    public void ShowWin()
    {
        winUI.SetActive(true);
    }

    public void ShowLose()
    {
        loseUI.SetActive(true);
    }

    public void ShowCardAbilityPanel(string description, int points, GameObject card)
    {
        if (lobbyPanel.activeSelf)
        {
            return;
        }

        currentCardShownInPanel = card;

        Vector3 panelPosition = player.playerCamera.WorldToScreenPoint(card.transform.position);
        panelPosition.y += 250f;

        cardPanel.position = panelPosition;
        cardPanel.gameObject.SetActive(true);

        cardPointsText.text = points.ToString();
        cardAbilityDescriptionText.text = description;
    }

    public void HideCardAbilityPanel()
    {
        currentCardShownInPanel = null;
        cardPanel.gameObject.SetActive(false);
    }

    public void OnClick_ReturnToLobby()
    {
        // @TODO
    }

    public void OnClick_LeaveServer()
    {
        if (isServer && isLocalPlayer)
        {
            GI.networkManager.StopHost();
            NetworkServer.Shutdown();
        }
        else
        {
            GI.networkManager.StopClient();
        }

        Destroy(Connect.Instance.gameObject);
        Destroy(GI.networkManager.gameObject);
        SceneManager.LoadScene(0);
    }

    // 'OnClick' means that this function is called by a button on the UI.
    public void OnClick_EndCurrentTurn()
    {
        player.CmdEndCurrentTurn();
    }

    public void UpdateCurrentTurnTime()
    {
        int time = (int)Mathf.Max(0f, player.currentTurn_t);
        currentTurnTimeText.text = "Remaining Time \n<size=300%>" + time.ToString() + "s</size>";
    }

    public void UpdateRespectUI(Family_Type family, int scaledValue)
    {
        if (!isLocalPlayer) return;

        float real = scaledValue / 100f;
        int level = Mathf.Clamp(Mathf.FloorToInt(real), 0, 6);
        string text = $"{real:F1} (Nv.{level})";

        switch (family)
        {
            case Family_Type.FAMILY_1:
                if (respectF1Text != null) respectF1Text.text = "F1: " + text;
                break;
            case Family_Type.FAMILY_2:
                if (respectF2Text != null) respectF2Text.text = "F2: " + text;
                break;
            case Family_Type.FAMILY_3:
                if (respectF3Text != null) respectF3Text.text = "F3: " + text;
                break;
            case Family_Type.FAMILY_4:
                if (respectF4Text != null) respectF4Text.text = "F4: " + text;
                break;
        }
    }

    public void ShowStartTurnMessage()
    {
        showStartMessage_t = 2f;
        showStartMessagePanel.SetActive(true);
    }

    public void ShowEndTurnMessage()
    {
        showEndTurnMessage_t = 2f;
        showEndTurnMessagePanel.SetActive(true);
    }



    [Header("Lobby UI")]
    public GameObject lobbyPanel;

    [TargetRpc]
    public void TargetShowLobby()
    {
        lobbyPanel.SetActive(true);
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(isServer);
    }

    [TargetRpc]
    public void TargetHideLobby()
    {
        lobbyPanel.SetActive(false);
    }
    [TargetRpc]
    public void TargetHideConnectMenu()
    {
        Connect.Instance?.HideConnectPanel();
    }

    public void ShowAnteTurnMessage()
    {
        showAnteTurnMessage_t = 2.5f;
        showAnteTurnMessagePanel.SetActive(true);
    }






    private void OnDestroy()
    {
        if (isLocalPlayer)
            PlayerController.OnScoreChanged -= OnAnyScoreChanged;
    }

    private void OnAnyScoreChanged(PlayerController player, int newScore)
    {
        RefreshScoreboard();
    }

    private void RefreshScoreboard()
    {
        if (scoreboardText == null) return;

        var players = PlayerController.allPlayers;

        players.Sort((a, b) => b.score.CompareTo(a.score));

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            sb.AppendLine($"Player {i + 1}: {p.score}");
        }
        scoreboardText.text = sb.ToString();
    }
}
