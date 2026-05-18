using UnityEngine;

public class vfxHandFull : MonoBehaviour, iVFX
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Animator anim;
    public void Active()
    {
        anim.SetTrigger("HandFull");    
    }
}
