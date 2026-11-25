using UnityEngine;


public enum Audio_Type
{
    // Gameplay
    COLLECT_CARD_FROM_DESK,
    ACTIVATE_PLAYER_CARD_ABILITY,
    SWITCH_TURN,
    ACTIVATE_BOSS_CARD_ABILITY,
    MAKE_TRIO,
    DELETE_PLAYER_CARD,
    SELECT_CARD_IN_HAND,
    START_MEMORIZATION_PHASE,
    THREE_CARDS_SELECTED,
    PLAYER_WIN,
    PLAYER_LOSE,
    BOSS_INTRO,
    BOSS_TAKE_DAMAGE,
    PLAYER_TAKE_DAMAGE,
    BOSS_PHASE_2,

    // UI
    BUTTON_CLICK,
    SWITCH_MENU,

    // Ambient
    AMBIENT_SOUND_1,
    AMBIENT_SOUND_2,

    // Music
    GAMEPLAY_MUSIC,
    MAIN_MENU_MUSIC,

    COUNT,
}


public class AudioPlayer : MonoBehaviour
{
    public AudioSource[] audios;

    void Awake()
    {
        GI.audio_player = this;
    }

    private void Start()
    {
        play(Audio_Type.AMBIENT_SOUND_1);
        play(Audio_Type.AMBIENT_SOUND_2);

        if (GI.player_card_game)
        {
            play(Audio_Type.GAMEPLAY_MUSIC);
        }
        else
        {
            play(Audio_Type.MAIN_MENU_MUSIC);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void play(Audio_Type type)
    {
        audios[(int)type].Play();
        Debug.Log("Playing: " + type);
    }
}
