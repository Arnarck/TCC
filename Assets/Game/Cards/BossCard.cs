using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum Boss_Card_Disable_Type
{
    NONE,
    CHIPS,
    FAMILY,

    COUNT
}


public class BossCard : MonoBehaviour
{
    public Boss_Abilities ability_type;
    public Boss_Card_Disable_Type disable_type;
    public int cost_to_disable_ability = 5;
    public Keyword_Type keyword_type;
    [TextArea(3, 15)]public string ability_description;
    public Family_Type[] families_to_disable_card;

    [Header("HUD")]
    public GameObject ability_description_canvas;
    public Transform ability_description_canvas_parent;
    public TextMeshProUGUI ability_description_text;
    public GameObject disable_price_panel;
    public GameObject disable_families_panel;
    public TextMeshProUGUI disable_price_text;
    public Image[] disable_families_image;

    [Header("ATTACK")]
    public Attack_Type attack_1;
    public Attack_Type attack_2;
    public int attack_amount;
    public int turn_off_t;

    [Header("INTERNAL")]
    public int current_family_to_disable;

    private void Start()
    {
        ability_description_canvas.SetActive(false);
    }

    private void Update()
    {
        if (ability_description_canvas.activeInHierarchy)
        {
            ability_description_canvas_parent.LookAt(GI.player_card_game.player_camera.transform);
        }
    }

    public void show_card_ability_description_canvas()
    {
        ability_description_canvas.SetActive(true);

        ability_description_text.text = ability_description;

        if (disable_type == Boss_Card_Disable_Type.CHIPS)
        {
            disable_price_panel.SetActive(true);
            disable_families_panel.SetActive(false);

            disable_price_text.text = cost_to_disable_ability.ToString();
        }
        else if (disable_type == Boss_Card_Disable_Type.FAMILY)
        {
            disable_price_panel.SetActive(false);
            disable_families_panel.SetActive(true);

            for (int i = 0; i < disable_families_image.Length; i++)
            {
                if (i < families_to_disable_card.Length && i >= current_family_to_disable)
                {
                    disable_families_image[i].enabled = true;
                    disable_families_image[i].sprite = GI.player_hud.families_sprite[(int)families_to_disable_card[i]];
                }
                else
                {
                    disable_families_image[i].enabled = false;
                }
            }
        }
    }

    public void hide_card_ability_description_canvas()
    {
        ability_description_canvas.SetActive(false);
    }

    public void turn_off(int turns)
    {
        turn_off_t = turns;
    }

    public void do_attack()
    {
        __attack(attack_1);
        __attack(attack_2);
    }

    public void __attack(Attack_Type type)
    {
        switch (type)
        {
            case Attack_Type.DAMAGE_BOSS:   { GI.boss.take_damage(attack_amount); } break;
            case Attack_Type.HEAL_BOSS:     { GI.boss.add_health(attack_amount); } break;
            case Attack_Type.DAMAGE_PLAYER: { GI.player_card_game.take_damage(attack_amount); } break;
            case Attack_Type.HEAL_PLAYER:   { GI.player_card_game.add_health(attack_amount); } break;
            default: break;
        }
    }

    public void swap_attacks()
    {
        swap_attack(ref attack_1);
        swap_attack(ref attack_2);
    }

    public void swap_attack(ref Attack_Type type)
    {
        switch (type)
        {
            case Attack_Type.DAMAGE_BOSS:   { type = Attack_Type.HEAL_BOSS; } break;
            case Attack_Type.HEAL_BOSS:     { type = Attack_Type.DAMAGE_BOSS; } break;
            case Attack_Type.DAMAGE_PLAYER: { type = Attack_Type.HEAL_PLAYER; } break;
            case Attack_Type.HEAL_PLAYER:   { type = Attack_Type.DAMAGE_PLAYER; } break;
            default: break;
        }
    }

    public void disable_ability()
    {
        GI.boss.remove_card_from_desk(this, true);
    }

    public void destroy()
    {
        gameObject.SetActive(false);
    }
}
