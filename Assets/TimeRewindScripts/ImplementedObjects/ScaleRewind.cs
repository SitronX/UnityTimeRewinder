using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//This script is showing setup of defaulty implemented tracking solutions (eg. tracking particles, audio...) in combination with custom variable tracking.
public class ScaleRewind : RewindAbstract           
{
    [SerializeField] Slider scaleSlider;

    CircularBuffer<Vector3> trackedObjectScales;

    [SerializeField] ParticlesSetting particleSettings;

    private void Start()
    {
        //InitializeParticles(particleSettings);      //When choosing to track particles in custom tracking script, you need to first initialize these particles in start method
    }

    //As stated in RewindAbstract class, this method must be filled with circular buffer that is used for custom variable tracking
    //Method must contain implementation of reset of the circullar buffer to work correctly
    protected override void AdditionalResets()
    {
        trackedObjectScales = new CircularBuffer<Vector3>(howManyItemsFit);     //For time rewinding purposes, give the CircularBuffer constructor this variable (howManyItemsFit)
    }


    //This method must be filled with circular buffer that is used for custom variable tracking
    //After rewinding the time, values that were previously stored in buffer are obsolete. 
    //This method must contain implementation of moving bufferPosition for next write to correct position in regards to time rewind
    protected override void AdditionalRestores(float timeStepMove)
    {
        trackedObjectScales.MoveLastBufferPosition(timeStepMove * howManyRecordsPerSecond);  //For time rewinding purposes, use these attributes (timeStepMove*howManyRecordsPerSecond)
    }

  

    //In this method define what will be tracked. In our case we want to track already implemented audio tracking,particle tracking + new custom added variable scale tracking
    protected override void Track()
    {
        TrackParticles();
        TrackAudio();
        TrackObjectScale();
        
    }




    //In this method define, what will be restored on time rewinding. In our case we want to restore previous audio state, particles + object scale state
    protected override void GetSnapshotFromSavedValues(float timestepMove)
    {
        float position = timestepMove * howManyRecordsPerSecond;        //For time rewinding purposes, use this calculation to restore correct position value (timeStepMove*howManyRecordsPerSecond)

        RestoreParticles(position);
        RestoreAudio(position);
        RestoreObjectScale(position);
    }




    // This is an example of custom tracking variable
    public void TrackObjectScale()
    {
        trackedObjectScales.WriteLastValue(transform.localScale);
    }


    
    // This is an example of custom variable restoring
    public void RestoreObjectScale(float position)
    {
        transform.localScale = trackedObjectScales.ReadFromBuffer(position);

        //While we are at it, we can also additionaly restore slider value to match the object scale 
        scaleSlider.value = transform.localScale.x;
    }

    //Just for demonstration object scaling was chosen, otherwise it would be probably better to track and restore slider value, which would then also update the object scale accordingly)  
}
