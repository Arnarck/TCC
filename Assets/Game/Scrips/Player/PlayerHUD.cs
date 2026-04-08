using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI pointsText;

    public void UpdateScore()
    {
        pointsText.text = player.score.ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
