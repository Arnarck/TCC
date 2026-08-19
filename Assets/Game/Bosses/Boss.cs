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
    public float finish_turn_t;

    void Awake()
    {
        GI.boss = this;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if (finish_turn_t > 0f)
        {
            finish_turn_t -= dt;
            if (finish_turn_t <= 0f)
            {
                int ability_to_use = Random.Range(0, (int)Boss_Abilities.COUNT);
                if (ability_to_use == 0)
                {
                    GI.player.remove_points(5);
                }
                else
                {
                    GI.player.remove_points(15);
                }

                GI.card_system.update_turn();
            }
        }
    }

    public void start_game()
    {

    }

    public void start_turn()
    {
        finish_turn_t = 2f;
    }
}
