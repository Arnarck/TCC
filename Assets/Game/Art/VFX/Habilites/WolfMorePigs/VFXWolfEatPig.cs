using UnityEngine;

public class VFXWolfEatPig : MonoBehaviour
{
    public GameObject obj;
    public GameObject spawn;
    void PlayNextAnimation()
    {
        obj = Instantiate(obj, spawn.transform.position, transform.rotation);
    }

    public void DestroyThis()
    {
        Destroy(obj.gameObject);
    }
    

}
