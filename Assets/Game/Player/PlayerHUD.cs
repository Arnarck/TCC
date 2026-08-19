using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI points_text;

    public void update_points_text()
    {
        points_text.text = "Chips: " + GI.player.points;
    }
}
