using UnityEngine;

public class vfxTurn : MonoBehaviour, iVFX
{
    

    public Animator anim;
    public GameObject light001;


    public void Active()
    {
        anim.SetTrigger("MyTurn");
    }   

    private void OnOffLight()
    {
        light001.SetActive(!light001.activeSelf);
    } 
}
