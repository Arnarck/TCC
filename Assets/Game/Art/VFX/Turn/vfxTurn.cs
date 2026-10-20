using UnityEngine;
using System.Collections.Generic;

public class VFXTurn : MonoBehaviour
{
    

    //public Animator anim;
    public GameObject light001;
    public Animator animator;

    public AudioSource audioSource;


    public void OnLight()
    {
        light001.SetActive(true);
        animator.SetTrigger("Active");
    } 

    public void OffLight()
    {
        animator.SetTrigger("Active");
    }
    private void OffObject()
    {
        light001.SetActive(false);
        
    }

    public void OnAudio()
    {
        audioSource.Play();
    }

}