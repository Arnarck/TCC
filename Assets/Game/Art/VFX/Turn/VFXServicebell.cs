using UnityEngine;

public class VFXServicebell : MonoBehaviour
{
    public Animator animator;
    public AudioSource audio;

    public void Active()
    {
        animator.SetTrigger("Active");
        audio.Play();
    }

}
