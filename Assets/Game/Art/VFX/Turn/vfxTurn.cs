using UnityEngine;
using System.Collections.Generic;

public class vfxTurn : MonoBehaviour
{
    

    //public Animator anim;
    public GameObject light001;
    List<GameObject> listCard = new List<GameObject>();
    bool on;


    public void Active()//List<GameObject> cards)
    {
        //listCard = cards;
        //OnLight();
        //OnCanvaCard();
    }
    public void Desactive()
    {
        //OffLight();
        //OffCanvaCard();
    }
    

    private void OnLight()
    {
        light001.SetActive(true);
    } 
     private void OffLight()
    {
        light001.SetActive(false);
    } 
    private void OnCanvaCard()
    {
        //for(int i = 0; i < listCard.Count; i++)
        //{
        //    listCard[i].GetComponentInChildren<vfxOutlineConfig>().Active();
        //}
    }
    private void OffCanvaCard()
    {
        //for(int i = 0; i < listCard.Count; i++)
        //{
        //    listCard[i].GetComponentInChildren<vfxOutlineConfig>().Desactive();
        //}
    }
}
