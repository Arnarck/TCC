
using UnityEngine;

public class CardSelectVFX : MonoBehaviour
{
    public vfxMove move;

    private Vector3 pos_card_in_hand;
    private Quaternion rot_card_in_hand;

    public void Select()
    {
        pos_card_in_hand = transform.position;
        rot_card_in_hand = transform.rotation;

        
        Transform pos = CardSelectManager.Instance.GetPoint();
        CardSelectManager.Instance.Add();

        move.pontoA = gameObject.transform;
        move.pontoB = pos;

        move.Active();
    }

    public void Deselect()
    {
        move.pontoA = gameObject.transform;
 
        move.pontoB.position = pos_card_in_hand;
        move.pontoB.rotation = rot_card_in_hand;

        CardSelectManager.Instance.Remove();

        move.Active();
    }

    

}
