using UnityEngine;
using System.Collections.Generic;

public class vfxTurn : MonoBehaviour
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
        audioSource.Play();
        light001.SetActive(false);
    }
    

    public void OnAudio()
    {
        audioSource.Play();
    }

}