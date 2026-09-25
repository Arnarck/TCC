using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class VFXTransform : MonoBehaviour
{
    private bool active = false;
    private Material material;
    public Material materialTransform;

    public Texture2D image; 
    private float valor_destino;

    public float valorA, valorB, tempo;
    public void Start()//Active(Texture2D image )
    {
        transform.GetComponent<SkinnedMeshRenderer>().sharedMaterial = materialTransform;
        material = transform.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        Debug.Log(material);

        material.SetFloat("_AlturaCorte", valorA);
        material.SetTexture("_NewTexture", image);

        if(!active)
        {
            active = true;
            if(material.GetFloat("_AlturaCorte") > 0)
            {
                valor_destino = valorB;
            }
            else
            {
                valor_destino = valorA;
            }
            StartCoroutine(Chavear());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(active)
        {
            material.SetFloat("_AlturaCorte", Mathf.Lerp(material.GetFloat("_AlturaCorte"), valor_destino, tempo/1000));
        }


    }
    public void OnOFf()
    {
        if(active == false)
        {
            active = true;
            if(material.GetFloat("_AlturaCorte") > 0)
            {
                valor_destino = valorB;
            }
            else
            {
                valor_destino = valorA;
            }
            StartCoroutine(Chavear());
        }
    }
    private IEnumerator Chavear()
    {
        yield return new WaitForSeconds(tempo * .8f);
        active = false;
    }
}
