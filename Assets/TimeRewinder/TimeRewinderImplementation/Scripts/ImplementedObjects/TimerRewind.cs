using UnityEngine;


//This script is showing setup of defaulty implemented tracking solutions (eg. tracking particles, audio...) in combination with custom variable tracking.
public class TimerRewind : RewindAbstract
{
    CircularBuffer<float> trackedTime;     //For storing data, use this CircularBuffer class
    [SerializeField] ParticleTimer particleTimer;
    
    [SerializeField] ParticlesSetting particleSettings;

    private void Start()
    {
        trackedTime = new CircularBuffer<float>();  //Circular buffer must be initialized in start method, it cannot use field initialization
        InitializeParticles(particleSettings);      //When choosing to track particles in custom tracking script, you need to first initialize these particles in start method
    }


    //In this method define what will be tracked. In our case we want to track already implemented audio tracking,particle tracking + new custom timer tracking
    protected override void Track()
    {
        TrackParticles();
        TrackAudio();
        TrackTimer();
    }

    //In this method define, what will be restored on time rewinding. In our case we want to restore Particles, Audio and custom implemented Timer
    protected override void Rewind(float seconds)
    {
        RestoreParticles(seconds);
        RestoreAudio(seconds);
        RestoreTimer(seconds);
    }


    // This is an example of custom variable tracking
    public void TrackTimer()
    {
        trackedTime.WriteLastValue(particleTimer.CurrentTimer);
    }


    // This is an example of custom variable restoring
    public void RestoreTimer(float seconds)
    {
        float rewindValue= trackedTime.ReadFromBuffer(seconds);
        particleTimer.CurrentTimer = rewindValue;
        particleTimer.SetText(rewindValue);
    }
}
