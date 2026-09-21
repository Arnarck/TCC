using UnityEngine;

public class BossCard : MonoBehaviour
{
    public Boss_Abilities ability_type;
    public int cost_to_disable_ability = 5;

    [Header("ATTACK")]
    public Attack_Type attack_1;
    public Attack_Type attack_2;
    public int attack_amount;
    public int turn_off_t;

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
