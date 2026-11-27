using UnityEngine;

public class ShuffleDeckVFX : MonoBehaviour
{
    public Animator animator;

    public void Active()
    {
        animator.SetTrigger("Shuffle");

    }

}
