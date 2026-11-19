
using UnityEngine;

public class demoteVFX : MonoBehaviour
{
    public ParticleSystem particle_main, particle_small;

    public void Active()
    {
        particle_main.Play();
        particle_small.Play();
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Q))
    //    {
    //        Active();
    //        Debug.Log("AA");
    //    }
    //}
}
