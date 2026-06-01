
using UnityEngine;
using System.Collections.Generic;

public class vfxOutlineConfig : MonoBehaviour, iVFX
{
    public GameObject obj;
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.U))
    //    {
    //        Active();
    //    }
    //    if (Input.GetKeyDown(KeyCode.I))
    //    {
    //        Deactivate();
    //    }
    //}
    public void Active()
    {

        obj.SetActive(true);
    }

    // Update is called once per frame
    public void Deactivate()
    {
        obj.SetActive(false);
    }
}
