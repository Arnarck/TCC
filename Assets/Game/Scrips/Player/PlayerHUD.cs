using UnityEngine;
using Mirror;
using TMPro;

public class PlayerHUD : NetworkBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundsText;

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
        roundsText.text = value.ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
