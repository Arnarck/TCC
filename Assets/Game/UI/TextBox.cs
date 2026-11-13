using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBox : MonoBehaviour
{
    public Image background_image;
    public TextMeshProUGUI text;
    public float fade_speed;

    [Header("INTERNAL")]
    public float fade_t;
    public float alive_t;

    public void init(string message)
    {
        gameObject.SetActive(true);
        text.text = message;

        alive_t = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if (fade_t > 0f)
        {
            fade_t -= dt*fade_speed;
            if (fade_t <= 0f)
            {
                fade_t = 0f;
                gameObject.SetActive(false);
            }

            Color text_color             = text.color;
            Color background_image_color = background_image.color;

            text_color.a             = fade_t;
            background_image_color.a = fade_t;

            text.color             = text_color;
            background_image.color = background_image_color;
        }

        if (alive_t > 0f)
        {
            alive_t -= dt;
            if (alive_t <= 0f)
            {
                fade_t = 1f;
            }
        }
    }
}
