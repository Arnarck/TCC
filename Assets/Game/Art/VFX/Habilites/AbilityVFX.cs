using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class AbilityVFX : MonoBehaviour
{
    [Header("Three Little Pigs")]
    public List<GameObject> house = new List<GameObject>();

    public Transform spawn_ThreeLittlePigs;
    GameObject houseActiveSelf;

    [Header("Wolf eating pigs")]

    public Animator cardPig;
    

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlayAnimVFXWolfEatPig();
        }
        
    }
    public void PlayAnimVFXWolfEatPig()
    {
        cardPig.SetTrigger("Ability_EatPig");
    }
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

}
