using UnityEngine;

public class BossCard : MonoBehaviour
{
    public Boss_Abilities ability_type;

    [Header("ATTACK")]
    public Attack_Type attack_1;
    public Attack_Type attack_2;
    public int attack_amount;

    public void do_attack()
    {
        __attack(attack_1);
        __attack(attack_2);
    }

    public void __attack(Attack_Type type)
    {
        switch (type)
        {
            case Attack_Type.DAMAGE_BOSS: { GI.boss.take_damage(attack_amount); } break;
            case Attack_Type.HEAL_BOSS: { GI.boss.add_health(attack_amount); } break;
            case Attack_Type.DAMAGE_PLAYER: { GI.player_card_game.take_damage(attack_amount); } break;
            case Attack_Type.HEAL_PLAYER: { GI.player_card_game.add_health(attack_amount); } break;
            default: break;
        }
    }

    public void swap_attacks()
    {
        __swap_attack(ref attack_1);
        __swap_attack(ref attack_2);
    }

    public void __swap_attack(ref Attack_Type type)
    {
        switch (type)
        {
            case Attack_Type.DAMAGE_BOSS: { type = Attack_Type.HEAL_BOSS; } break;
            case Attack_Type.HEAL_BOSS: { type = Attack_Type.DAMAGE_BOSS; } break;
            case Attack_Type.DAMAGE_PLAYER: { type = Attack_Type.HEAL_PLAYER; } break;
            case Attack_Type.HEAL_PLAYER: { type = Attack_Type.DAMAGE_PLAYER; } break;
            default: break;
        }
    }

    public void destroy()
    {
        gameObject.SetActive(false);
    }
}
