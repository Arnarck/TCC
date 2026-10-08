
using UnityEngine;

public class vfxDistribute : MonoBehaviour
{
    bool active = false;
    Transform pointA, pointB;
    float animation_t;
    public float velocidade = 5.0f;
    float speed = 5f;

    public GameObject card;
    public void Active(Transform pointA, Transform pointB)
    {
        active = true;

        this.pointA = pointA;
        this.pointB = pointB;
    }
    public void Active(Transform pointA, Transform pointB, float speed)
    {
        active = true;

        this.pointA = pointA;
        this.pointB = pointB;
        this.speed = speed;
    }

    void Update()
    {
        if(active)
        {
             if (animation_t < 1f)
            {
                animation_t += (Time.deltaTime*speed);
                if (animation_t >= 1f)
                {
                    animation_t = 1f;
                }
            }

            card.transform.position = Vector3.Lerp(pointA.position, pointB.position, animation_t );
            card.transform.rotation = Quaternion.Lerp(pointA.rotation, pointB.rotation, animation_t );

            if (animation_t >= 1f)
            {
                speed = velocidade;
                animation_t = 0;
                active = false;
            }
        }
    }
}

