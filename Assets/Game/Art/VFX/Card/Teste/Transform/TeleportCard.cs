using UnityEngine;
using UnityEngine.VFX;

public class TeleportCard : MonoBehaviour, iVFX
{
    public Animator anim;
    public VisualEffect vfx;

    public void Active()
    {
        anim.SetTrigger("Teleport");
    }
    private void ActiveVFX()
    {
        vfx.Play();
    }

    public void TeleportIN()
    {
        anim.SetTrigger("TeleportIN");
    }
    public void TeleportOUT()
    {
        anim.SetTrigger("TeleportOUT");
    }
}
