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
}
