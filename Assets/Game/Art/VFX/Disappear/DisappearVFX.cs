using UnityEngine;

public class DisappearVFX : MonoBehaviour
{
    public Animator animator;
    public void Active()
    {
        animator.SetTrigger("Disappear");
    }
}
