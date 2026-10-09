
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class VFXChips : MonoBehaviour
{
    public GameObject chip;
    public CardSystem cardSystem;

    public List<StackChips> stacks;
    List<int> blockStacks;

    public Vector3 offset = new Vector3(0, 0.3f, 0);

    public float delay = 1f;

    public void Start()
    {
        StartCoroutine(MaxChips());
    }
    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame) RemoveChips(10);
    }
    IEnumerator MaxChips()
    {
        yield return new WaitForSeconds(cardSystem.memorization_time);

        int total = 0;

        for(int i = 0; i < stacks.Count ;i++)
        {
            stacks[i].AddChip(chip, stacks[i].limit, delay, offset);
            total += stacks[i].limit;
        }

        Debug.Log("Total de Fichas na mesa = " + total);
        
    }
    bool RemoveChips(int quant)  //Retorna false se estiver sem fichas
    {
        int i = 0;
        while(quant > 0 && blockStacks.Count != stacks.Count)
        {
            int aux = i%stacks.Count;

            if(stacks[aux].count <= 0 && ConfirmBlockList(aux)) blockStacks.Add(aux);

            if (ConfirmBlockList(aux))
            {
                stacks[aux].RemoveChip();
                quant--;
            }
            i++;
        }
        if(quant != 0)
        {
            return false;
        }

        return true;
    }
    bool ConfirmBlockList(int valor) //Retorna false se o valor estiver na lista
    {
        bool block = true;;
        for(int i = 0; i < blockStacks.Count; i++)
        {
            if(blockStacks[i] == valor) block = false;
        }

        return block;
    }

}
