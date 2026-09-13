using UnityEngine;

public class CameraMenu : MonoBehaviour
{
    public float velocidade = 10f;

    void Update()
    {
        transform.Rotate(0, velocidade * Time.deltaTime, 0);
    }
}