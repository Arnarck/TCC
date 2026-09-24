
using UnityEngine;
using UnityEngine.Splines;

public class vfxCreate : MonoBehaviour
{
    public SplineAnimate splineAnimate;
    public float maxSpeed = 5.0f;
    
    
    public void Active(SplineContainer spline, Transform pos_card_in_hand)
    {
        BezierKnot last_knot = spline.Spline[spline.Spline.Count - 1];
        
        last_knot.Position = pos_card_in_hand.position;
        spline.Spline.SetKnot(spline.Spline.Count - 1, last_knot);

        splineAnimate.MaxSpeed = maxSpeed;

        splineAnimate.SetTarget(spline);  //@VITOR mexemo no Spline animation voltar la pra ver se deu BO quando for testar.
        splineAnimate.Play();
       
    }
}
