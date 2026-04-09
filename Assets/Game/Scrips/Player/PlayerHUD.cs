using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : NetworkBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI currentTurnTimeText;
    public Button endCurrentTurnButton;
    public GameObject gameplayHUD;
    public GameObject spectatorHUD;

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            GI.playerHUD = this;
            UpdateCurrentRound(0);
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
