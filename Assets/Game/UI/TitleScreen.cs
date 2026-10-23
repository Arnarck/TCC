using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public GameObject painelTitle;
    public GameObject painelMenu;
    public GameObject painelAnimation;
    public Image telaFade;

    public float duracaoFade = 0.6f;

    private bool emTransicao;

    IEnumerator Start()
    {
        emTransicao = true;

        painelTitle.SetActive(true);
        painelMenu.SetActive(false);

        // Começa preto e revela a title screen.
        yield return FazerFade(1f, 0f);

        emTransicao = false;
    }

    void Update()
    {
        if (!emTransicao && Input.anyKeyDown)
        {
            StartCoroutine(AbrirMenu());
        }
    }

    IEnumerator AbrirMenu()
    {
        emTransicao = true;

        // Escurece a tela.
        yield return FazerFade(0f, 1f);

        // Abre o menu enquanto tudo está preto.
        painelMenu.SetActive(true);
        painelAnimation.SetActive(true);
        painelTitle.SetActive(false);

        // Aguarda a câmera mudar de posição.
        yield return null;

        // Revela o menu.
        yield return FazerFade(1f, 0f);
    }

    IEnumerator FazerFade(float inicio, float fim)
    {
        float tempo = 0f;
        float duracao = Mathf.Max(0.01f, duracaoFade);

        Color cor = Color.black;
        cor.a = inicio;
        telaFade.color = cor;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            cor.a = Mathf.Lerp(inicio, fim, tempo / duracao);
            telaFade.color = cor;

            yield return null;
        }

        cor.a = fim;
        telaFade.color = cor;
    }
}