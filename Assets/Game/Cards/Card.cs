using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;
using TMPro;

public enum Card_Type
{
    // Family 1
    WOODEN_HOUSE_PIG,
    PUSS_IN_BOOTS,
    BAD_WITCH,
    BIG_BAD_WOLF,

    // Family 2
    STRAW_HOUSE_PIG,
    PRINCESS_AND_FROG,
    SLEEPING_BEAUTY,
    UGLY_DUCK,
    HUMAN_FROG, // Invocable

    // Family 3
    BRICK_HOUSE_PIG,
    LITTLE_RED_RIDING_HOOD,
    GEPETTO,
    CINDERELLA,
    GRANNY, // Invocable
    PINOCCHIO, // Invocable

    // Bosses
    BAD_APPLE,
    USELESS_DWARF,
    ANNOYING_DWARF,
    DIZZINESS,

    COUNT
}

public enum Family_Type
{
    PAW,
    CANDY,
    LOTUS,

    COUNT // Used as a way of knowing how many elements there is in this enum.
}

public enum Attack_Type
{
    NONE,
    DAMAGE_BOSS,
    HEAL_BOSS,
    DAMAGE_PLAYER,
    HEAL_PLAYER,

    COUNT
}

public enum Keyword_Type
{
    PROMOTE,
    DEMOTE,
    SWAP,
    SABOTAGE,
    DESTROY,
    RECRUIT,

    COUNT,
}

public class Card : MonoBehaviour
{
    public Card_Type type;
    public Family_Type family_type;
    public Keyword_Type keyword_type;
    public Family_Type[] families_in_trio_to_activate_ability;

    [Header("HUD")]
    public GameObject ability_description_canvas;
    public TextMeshProUGUI ability_description_text;
    public TextMeshProUGUI points_text;
    public Image arrow_up_image;
    public Image arrow_down_image;
    public Image family_type_image;
    [TextArea(5, 20)]public string ability_description;

    [Header("ATTACK")]
    public Color heal_color;
    public Color damage_color;
    public Attack_Type attack_1;
    public Attack_Type attack_2;
    public int attack_amount;

    [Header("VFX")]
    public Animator animator;
    public GameObject visual;
    public vfxMove vfx_steal;
    public TeleportCard teleport_card;
    public ActiveCard active_card;
    public SelectCard select_card;
    public ToTurn to_turn;
    public vfxTransform vfx_transform;
    public vfxHandFull vfx_hand_full;
    public vfxShuffle vfx_shuffle;
    public vfxDistribute vfx_distribute;
    public vfxCreate vfx_create;

    public bool is_revealed = true;

    [Header("INTERNAL")]
    public bool ignore_card_ability;
    public bool blow_up_if_selected;
    public bool remove_player_points_when_selected;
    public bool is_in_desk;
    public float disable_t;
    public bool promoted_at_start_of_turn;
    public int remove_points_t;
    public int points_to_remove_after_x_turns;
    public bool has_improved_after_a_trio;

    private void Start()
    {
        family_type_image.sprite = GI.player_hud.families_sprite[(int)family_type];

        update_points_text();
        update_arrows_image();
    }

    private void Update()
    {
        if (GI.player_card_game.game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (disable_t > 0f)
        {
            disable_t -= dt;
            if (disable_t <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void update_arrows_image()
    {
        arrow_up_image.enabled = false;
        arrow_down_image.enabled = false;

        if (attack_1 == Attack_Type.DAMAGE_BOSS || attack_1 == Attack_Type.HEAL_BOSS ||
            attack_2 == Attack_Type.DAMAGE_BOSS || attack_2 == Attack_Type.HEAL_BOSS)
        {
            arrow_up_image.enabled = true;
        }

        if (attack_1 == Attack_Type.DAMAGE_PLAYER || attack_1 == Attack_Type.HEAL_PLAYER ||
            attack_2 == Attack_Type.DAMAGE_PLAYER || attack_2 == Attack_Type.HEAL_PLAYER)
        {
            arrow_down_image.enabled = true;
        }

        if (attack_1 != Attack_Type.NONE)
        {
            update_arrow_color(attack_1);

            if (attack_2 != Attack_Type.NONE)
            {
                update_arrow_color(attack_2);
            }
        }
        else if (attack_2 != Attack_Type.NONE)
        {
            arrow_down_image.enabled = false;

            update_arrow_color(attack_2);
        }
    }

    public void PlayVFXWolfEatPig()
    {
        animator.SetTrigger("Ability_EatPig");
    }
    public void disable_card_ability_description_canvas()
    {
        ability_description_canvas.SetActive(false);
    }

    public void update_points_text()
    {
        points_text.text = attack_amount.ToString();
    }

    public void update_arrow_color(Attack_Type attack)
    {
        if      (attack == Attack_Type.DAMAGE_BOSS)   { arrow_up_image.color = damage_color; }
        else if (attack == Attack_Type.HEAL_BOSS)     { arrow_up_image.color = heal_color; }
        else if (attack == Attack_Type.HEAL_PLAYER)   { arrow_down_image.color = heal_color; }
        else if (attack == Attack_Type.DAMAGE_PLAYER) { arrow_down_image.color = damage_color; }
    }

    public void improve_points_after_a_trio_to_self_demote_after_turn(int amount)
    {
        attack_amount += amount;
        has_improved_after_a_trio = true;

        update_points_text();
    }

    public void turn_card()
    {
        to_turn.Active();
        is_revealed = !is_revealed;
    }
    public void distribute_cards(Transform pointA, Transform pointB) //@VITOR
    {
        vfx_distribute.Active(pointA, pointB);
    }
    public void MoveCard(Transform pointA, Transform pointB, float speed) //@VITOR
    {
        vfx_distribute.Active(pointA, pointB, speed);
    }
    public void create_card()//@VITOR
    {
        vfx_create.Active();//spline, target_hand_pos);
    }

    public void remove_points(int amount)
    {
        attack_amount -= amount;
        if (attack_amount < 0)
        {
            attack_amount = 0;
        }

        update_points_text();
    }

    public void add_to_player_hand()
    {
        if (blow_up_if_selected)
        {
            GI.player_card_game.lose();
            return;
        }

        is_in_desk = false;

        if (remove_player_points_when_selected)
        {
            GI.player_card_game.take_damage(5);
        }
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
        __swap_attack(ref attack_1);
        __swap_attack(ref attack_2);

        update_arrows_image();
    }

    public void __swap_attack(ref Attack_Type type)
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

    public void add_to_desk()
    {
        is_in_desk = true;
    }

    public void disable_from_trio()
    {
        active_card.Active(transform.position, transform.rotation); // VFX
        disable_t = 2f;
    }

    public void add_points(int amount)
    {
        attack_amount += amount;

        update_points_text();
    }

    public void add_points_temporarily(int amount, int turns)
    {
        attack_amount += amount;

        remove_points_t = turns;
        points_to_remove_after_x_turns = amount;

        update_points_text();
    }

    public void destroy()
    {
        gameObject.SetActive(false);
    }
}

