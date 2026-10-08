
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VFXChips : MonoBehaviour
{
    public GameObject chip;
    public CardSystem cardSystem;

    public List<StackChips> stacks;

    public Vector3 offset = new Vector3(0, 0.3f, 0);

    public float delay = 1f;
    int total_Chips;

    public void SetChips(int total_Chips) { this.total_Chips = total_Chips;}

    public void Start()
    {
        StartCoroutine(MaxChips());
    }
    IEnumerator MaxChips()
    {
        yield return new WaitForSeconds(cardSystem.memorization_time);
        for(int i = 0; i < stacks.Count ;i++)
        stacks[i].AddChip(chip, stacks[i].limit, delay, offset);
    }

}
