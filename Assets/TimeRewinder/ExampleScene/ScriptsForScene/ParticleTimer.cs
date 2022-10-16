using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleTimer : MonoBehaviour
{
    [SerializeField] Text timeText;
    [SerializeField] GameObject particles;
    float timerDefault = 5;    
    public float CurrentTimer { get; set; }
    private void Start()
    {
        CurrentTimer = timerDefault;
    }
    void FixedUpdate()
    {
        CurrentTimer -= Time.deltaTime;
        timeText.text = "Time to disable/enable particles: " + CurrentTimer.ToString("0.0");
        if(CurrentTimer < 0)
        {
            particles.SetActive(!particles.activeSelf);
            CurrentTimer = timerDefault;
        }
    }
    public void SetText(float value)
    {
        timeText.text = "Time to disable/enable particles: " + value.ToString("0.0");
    }
}
