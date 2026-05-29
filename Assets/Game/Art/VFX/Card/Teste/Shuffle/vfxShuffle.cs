
using UnityEngine;
using System.Collections;

public class vfxShuffle : MonoBehaviour
{
    public Animator anim;

    private Vector3 pontoB;
    private bool indoParaB = true;
    private float animation_t = 0;
    private float velocidade = 2f;
    private float delay;
    private Vector3 startPosition;
    bool active = false, end = false;
    public void Active( GameObject pos, float delay)
    {
        pontoB = pos.transform.position;
        this.delay = delay;
        startPosition = transform.position;
        active = true;
        StartCoroutine(WaitForDelay());
    }
    int index;
    public void Active( Vector3 pos, float delay, int index)
    {
        this.index = index;
        pontoB = pos;
        this.delay = delay;
        startPosition = transform.position;
        active = true;
        StartCoroutine(WaitForDelay());
    }

    public void Update()
    {
        if(active)
        {
           
            if (end)
            {
                if (animation_t > 0f)
                {
                    animation_t -= Time.deltaTime*velocidade;
                    if (animation_t <= 0f)
                    {
                        animation_t = 0f;
                        active = false;
                        end = false;
                    }
                }
            }
            else
            {
                if (animation_t < 1f)
                {
                    animation_t += Time.deltaTime*velocidade;
                    if (animation_t >= 1f)
                    {
                        animation_t = 1f;
                    }
                }
            }
            transform.parent.position = Vector3.Lerp(startPosition, pontoB, animation_t);

            
        }
    }
    IEnumerator WaitForDelay()
    {
        yield return new WaitForSeconds(2);
        if(index%2 == 0)
        {
            anim.SetTrigger("Shuffle.L");
            Debug.Log("Shuffle");
        }
        else anim.SetTrigger("Shuffle.R");

        yield return new WaitForSeconds(delay);
        end = true;
    }
}
