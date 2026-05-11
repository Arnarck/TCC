using UnityEngine;
using UnityEngine.VFX;


public class SelectCard : MonoBehaviour, iVFX
{

    public Animator anim;

    public void Active()
    {
        anim.SetTrigger("Select");
    }
}
