using UnityEngine;
using System.Collections.Generic;

public class TesteShuffle : MonoBehaviour
{
    public GameObject obj;
    public float delay;
    public List<GameObject> listCards = new List<GameObject>();

    public Vector3 offSet;
    public void Active()
    {
        for(int i = 0; i < listCards.Count; i++)
        {
            Vector3 pos = obj.transform.position;
            //float iDelay = delay * i;
            pos += (offSet * i);
            Debug.Log(offSet);
            listCards[i].GetComponentInChildren<vfxShuffle>().Active(pos , delay, i);//iDelay);
        }
    } 
}
