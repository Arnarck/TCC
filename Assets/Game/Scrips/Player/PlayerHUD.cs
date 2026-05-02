using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : NetworkBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI currentTurnTimeText;
    public Button endCurrentTurnButton;
    public GameObject gameplayHUD;
    public GameObject spectatorHUD;
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject memorizationPhasePanel;
    public GameObject mainHUD;
    public GameObject messagePanel;
    public TextMeshProUGUI messagePanelText;

    [Header("INTERNAL")]
    public float showMessage_t;

 [Header("Respect UI")]
    public TextMeshProUGUI respectF1Text;
    public TextMeshProUGUI respectF2Text;
    public TextMeshProUGUI respectF3Text;
    public TextMeshProUGUI respectF4Text;

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            GI.playerHUD = this;
            UpdateCurrentRound(0);
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
    }

    public void UpdateScore()
    {
        scoreText.text = player.score.ToString();
    }

    public void UpdateCurrentRound(int value)
    {
        roundsText.text = (value + 1).ToString(); // Starts from 1, rather than from 0
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
        showMessage_t = t;
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
        if (!isLocalPlayer) return; // só atualiza para o jogador local

        float real = scaledValue / 100f;
        int level = Mathf.Clamp(Mathf.FloorToInt(real), 0, 6);
        string text = $"{real:F1} (Nv.{level})";

        switch (family)
        {
            case Family_Type.FAMILY_1:
                if (respectF1Text != null) respectF1Text.text = "F1: "  + text;
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
}
