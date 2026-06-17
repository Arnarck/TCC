using UnityEngine;

public class vfxSteal : MonoBehaviour, iVFX
{
    public Transform pontoA;
    public Transform pontoB;
    public float velocidade = 5f;

    private bool indoParaB = true;
    private bool active = false;

    [Header("Animator")]

    public Animator anim;

    [Header("INTERNAL")]
    public float animation_t;
    public Vector3 startPosition;
    public Quaternion startRotation;

    void Start()
    {
        if (pontoA == false)
        {
            pontoA = transform;
        }
    }

    void Update()
    {
        if(active)
        {
        Vector3 alvo = indoParaB ? pontoB.position : transform.position;

            //transform.position = Vector3.MoveTowards(
            //    transform.position,
            //    alvo,
            //    velocidade * Time.deltaTime
            //);
            if (animation_t < 1f)
            {
                animation_t += Time.deltaTime*velocidade;
                if (animation_t >= 1f)
                {
                    animation_t = 1f;
                }
            }
            transform.parent.position = Vector3.Lerp(pontoA.position, pontoB.position, animation_t);
            transform.parent.rotation = Quaternion.Lerp(pontoA.rotation, pontoB.rotation* Quaternion.Euler(0, 0, 180), animation_t);

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
