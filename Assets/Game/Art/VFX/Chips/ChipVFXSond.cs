using UnityEngine;

public class ChipVFXSond : MonoBehaviour
{
    public AudioSource audio;
    public void StartSound()
    {
        audio.Play();
    }

    private void destroy_this()
    {
        Destroy(this.gameObject);
    }
}
