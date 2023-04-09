using System;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// In latest version, Scale tracking/rewind was added defaultly to GenericRewind alongside with tracking position and rotation. Script is still here to only show how it internally works in the simplest way possible
/// </summary>
[Obsolete]
//This script is showing setup of simple custom variable tracking
public class ScaleRewind : RewindAbstract           
{
    [SerializeField] Slider scaleSlider;

    CircularBuffer<Vector3> trackedObjectScales;       //For storing data, use this CircularBuffer class

    private void Start()
    {
        trackedObjectScales = new CircularBuffer<Vector3>();        //Circular buffer must be initialized in start method, it cannot use field initialization
    }

    //In this method define what will be tracked. In our case we want only track our custom added variable scale tracking
    protected override void Track()
    {
        TrackObjectScale();      
    }


    //In this method define, what will be restored on time rewinding. In our case we want to restore object scale
    protected override void Rewind(float seconds)
    {
        RestoreObjectScale(seconds);
    }


    // This is an example of custom variable tracking
    public void TrackObjectScale()
    {
        trackedObjectScales.WriteLastValue(transform.localScale);
    }

    
    // This is an example of custom variable restoring
    public void RestoreObjectScale(float seconds)
    {
        transform.localScale = trackedObjectScales.ReadFromBuffer(seconds);

        //While we are at it, we can also additionaly restore slider value to match the object scale 
        scaleSlider.value = transform.localScale.x;
    }

    //Just for demonstration object scaling was chosen, otherwise it would be probably better to track and restore slider value, which would then also update the object scale accordingly
}
