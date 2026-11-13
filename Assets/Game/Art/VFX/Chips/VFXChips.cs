
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VFXChips : MonoBehaviour
{
    public GameObject chip;
    public CardSystem cardSystem;

    public List<StackChips> stacks;

    public Vector3 offset = new Vector3(0, 0.3f, 0);
    public float delay = 0.15f;

    private Boss currentBoss;
    private bool initialized;

    void Start()
    {
        StartCoroutine(SyncBossChips());
    }

    IEnumerator SyncBossChips()
    {
        // Mantém o tempo original antes de mostrar as fichas.
        yield return new WaitForSeconds(cardSystem.memorization_time);

        initialized = true;

        while (true)
        {
            if (GI.boss != null && GI.boss.isActiveAndEnabled)
            {
                // Detecta troca de Boss.
                if (currentBoss != GI.boss)
                {
                    currentBoss = GI.boss;
                    Debug.Log("Novo Boss detectado: " + currentBoss.name);
                }

                int target = Mathf.Max(0, currentBoss.health);
                int current = GetTotalChips();

                // Adiciona fichas até alcançar a vida do Boss.
                if (current < target)
                {
                    if (AddOneChip())
                    {
                        yield return new WaitForSeconds(delay);
                    }
                    else
                    {
                        Debug.LogWarning(
                            "Não há espaço suficiente nas pilhas para representar a vida do Boss."
                        );
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                // Remove fichas quando o Boss perde vida.
                else if (current > target)
                {
                    if (RemoveOneChip())
                    {
                        // RemoveChip possui animação de 2 segundos.
                        yield return new WaitForSeconds(2.1f);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    int GetTotalChips()
    {
        int total = 0;

        foreach (StackChips stack in stacks)
        {
            if (stack != null)
                total += stack.count;
        }

        return total;
    }

    bool AddOneChip()
    {
        foreach (StackChips stack in stacks)
        {
            if (stack != null && stack.count < stack.limit)
            {
                return stack.AddOneChip(chip, offset);
            }
        }

        return false;
    }

    bool RemoveOneChip()
    {
        // Procura uma pilha que ainda tenha fichas.
        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            if (stacks[i] != null && stacks[i].count > 0)
            {
                return stacks[i].RemoveChip() > 0;
            }
        }

        return false;
    }

    public int GetBossChipCount()
    {
        return GetTotalChips();
    }
}
