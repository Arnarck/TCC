using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Splines;
using UnityEngine.InputSystem;

public class AbilityVFX : MonoBehaviour
{
    [Header("Three Little Pigs")]
    public List<GameObject> house = new List<GameObject>();

    public Transform spawn_ThreeLittlePigs;
    GameObject houseActiveSelf;

    [Header("Wolf eating pigs")]

    public Animator cardPig;

    [Header("Create")]
    public GameObject createVFX;
    public Transform target_CreateVFX;
    

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            CreateCard();
        }
        
    }



    // Habilidade Lobo + Porquinhos
    public void PlayVFXWolfEatPig(GameObject card)
    {
        card.GetComponent<Animator>().SetTrigger("Ability_EatPig");
    }
    private void PlayVFXWolfEatPig()
    {
        cardPig.SetTrigger("Ability_EatPig");
    }

    // Habilidade 3 porquinhos
    public void ThreeLittlePigsVFX(int i)
    {
        if(Destroy_House()) StartCoroutine(WaitForDestroyingPigsHouse(i));
        
        else houseActiveSelf = Instantiate(house[i], spawn_ThreeLittlePigs.position, spawn_ThreeLittlePigs.rotation);
    }

    bool Destroy_House()
    {
        
        if(houseActiveSelf != null) {
            
            houseActiveSelf.GetComponent<VFXThreeLittlePigs>().Desable();
            houseActiveSelf = null;

            return true;
            }

        return false;

    }

    IEnumerator WaitForDestroyingPigsHouse(int i)
    {
        yield return new WaitForSeconds(1f);
        houseActiveSelf = Instantiate(house[i], spawn_ThreeLittlePigs.position, spawn_ThreeLittlePigs.rotation);
    }


    //Testes
    public void CreateCard(Transform pos)
    {
        GameObject obj = Instantiate(createVFX, pos.position, pos.rotation);
        obj.GetComponentInChildren<vfxCreate>().Active();
    }
    
    private void CreateCard()
    {
        GameObject obj = Instantiate(createVFX, target_CreateVFX.position, target_CreateVFX.rotation);
        obj.GetComponentInChildren<vfxCreate>().Active();//spline, target_VfxCreate);    
    }
}
