
using UnityEngine;
using System.Collections;
using UnityEngine.VFX;


public class ActiveCard : MonoBehaviour
{
    public Animator anim;
    public VisualEffect vfx;

    Vector3 pontoB, startPosition;
    public bool active = false;
    public float delay = 2;
    public float velocidade = 2;
    float animation_t = 0;

    //public GameObject teste;
    //void Start()
    //{
    //    pontoB = teste.transform.position;
    //    startPosition = transform.position;
    //}

    public void Active(Vector3 pos)
    {
        pontoB = pos;
        startPosition = transform.position;
        active = true;
    }
    private void ActiveVFX()
    {
        vfx.Play();
    }
     public void Update()
    {
        if(active)
        {
            if (animation_t < 1f)
            {
                animation_t += Time.deltaTime*velocidade;
                if (animation_t >= 1f)
                {
                    animation_t = 1f;
                    StartCoroutine(WaitForDelay());
                }
            }
            transform.parent.position = Vector3.Lerp(startPosition, pontoB, animation_t);
        }
        
    }
    IEnumerator WaitForDelay()
    {
        yield return new WaitForSeconds(delay);
        active = false;
        anim.SetTrigger("Active");
        
    }
}
