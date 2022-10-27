using UnityEngine;
using UnityEngine.UI;

public class ParticleTimer : MonoBehaviour
{
    [SerializeField] Text timeText;
    [SerializeField] GameObject particles;
    RewindManager rewindManager;
    float timerDefault = 5;    
    public float CurrentTimer { get; set; }
    private void Start()
    {
        CurrentTimer = timerDefault;
        rewindManager = FindObjectOfType<RewindManager>();
    }
    void Update()                               
    {
        if(rewindManager.IsBeingRewinded)                       //Simple solution how to solve Update fighting with FixedUpdate in rewind
            return;
        

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
