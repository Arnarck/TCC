using UnityEngine;

public class promoteVFX : MonoBehaviour
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
    //    }
    //}
}
