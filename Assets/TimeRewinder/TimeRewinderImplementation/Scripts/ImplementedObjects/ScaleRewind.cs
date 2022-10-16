using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//This script is showing setup of simple one custom variable tracking
public class ScaleRewind : RewindAbstract           
{
    [SerializeField] Slider scaleSlider;

    CircularBuffer<Vector3> trackedObjectScales;        //For storing data, use this CircularBuffer class



    //As stated in RewindAbstract class, this method must be filled with circular buffer that is used for custom variable tracking
    //Method must contain implementation of reset of the circullar buffer to work correctly
    protected override void AdditionalResets()
    {
        trackedObjectScales = new CircularBuffer<Vector3>(howManyItemsFit);     //For time rewinding purposes, give the CircularBuffer constructor this variable (howManyItemsFit)
    }


    //After rewinding the time, values that were previously stored in buffer are obsolete. 
    //This method must contain implementation of moving bufferPosition for next write to correct position in regards to time rewind
    protected override void AdditionalRestores(float seconds)
    {
        trackedObjectScales.MoveLastBufferPosition(seconds * howManyRecordsPerSecond);  //For time rewinding purposes, use these attributes (seconds*howManyRecordsPerSecond)
    }

  

    //In this method define what will be tracked. In our case we want to track already implemented audio tracking,particle tracking + new custom added variable scale tracking
    protected override void Track()
    {
        TrackObjectScale();      
    }


    //In this method define, what will be restored on time rewinding. In our case we want to restore object scale
    protected override void GetSnapshotFromSavedValues(float seconds)
    {
        float position = seconds * howManyRecordsPerSecond;        //For time rewinding purposes, use this calculation to restore correct position value (seconds*howManyRecordsPerSecond)

        RestoreObjectScale(position);
    }



    // This is an example of custom variable tracking
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
