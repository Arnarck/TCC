using UnityEngine;

public class vfxSteal : MonoBehaviour, iVFX
{
    public Transform pontoB;
    public float velocidade = 5f;

    private bool indoParaB = true;
    private bool active = false;

    [Header("Animator")]

    public Animator anim;

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
            anim.SetTrigger("EndSteal");
        }
        }
    }
    public void Active()
    {
        anim.SetTrigger("ToTurn");
        anim.SetTrigger("StartSteal");
    }
    void ActiveMov()
    {
        active = !active;
    }
}
