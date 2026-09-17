using UnityEngine;

public class vfxDistribute : MonoBehaviour
{
    bool active = false;
    Transform pointA, pointB;
    float animation_t;
    public float velocidade = 5.0f;
    public GameObject card;
    public void Active(Transform pointA, Transform pointB)
    {
        active = true;

        this.pointA = pointA;
        this.pointB = pointB;
    }

    void Update()
    {
        if(active)
        {
             if (animation_t < 1f)
            {
                animation_t += Time.deltaTime*velocidade;
                if (animation_t >= 1f)
                {
                    animation_t = 1f;
                }
            }

            card.transform.position = Vector3.Lerp(pointA.position, pointB.position, animation_t);

            if (animation_t >= 1f)
            {
                active = false;
            }
        }
    }
}

