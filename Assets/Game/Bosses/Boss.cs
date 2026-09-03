using UnityEngine;


public enum Boss_Type
{
    CAT,
    WITCH,
    KAME,

    COUNT
}

public enum Boss_Abilities
{
    REMOVE_PLAYER_POINTS,
    REMOVE_A_LOT_OF_PLAYER_POINTS,

    COUNT
}


public class Boss : MonoBehaviour
{
    public Boss_Type type;

    [Header("INTERNAL")]
    public int health;
    public float finish_turn_t;

    void Awake()
    {
        GI.boss = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (GI.player.game_stopped)
        {
            return;
        }

        float dt = Time.deltaTime;

        if (finish_turn_t > 0f)
        {
            finish_turn_t -= dt;
            if (finish_turn_t <= 0f)
            {
                int ability_to_use = Random.Range(0, (int)Boss_Abilities.COUNT);
                if (ability_to_use == 0)
                {
                    GI.player.take_damage(5);
                }
                else
                {
                    GI.player.take_damage(15);
                }

                GI.card_system.update_turn();
            }
        }
    }

    public void init()
    {
        gameObject.SetActive(true);
    }

    public void start_game()
    {
        health = 100;
        GI.player_hud.update_boss_health_text();
    }

    public void start_turn()
    {
        finish_turn_t = 2f;
    }

    public void take_damage(int amount)
    {
        health -= amount*3; // @TODO: Remove the '*3'
        if (health <= 0)
        {
            health = 0;
            GI.player.win();
        }

        GI.player_hud.update_boss_health_text();
    }
}
