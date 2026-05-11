using UnityEngine;
using UnityEngine.VFX;


public class ActiveCard : MonoBehaviour, iVFX
{
    public Animator anim;
    public VisualEffect vfx;

    public void Active()
    {
        anim.SetTrigger("Active");
    }
    private void ActiveVFX()
    {
        vfx.Play();
    }
}
