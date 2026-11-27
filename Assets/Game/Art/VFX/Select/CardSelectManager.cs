using UnityEngine;
using System.Collections.Generic;

public class CardSelectManager : MonoBehaviour
{
    public static CardSelectManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    int objetosOcupados = 0;

    public List<Transform> transforms = new List<Transform>();

    public Transform GetPoint()
    {
        return transforms[objetosOcupados];
    }
    public void Add()
    {
        objetosOcupados++;
    }
    public void Remove()
    {
        objetosOcupados--;
    }
}
