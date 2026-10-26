using UnityEngine;

public class VFXWolfEatPig : MonoBehaviour
{
    public GameObject obj;
    void PlayNextAnimation()
    {
        obj = Instantiate(obj, transform.position, transform.rotation);
    }

    public void DestroyThis()
    {
        Destroy(obj.gameObject);
    }
    

}
