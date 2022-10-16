using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.ParticleSystem;

public abstract class RewindAbstract : MonoBehaviour
{
    
    protected static float howManyRecordsPerSecond;
    protected static int howManyItemsFit;
    RewindManager rewindManager;
    float howManySecondsToTrack;
    public bool IsTracking { get; set; } = false;

    Rigidbody body;
    Animator animator;
    AudioSource audioSource;
    

    void Awake()
    {

        rewindManager = FindObjectOfType<RewindManager>();
        if (rewindManager != null)
        {
            howManySecondsToTrack = rewindManager.howManySecondsToTrack;
            howManyItemsFit = (int)(howManySecondsToTrack / Time.fixedDeltaTime);
            body = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            howManyRecordsPerSecond = 1 / Time.fixedDeltaTime;

            IsTracking = true;
        }
        else
        {
            Debug.Log("TimeManager script cannot be found in scene. Time tracking cannot be started. Did you forget to put it into the scene?");
        }
    }
   

    private void FixedUpdate()
    {
        if (IsTracking)
            Track();
    }



    #region PositionRotation

    CircularBuffer<PositionAndRotationValues> trackedPositionsAndRotation;
    public struct PositionAndRotationValues
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    
    protected void TrackPositionAndRotation()
    {
        PositionAndRotationValues valuesToWrite;
        valuesToWrite.position = transform.position;
        valuesToWrite.rotation = transform.rotation;
        trackedPositionsAndRotation.WriteLastValue(valuesToWrite);
    }
    protected void RestorePositionAndRotation(float position)
    {
        PositionAndRotationValues valuesToRead = trackedPositionsAndRotation.ReadFromBuffer(position);
        transform.SetPositionAndRotation(valuesToRead.position, valuesToRead.rotation);
    }
    #endregion

    #region Velocity
    CircularBuffer<Vector3> trackedVelocities;
    protected void TrackVelocity()
    {
        trackedVelocities.WriteLastValue(body.velocity);
    }
    protected void RestoreVelocity(float position)
    {   
        body.velocity = trackedVelocities.ReadFromBuffer(position);   
    }
    #endregion

    #region Animator
    CircularBuffer<AnimationValues> trackedAnimationTimes;
    public struct AnimationValues
    {
        public float animationStateTime;
        public int animationHash;
    }
    protected void TrackAnimator()
    {
        animator.speed = 1;

        AnimationValues valuesToWrite; 
        valuesToWrite.animationStateTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        valuesToWrite.animationHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        trackedAnimationTimes.WriteLastValue(valuesToWrite);
    }
    protected void RestoreAnimator(float position)
    {
        animator.speed = 0;
        
        AnimationValues readValues = trackedAnimationTimes.ReadFromBuffer(position);
        animator.Play(readValues.animationHash, -1, readValues.animationStateTime);      
    }
    #endregion

    #region Audio
    CircularBuffer<AudioTrackedData> trackedAudioTimes;
    public struct AudioTrackedData
    {
        public float time;
        public bool isPlaying;
    }

    protected void TrackAudio()
    {
        
        audioSource.volume = 1;
        AudioTrackedData dataToWrite;
        dataToWrite.time = audioSource.time;
        dataToWrite.isPlaying = audioSource.isPlaying;

        trackedAudioTimes.WriteLastValue(dataToWrite);
    }
    protected void RestoreAudio(float position)
    {
        AudioTrackedData readValues = trackedAudioTimes.ReadFromBuffer(position);
        if(readValues.isPlaying)
        {
            audioSource.time = readValues.time;
            audioSource.volume = 0;

            if (!audioSource.isPlaying)
            {  
                audioSource.Play();
            }
        }
        else if(audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    #endregion

    #region Particles
    private float particleTimeLimiter;
    private float particleResetTimeTo;
    List<CircularBuffer<ParticleTrackedData>> trackedParticleTimes;
    public struct ParticleTrackedData
    {
        public bool isActive;
        public float particleTime;
    }


    private List<ParticleData> particleSystemsData;

    /// <summary>
    /// particle system and its enabler, which is for tracking if particle system game object is enabled or disabled
    /// particle
    /// </summary>
    [Serializable]
    public struct ParticleData
    {
        
        public ParticleSystem particleSystem;
        [Tooltip("Particle system enabler, for tracking particle system game object active state")]
        public GameObject particleSystemEnabler;
    }
    /// <summary>
    /// Particle settings to setup particles in custom variable tracking
    /// </summary>
    [Serializable]
    public struct ParticlesSetting
    {
        [Tooltip("For long lasting particle systems, set time tracking limiter to drastically improve performance ")]
        public float particleLimiter;
        [Tooltip("Variable defining to which second should tracking return to after particle tracking limit was hit. Play with this variable to get better results, so the tracking resets are not much noticeable.")]
        public float particleResetTo;
        public List<ParticleData> particlesData;
    }

    /// <summary>
    /// Use this method first when using particle rewinding implementation
    /// </summary>
    /// <param name="particleDataList">Data defining which particles will be tracked</param>
    /// <param name="particleTimeLimiter">For long lasting particle systems, set time tracking limiter to drastically improve performance </param>
    /// <param name="resetParticleTo">Variable defining to which second should tracking return to after particle tracking limit was hit. Play with this variable to get better results, so the tracking resets are not much noticeable.</param>
    protected void InitializeParticles(ParticlesSetting particleSettings)
    {
        particleSystemsData = particleSettings.particlesData;
        particleTimeLimiter = particleSettings.particleLimiter;
        particleResetTimeTo = particleSettings.particleResetTo;
        particleSystemsData.ForEach(x => trackedParticleTimes.Add(new CircularBuffer<ParticleTrackedData>(howManyItemsFit)));
        foreach (CircularBuffer<ParticleTrackedData> i in trackedParticleTimes)
        {
            ParticleTrackedData trackedData;
            trackedData.particleTime = 0;
            trackedData.isActive = false;
            i.WriteLastValue(trackedData);
        }
    }
    protected void TrackParticles()
    {
        if(particleSystemsData==null)
        {
            Debug.Log("Particles not initialized!!! Call InitializeParticles() before the tracking starts");
            return;
        }

        for (int i = 0; i < particleSystemsData.Count; i++)
        {
            if (particleSystemsData[i].particleSystem.isPaused)
                particleSystemsData[i].particleSystem.Play();

            ParticleTrackedData lastValue = trackedParticleTimes[i].ReadLastValue();
            float addTime = lastValue.particleTime + Time.fixedDeltaTime;

            ParticleTrackedData particleData;
            particleData.isActive = particleSystemsData[i].particleSystemEnabler.activeInHierarchy;

            if ((!lastValue.isActive) && (particleData.isActive))
                particleData.particleTime = 0;
            else if (!particleData.isActive)
                particleData.particleTime = 0;
            else
                particleData.particleTime = (addTime > particleTimeLimiter) ? particleResetTimeTo : addTime;


            trackedParticleTimes[i].WriteLastValue(particleData);
        }        
    }
    protected void RestoreParticles(float position)
    {
        for (int i = 0; i < particleSystemsData.Count; i++)
        {
            GameObject particleEnabler = particleSystemsData[i].particleSystemEnabler;


            ParticleTrackedData particleTracked = trackedParticleTimes[i].ReadFromBuffer(position);

            if (particleTracked.isActive)
            {

                if (!particleEnabler.activeSelf)
                    particleEnabler.SetActive(true);

                particleSystemsData[i].particleSystem.Simulate(particleTracked.particleTime, false, true, false);
            }
            else
            {
                if (particleEnabler.activeSelf)
                    particleEnabler.SetActive(false);
            }
        }
    }
    #endregion

    private void RestoreBufferPositions(float seconds)
    {
        trackedPositionsAndRotation.MoveLastBufferPosition(seconds * howManyRecordsPerSecond);
        trackedVelocities.MoveLastBufferPosition(seconds * howManyRecordsPerSecond);
        trackedAnimationTimes.MoveLastBufferPosition(seconds * howManyRecordsPerSecond);

        trackedParticleTimes.ForEach(x => x.MoveLastBufferPosition(seconds * howManyRecordsPerSecond));
        trackedAudioTimes.MoveLastBufferPosition(seconds * howManyRecordsPerSecond);
        AdditionalRestores(seconds);
    }
    private void OnTrackingChange(bool val)
    {
        IsTracking = val;
    }
    private void OnEnable()
    {
        if(rewindManager != null)
        {
            rewindManager.RewindTimeCall += GetSnapshotFromSavedValues;
            rewindManager.TrackingStateCall += OnTrackingChange;
            rewindManager.RestoreBuffers += RestoreBufferPositions;
        }
        
        trackedPositionsAndRotation = new CircularBuffer<PositionAndRotationValues>(howManyItemsFit);
        trackedVelocities = new CircularBuffer<Vector3>(howManyItemsFit);
        trackedAnimationTimes = new CircularBuffer<AnimationValues>(howManyItemsFit);
        trackedParticleTimes = new List<CircularBuffer<ParticleTrackedData>>();
        trackedAudioTimes = new CircularBuffer<AudioTrackedData>(howManyItemsFit);
      
        AdditionalResets();
    }
    private void OnDisable()
    {
        rewindManager.RewindTimeCall -= GetSnapshotFromSavedValues;
        rewindManager.TrackingStateCall -= OnTrackingChange;
        rewindManager.RestoreBuffers -= RestoreBufferPositions;
    }

    /// <summary>
    /// Method where all tracking is filled, lets choose here what will be tracked for specific object
    /// </summary>
    protected abstract void Track();


    /// <summary>
    /// Main method to restore saved snapshots for specific object
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds we want to view back</param>
    protected abstract void GetSnapshotFromSavedValues(float seconds);



    /// <summary>
    /// Method reseting circullar buffer values usually on SceneReload  (method must be used for custom variables)
    /// </summary>
    protected abstract void AdditionalResets();


    /// <summary>
    /// Method deleting circullar buffer values, to correct position after rewind for custom variable tracking (method must be used for custom variables)
    /// </summary>
    /// <param name="seconds">How many seconds should the circular buffer restore back</param>
    protected abstract void AdditionalRestores(float seconds);
}      