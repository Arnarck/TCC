using UnityEngine;

public class AbilityTearUPaCard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject torn_card;
    public GameObject myFather;

    public void Active()
    {
        gameObject.GetComponent<Animator>().SetTrigger("Ability_DestroyCard");
    }
    private void Start_Torn_Animate()
    {
        Quaternion rotacao = myFather.transform.rotation * Quaternion.Euler(90, 0, 0);
        torn_card = Instantiate(torn_card, myFather.transform.position, rotacao);
    }

    private void DestroyThisObj()
    {
        Destroy(torn_card);
        Destroy(myFather);

    }
}
