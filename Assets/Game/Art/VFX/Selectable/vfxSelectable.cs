using UnityEngine;

public class vfxOutline : MonoBehaviour
{
    int originalLayer;
    bool active = false;
    public void OnOff()
    {
        if(!active){
          Active();
          active = true;  
        }
        else
        {
            Desactive();
            active = false;
        }
    }
    public void Active()
    {
        originalLayer = gameObject.layer;
        SetLayer(transform, LayerMask.NameToLayer("Outline"));
    }

    public void Desactive()
    {
        if(originalLayer == null) Debug.Log("The outline is already deactivated");
        else   SetLayer(transform, originalLayer);
    }

    private void SetLayer(Transform current, int layer)
    {
        current.gameObject.layer = layer;

        foreach (Transform child in current)
        {
            SetLayer(child, layer);
        }
    }
}