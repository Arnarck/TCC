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
    public int count = 0;
    GameObject chip;
    public int AddChip(GameObject chip, int quant, float delay, Vector3 offset)
    {
        this.chip = chip;
        this.delay = delay;
        this.offset = offset;
        
        if(count < limit){
        
            StartCoroutine(Create(quant));
            
            return count;
        }

        return -1;
    }
    IEnumerator Create(int quant)
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

    public int RemoveChip()
    {
        if(count > 0) {
            StartCoroutine(Remove());
            return 1;}
        
        return -1;
        
    }
    IEnumerator Remove()
    {
         count--;
         
        stack[count].GetComponentInChildren<Animator>().SetTrigger("Active");

        yield return new WaitForSeconds(2);

        Destroy(stack[count]);
        stack.RemoveAt(count);
    } 
}