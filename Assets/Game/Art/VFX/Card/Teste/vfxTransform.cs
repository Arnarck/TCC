using UnityEngine;
using UnityEngine.VFX;

public class vfxTransform : MonoBehaviour, iVFX
{

    public Animator anim;
    public VisualEffect vfx;
    public GameObject curve;


    public void Active()
    {
        anim.SetTrigger("Transform");
        
    } 

    private void ActiveVFXTransform()
    {
        vfx.Play();
    }
    private void ActiveCurveTransform()
    {
        curve.SetActive(true);
    }

    private void DisableCurveTransform()
    {
        curve.SetActive(false);
    }
}
