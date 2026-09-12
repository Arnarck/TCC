using UnityEngine;

public class BossCard : MonoBehaviour
{
    public Boss_Abilities ability_type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void destroy()
    {
        gameObject.SetActive(false);
    }
}
