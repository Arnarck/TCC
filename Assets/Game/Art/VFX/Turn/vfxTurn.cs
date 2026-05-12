using UnityEngine;

public class vfxTurn : MonoBehaviour, iVFX
{
    

    public Animator anim;


    public void Active()
    {
        anim.SetTrigger("MyTurn");
    }    
}
