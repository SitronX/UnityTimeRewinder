using System;
using UnityEngine;
using UnityEngine.UI;

public class ScaleChanger : MonoBehaviour
{
    [SerializeField] Text textToUpdate;
    [SerializeField] Slider slider;

    public void OnSliderChange(Single value)
    {
        transform.localScale = new Vector3(value, value, value);
        textToUpdate.text = value.ToString("0.00");
    }

}
