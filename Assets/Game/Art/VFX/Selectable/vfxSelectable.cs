using UnityEngine;

public class vfxOutline : MonoBehaviour
{
    int originalLayer;
    public void Active()
    {
        originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Outline");
    }

    public void Desactive()
    {
        if(originalLayer == null) Debug.Log("The outline is already deactivated");
        else   gameObject.layer = originalLayer;
    }
}