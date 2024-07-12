using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
 
    /// <summary>
    /// Property defining how much into the past should be tracked. 
    /// You can edit this value to your preference
    /// </summary>
    [field:SerializeField] public float HowManySecondsToTrack { get; private set; } = 12;

    /// <summary>
    /// This property returns how many seconds are currently available for rewind
    /// </summary>
    public float HowManySecondsAvailableForRewind { get; private set; }

    /// <summary>
    /// Tells you if scene is currently being rewinded
    /// </summary>
    public bool IsBeingRewinded { get; private set; } = false;

    /// <summary>
    /// Singleton instance of RewindManager
    /// </summary>
    public static RewindManager Instance { get; private set; }

    /// <summary>
    /// Property defining if Circular buffers should be written to, when the system is not rewinding and should normally be tracking values.
    /// </summary>
    public bool TrackingEnabled { get; set; } = true;


    float rewindSeconds = 0;
    List<RewindAbstract> _rewindedObjects;
    List<Tuple<RewindAbstract, OutOfBoundsBehaviour>> _lateAddedRewindedObjects = new List<Tuple<RewindAbstract, OutOfBoundsBehaviour>>();

    /// <summary>
    /// Call this method to rewind time by specified seconds instantly without snapshot preview. Usefull for one time instant rewinds.
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds should object rewind to from now (Parameter must be >=0).</param>
    public void InstantRewindTimeBySeconds(float seconds)
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
        _rewindedObjects.ForEach(x => x.Rewind(seconds));
        _lateAddedRewindedObjects.ForEach(x =>
        {
            x.Item1.Rewind(rewindSeconds);

            if (x.Item1.IsManagerTrackingActiveState)
                x.Item1.RestoreObjectActiveState(rewindSeconds);
        });

        HowManySecondsAvailableForRewind -= seconds;
        BuffersRestore?.Invoke(seconds);
    }
    /// <summary>
    /// Call this method if you want to start rewinding time with ability to preview snapshots. After done rewinding, StopRewindTimeBySeconds() must be called!!!. To update snapshot preview between, call method SetTimeSecondsInRewind().
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds before should the rewind preview rewind to (Parameter must be >=0)</param>
    /// <returns></returns>
    public void StartRewindTimeBySeconds(float seconds)
    {
        if (IsBeingRewinded)
            Debug.LogError("The previous rewind must be stopped by calling StopRewindTimeBySeconds() before you start another rewind");
        
        CheckReachingOutOfBounds(seconds);

        rewindSeconds = seconds;
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
    /// Call this method to stop previewing rewind state and resume normal game flow
    /// </summary>
    public void StopRewindTimeBySeconds()
    {
        if (!IsBeingRewinded)
            Debug.LogError("Rewind must be started before you try to stop it. StartRewindTimeBySeconds() must be called first");

        HowManySecondsAvailableForRewind -= rewindSeconds;
        BuffersRestore?.Invoke(rewindSeconds);
        IsBeingRewinded = false;
    }
    /// <summary>
    /// Call if you want to restart the whole tracking system
    /// </summary>
    public void RestartTracking()
    {
        if (IsBeingRewinded)
            StopRewindTimeBySeconds();

        HowManySecondsAvailableForRewind = 0;
        TrackingEnabled = true;
    }
    private void CheckReachingOutOfBounds(float seconds)
    {
        if (Mathf.Round(seconds*100) > Mathf.Round(HowManySecondsAvailableForRewind*100))
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
    public enum OutOfBoundsBehaviour
    {
        /// <summary>
        /// Only disables the object when out of bounds
        /// </summary>
        Disable,
        /// <summary>
        /// Disables the object when out of bounds. Then, if the game resumes in out of bound state, the object is destroyed
        /// </summary>
        DisableDestroy
    }
    /// <summary>
    /// Lets you add object to track pool even when game is already running.
    /// </summary>
    /// <param name="objectToRewind"></param>
    /// <param name="automaticDestroy">When you rewind before this object is added for tracking, you can choose if object gets disabled or is detroyed when resuming the game.</param>
    public void AddObjectForTracking(RewindAbstract objectToRewind, OutOfBoundsBehaviour outOfBoundsBehaviour)
    {
        if (!_lateAddedRewindedObjects.Any(x => x.Item1 == objectToRewind))
        {
            objectToRewind.MainInit();
            _lateAddedRewindedObjects.Add(new Tuple<RewindAbstract, OutOfBoundsBehaviour>(objectToRewind, outOfBoundsBehaviour));
        }
    }
    private void Awake()
    {
        _rewindedObjects = FindObjectsOfType<RewindAbstract>().ToList();

        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;

        _rewindedObjects.ForEach(x => x.MainInit());
    }
    private void OnEnable()
    {
        HowManySecondsAvailableForRewind = 0;
    }
    private  void FixedUpdate()
    {   
        if (IsBeingRewinded)
        {
            _rewindedObjects.ForEach(x => x.Rewind(rewindSeconds));

            _lateAddedRewindedObjects.ForEach(x =>
            {
                x.Item1.Rewind(rewindSeconds);

                if (x.Item1.IsManagerTrackingActiveState)
                    x.Item1.RestoreObjectActiveState(rewindSeconds);
            });
        }
        else 
        {
            _rewindedObjects.ForEach(x => x.Track());

            for(int i=0;i<_lateAddedRewindedObjects.Count;i++)
            {
                if (_lateAddedRewindedObjects[i].Item1 == null)
                {
                    _lateAddedRewindedObjects.Remove(_lateAddedRewindedObjects[i]);
                    continue;
                }

                if (!_lateAddedRewindedObjects[i].Item1.isActiveAndEnabled&& _lateAddedRewindedObjects[i].Item2==OutOfBoundsBehaviour.DisableDestroy)
                {
                    RewindAbstract objectToDestroy = _lateAddedRewindedObjects[i].Item1;
                    _lateAddedRewindedObjects.Remove(_lateAddedRewindedObjects[i]);
                    Destroy(objectToDestroy.gameObject);
                    continue;
                }

                _lateAddedRewindedObjects[i].Item1.Track();

                if (_lateAddedRewindedObjects[i].Item1.IsManagerTrackingActiveState)
                    _lateAddedRewindedObjects[i].Item1.TrackObjectActiveState();
                else if (!_lateAddedRewindedObjects[i].Item1.IsActiveStateTracked)
                {
                    _lateAddedRewindedObjects[i].Item1.IsManagerTrackingActiveState = true;
                    _lateAddedRewindedObjects[i].Item1.TrackObjectActiveState();
                }
            }
  
            if(TrackingEnabled)
                HowManySecondsAvailableForRewind = Mathf.Min(HowManySecondsAvailableForRewind + Time.fixedDeltaTime, HowManySecondsToTrack);
        }
    }

    /// <summary>
    /// This action is not meant to be used by users. CircularBuffers listens to it
    /// </summary>
    public static Action<float> BuffersRestore { get; set; }
}
