using UnityEngine;

public class vfxSteal : MonoBehaviour, iVFX
{
    public Transform pontoB;
    public float velocidade = 5f;

    private bool indoParaB = true;
    private bool active = false;

    void Update()
    {
        if(active)
        {
        Vector3 alvo = indoParaB ? pontoB.position : transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            alvo,
            velocidade * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, alvo) < 0.1f)
        {
            indoParaB = !indoParaB;
        }
        }
    }
    public void Active()
    {
        active = !active;
    }
}
