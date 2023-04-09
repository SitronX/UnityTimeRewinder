using System;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    /// <summary>
    /// Action is not meant to be used by users. It shares data between classes. You probably want to use prepared methods like: RewindTimeBySeconds(), StartRewindTimeBySeconds(), SetTimeSecondsInRewind(), StopRewindTimeBySeconds()
    /// </summary>
    public static Action<float> RewindTimeCall { get; set; }
    /// <summary>
    /// Action is not meant to be used by users. It shares data between classes. You probably want to use prepared methods like: RewindTimeBySeconds(), StartRewindTimeBySeconds(), SetTimeSecondsInRewind(), StopRewindTimeBySeconds()
    /// </summary>
    public static Action<bool> TrackingStateCall { get; set; }
    /// <summary>
    /// Action is not meant to be used by users. It shares data between classes. You probably want to use prepared methods like: RewindTimeBySeconds(), StartRewindTimeBySeconds(), SetTimeSecondsInRewind(), StopRewindTimeBySeconds()
    /// </summary>
    public static Action<float> RestoreBuffers { get; set; }
    
    
    /// <summary>
    /// This property returns how many seconds are available for rewind
    /// </summary>
    public float HowManySecondsAvailableForRewind { get; private set; }


    /// <summary>
    /// Tells you if scene is currently being rewinded
    /// </summary>
    public bool IsBeingRewinded { get; private set; } = false;


    float rewindSeconds = 0;

    /// <summary>
    /// Variable defining how much into the past should be tracked, after set limit is hit, old values will be overwritten in circular buffer
    /// </summary>
    public static readonly float howManySecondsToTrack = 12;


    /// <summary>
    /// Call this method to rewind time by specified seconds instantly without snapshot preview
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds should object rewind to from now (Parameter must be >=0).</param>
    public void RewindTimeBySeconds(float seconds)
    {
        if(seconds>HowManySecondsAvailableForRewind)
        {
            Debug.LogError("Not enough stored tracked value!!! Reaching on wrong index. Called rewind should be less than HowManySecondsAvailableForRewind property");
            return;
        }
        if(seconds<0)
        {
            Debug.LogError("Parameter in RewindTimeBySeconds() must have positive value!!!");
            return;
        }
        TrackingStateCall?.Invoke(false);
        RewindTimeCall?.Invoke(seconds);
        RestoreBuffers?.Invoke(seconds);
        TrackingStateCall?.Invoke(true);
    }
    /// <summary>
    /// Call this method if you want to start rewinding time with ability to preview snapshots. After done rewinding, StopRewindTimeBySeconds() must be called!!!. To update snapshot preview between, call method SetTimeSecondsInRewind()
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds before should the rewind preview rewind to (Parameter must be >=0)</param>
    /// <returns></returns>
    public void StartRewindTimeBySeconds(float seconds)
    {
        CheckReachingOutOfBounds(seconds);

        rewindSeconds = seconds;
        TrackingStateCall?.Invoke(false);
        IsBeingRewinded = true;
    }

    /// <summary>
    /// Call this method to update rewind preview while rewind is active (StartRewindTimeBySeconds() method was called before)
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds should the rewind preview move to (Parameter must be >=0)</param>
    public void SetTimeSecondsInRewind(float seconds)
    {
        CheckReachingOutOfBounds(seconds);
        rewindSeconds = seconds;
    }
    /// <summary>
    /// Call this method to stop previewing rewind state and effectively set current values to the rewind state
    /// </summary>
    public void StopRewindTimeBySeconds()
    {
        HowManySecondsAvailableForRewind -= rewindSeconds;
        IsBeingRewinded = false;
        RestoreBuffers?.Invoke(rewindSeconds);
        TrackingStateCall?.Invoke(true);
    }
    private void CheckReachingOutOfBounds(float seconds)
    {
        if (seconds > HowManySecondsAvailableForRewind)
        {
            Debug.LogError("Not enough stored tracked value!!! Reaching on wrong index. Called rewind should be less than HowManySecondsAvailableForRewind property");
            return;
        }
        if (seconds < 0)
        {
            Debug.LogError("Parameter in StartRewindTimeBySeconds() must have positive value!!!");
            return;
        }
    }
   
    private void Awake()
    {
        RewindManager[] managers = FindObjectsOfType<RewindManager>();

        if (managers.Length > 1)                                               //Check if each scene contains only one script with RewindManager
        {
            Debug.LogError("RewindManager cannot be more than once in each scene. Remove the other RewindManager!!!");
        }
    }
    private void OnEnable()
    {
        HowManySecondsAvailableForRewind = 0;
    }
    private void FixedUpdate()
    {
        if (IsBeingRewinded)
        {
            RewindTimeCall?.Invoke(rewindSeconds);
        }
        else if (HowManySecondsAvailableForRewind != howManySecondsToTrack)
        {
            HowManySecondsAvailableForRewind += Time.fixedDeltaTime;

            if (HowManySecondsAvailableForRewind > howManySecondsToTrack)
                HowManySecondsAvailableForRewind = howManySecondsToTrack;
        }
    }
}
