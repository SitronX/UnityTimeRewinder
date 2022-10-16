using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    /// <summary>
    /// Action is not meant to be used by users. It shares data between classes. You probably want to use prepared methods like: RewindTimeBySeconds(), StartRewindTimeBySeconds(), SetTimeSecondsInRewind(), StopRewindTimeBySeconds()
    /// </summary>
    public Action<float> RewindTimeCall { get; set; }
    /// <summary>
    /// Action is not meant to be used by users. It shares data between classes. You probably want to use prepared methods like: RewindTimeBySeconds(), StartRewindTimeBySeconds(), SetTimeSecondsInRewind(), StopRewindTimeBySeconds()
    /// </summary>
    public Action<bool> TrackingStateCall { get; set; }
    /// <summary>
    /// Action is not meant to be used by users. It shares data between classes. You probably want to use prepared methods like: RewindTimeBySeconds(), StartRewindTimeBySeconds(), SetTimeSecondsInRewind(), StopRewindTimeBySeconds()
    /// </summary>
    public Action<float> RestoreBuffers { get; set; }



    bool rewinding = false;
    float rewindSeconds = 0;


    /// <summary>
    /// Variable defining how much into the past should be tracked, after set limit is hit, old values will be overwritten in circular buffer
    /// </summary>
    public readonly float howManySecondsToTrack = 12;


    /// <summary>
    /// Call this method to rewind time by specified seconds instantly without snapshot preview
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds should object rewind to from now.</param>

    public void RewindTimeBySeconds(float seconds)
    {
        TrackingStateCall?.Invoke(false);
        RewindTimeCall?.Invoke(-seconds);
        RestoreBuffers?.Invoke(-seconds);
        TrackingStateCall?.Invoke(true);
    }
    /// <summary>
    /// Call this method if you want to start rewinding time with ability to preview snapshots. After done rewinding, StopRewindTimeBySeconds() must be called!!!. To update snapshot preview between, call method SetTimeSecondsInRewind()
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds before should the rewind preview rewind to</param>
    /// <returns></returns>
    public void StartRewindTimeBySeconds(float seconds)
    {
        rewindSeconds = seconds;
        TrackingStateCall?.Invoke(false);
        rewinding = true;
    }
    private void FixedUpdate()
    {
        if (rewinding)
        {
            RewindTimeCall?.Invoke(-rewindSeconds);
        }
    }
  
    /// <summary>
    /// Call this method to update rewind preview while rewind is active (StartRewindTimeBySeconds() method was called before)
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds should the rewind preview move to</param>
    public void SetTimeSecondsInRewind(float seconds)
    {
        rewindSeconds = seconds;
    }
    /// <summary>
    /// Call this method to stop previewing rewind state and effectively set current time to the rewind state
    /// </summary>
    public void StopRewindTimeBySeconds()
    {
        rewinding = false;
        RestoreBuffers?.Invoke(-rewindSeconds);
        TrackingStateCall?.Invoke(true);
    }
}
