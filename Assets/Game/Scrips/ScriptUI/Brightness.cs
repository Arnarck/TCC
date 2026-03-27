using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class ExposureController : MonoBehaviour
{
    public RawImage brightnessImage;

    [SerializeField] private Slider exposureSlider;
    [SerializeField] private Volume postProcessingVolume;
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        float valorBrilho = PlayerPrefs.GetFloat("exposureSlider", 0f); 
        exposureSlider.value = valorBrilho;

        if (postProcessingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.postExposure.value = valorBrilho;
            exposureSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        AlterarBrilho(exposureSlider.value);
    }

    void OnSliderValueChanged(float value)
    {
       
            colorAdjustments.postExposure.value = value;
            AlterarBrilho(value);
            PlayerPrefs.SetFloat("exposureSlider", value);
        
       
    }

    public void AlterarBrilho(float value)
    {
        float transparency = Mathf.Abs(value) / 5.0f;

        Color novaCor;
        if (value < 0)
        {
            novaCor = new Color(0, 0, 0, transparency);
        }
        else
        {
            novaCor = new Color(1, 1, 1, transparency);
        }

        brightnessImage.color = novaCor;
    }

    private int MapValueToText(float value)
    {
        return Mathf.RoundToInt((value + 5) / 10.0f * 100);
    }
}
