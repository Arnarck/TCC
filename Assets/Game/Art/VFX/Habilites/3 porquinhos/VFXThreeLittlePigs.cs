
using UnityEngine;

public class VFXThreeLittlePigs : MonoBehaviour
{
    public Animator animator;
    public void OnEnable()
    {
        animator.SetTrigger("Active");       
    }

    public void Desable()
    {
        animator.SetTrigger("Active");
    }
    void DestroyThis()
    {
        Destroy(this.gameObject);
    }
}
