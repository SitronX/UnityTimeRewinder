using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static RewindAbstract;

//This script is showing setup of defaulty implemented tracking solutions (eg. tracking particles, audio...) in combination with custom variable tracking.
public class TimerRewind : RewindAbstract
{
    CircularBuffer<float> trackedTime;              //For storing data, use this CircularBuffer class
    [SerializeField] ParticleTimer particleTimer;
    
    [SerializeField] ParticlesSetting particleSettings;

    private void Start()
    {
        InitializeParticles(particleSettings);      //When choosing to track particles in custom tracking script, you need to first initialize these particles in start method
    }

    //As stated in RewindAbstract class, this method must be filled with circular buffer that is used for custom variable tracking
    //Method must contain implementation of reset of the circullar buffer to work correctly
    protected override void AdditionalResets()
    {
        trackedTime = new CircularBuffer<float>(howManyItemsFit);                //For time rewinding purposes, give the CircularBuffer constructor this variable (howManyItemsFit)
    }



    //After rewinding the time, values that were previously stored in buffer are obsolete. 
    //This method must contain implementation of moving bufferPosition for next write to correct position in regards to time rewind
    protected override void AdditionalRestores(float seconds)
    {
        trackedTime.MoveLastBufferPosition(seconds * howManyRecordsPerSecond);  //For time rewinding purposes, use these attributes (seconds*howManyRecordsPerSecond)
    }

    //In this method define what will be tracked. In our case we want to track already implemented audio tracking,particle tracking + new custom timer tracking
    protected override void Track()
    {
        TrackParticles();
        TrackAudio();
        TrackTimer();
    }

    //In this method define, what will be restored on time rewinding. In our case we want to restore Particles, Audio and custom implemented Timer
    protected override void GetSnapshotFromSavedValues(float seconds)
    {
        float position = seconds * howManyRecordsPerSecond;
        RestoreParticles(position);
        RestoreAudio(position);
        RestoreTimer(position);
    }



    // This is an example of custom variable tracking
    public void TrackTimer()
    {
        trackedTime.WriteLastValue(particleTimer.CurrentTimer);
    }



    // This is an example of custom variable restoring
    public void RestoreTimer(float position)
    {
        float rewindValue= trackedTime.ReadFromBuffer(position);
        particleTimer.CurrentTimer = trackedTime.ReadFromBuffer(position);
        particleTimer.SetText(rewindValue);
    }
}
