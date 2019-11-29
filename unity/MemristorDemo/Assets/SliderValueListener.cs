using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SliderValueListener : MonoBehaviour
{
    public enum SliderType
    {
        MinSlider,
        MaxSlider
    }
    public SliderType type = SliderType.MinSlider;
    TextMeshProUGUI label;

    private void Awake()
    {
        label = this.GetComponent<TextMeshProUGUI>();
    }

    public void UpdateValue(float min, float max)
    {
        if (type == SliderType.MinSlider)
            label.text = min.ToString();
        else
            label.text = max.ToString();

        OutputController.LowerLimitState = (int)min;
        OutputController.UpperLimitState = (int)max;
    }
}
