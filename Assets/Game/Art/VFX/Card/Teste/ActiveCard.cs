
using UnityEngine;
using System.Collections;
using UnityEngine.VFX;


public class ActiveCard : MonoBehaviour
{
    public Animator anim;
    public VisualEffect vfx;

    Vector3 startPosition, pontoBpos;
    Quaternion startRotation, pontoBrot;
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

    public void Active(Vector3 pos, Quaternion rot)
    {
        pontoBpos = pos;
        pontoBrot = rot;

        startPosition = transform.position;
        startRotation = transform.rotation;

        GetComponent<SelectCard>().Active();
        StartCoroutine(WaitForDelay());

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
                }
            }
            transform.parent.position = Vector3.Lerp(startPosition, pontoBpos, animation_t);
            transform.parent.rotation = Quaternion.Lerp(startRotation, pontoBrot, animation_t);
        }
        
    }
    IEnumerator WaitForDelay()
    {
        yield return new WaitForSeconds(delay);
        active = false;
        anim.SetTrigger("Active");
        
    }
}
