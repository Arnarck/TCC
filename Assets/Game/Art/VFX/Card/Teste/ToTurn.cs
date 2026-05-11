using UnityEngine;

public class ToTurn : MonoBehaviour, iVFX
{
    public Animator anim;

    public void Active()
    {
        anim.SetTrigger("ToTurn");
    }

   
}
