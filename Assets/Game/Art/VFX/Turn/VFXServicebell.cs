using UnityEngine;

public class VFXServicebell : MonoBehaviour
{
    public Animator animator;
    public AudioSource audio;

    public void Start()
    {
        animator.SetTrigger("Active");
        audio.Play();
    }

}
