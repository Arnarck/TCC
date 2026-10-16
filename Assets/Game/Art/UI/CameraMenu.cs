using UnityEngine;

public class CameraMenu : MonoBehaviour
{
    public float velocidade = 10f;

    public GameObject painelTitle;
    public Transform posicaoInicial;
    public Transform posicaoMenu;

    private bool chegouNoMenu;

    void Start()
    {
        transform.SetPositionAndRotation(
            posicaoInicial.position,
            posicaoInicial.rotation
        );
    }

    void LateUpdate()
    {
        if (chegouNoMenu)
            return;

        if (painelTitle.activeInHierarchy)
        {
            transform.Rotate(
                0f,
                velocidade * Time.deltaTime,
                0f,
                Space.World
            );
        }
        else
        {
            transform.SetPositionAndRotation(
                posicaoMenu.position,
                posicaoMenu.rotation
            );

            chegouNoMenu = true;
        }
    }
}