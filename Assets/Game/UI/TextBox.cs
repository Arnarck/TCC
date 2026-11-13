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

    public void init(string message, Transform parent)
    {
        transform.SetParent(parent);
        gameObject.SetActive(true);
        text.text = message;

        alive_t = 3f;
        update_text_and_background_opacity(1f);
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
                destroy();
            }

            update_text_and_background_opacity(fade_t);
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

    public void update_text_and_background_opacity(float opacity)
    {
        Color text_color = text.color;
        Color background_image_color = background_image.color;

        text_color.a = opacity;
        background_image_color.a = opacity;

        text.color = text_color;
        background_image.color = background_image_color;
    }

    public void destroy()
    {
        gameObject.SetActive(false);
        GI.player_hud.add_text_box_to_pool(this);
    }
}
