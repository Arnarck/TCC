using UnityEngine;

public class VFXWolfEatPig : MonoBehaviour
{
    public GameObject obj;
    void PlayNextAnimation()
    {
        Instantiate(obj, transform.position, transform.rotation);
    }

    

}
