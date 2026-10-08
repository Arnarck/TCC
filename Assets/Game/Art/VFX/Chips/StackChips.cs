using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StackChips : MonoBehaviour
{
    
    List<GameObject> stack = new List<GameObject>();
    public GameObject spawn;
    public int limit = 10;
    Vector3 offset;


    float delay = 1.0f;
    int count = 0;
    int quant;
    GameObject chip;
    public int AddChip(GameObject chip, int quant, float delay, Vector3 offset)
    {
        this.chip = chip;
        this.quant = quant;
        this.delay = delay;
        this.offset = offset;
        
        if(count < limit){
        
            StartCoroutine(Create());
            
            return count;
        }

        return -1;
    }
    IEnumerator Create()
    {
        for(int i = 0; i < quant; i++)
        {
            stack.Add(CreateChip());

            count++;

            stack[i].GetComponentInChildren<Animator>().SetTrigger("Active");
            yield return new WaitForSeconds(delay);
        }
    }
    GameObject CreateChip()
    {
        Vector3 total_offset = offset * count;
        return Instantiate(chip, spawn.transform.position+ total_offset, spawn.transform.rotation);
    }
}