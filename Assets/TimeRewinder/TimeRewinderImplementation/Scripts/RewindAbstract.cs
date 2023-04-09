using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class RewindAbstract : MonoBehaviour
{
    RewindManager rewindManager;
    public bool IsTracking { get; set; } = false;

    Rigidbody body;
    Rigidbody2D body2;
    Animator animator;
    AudioSource audioSource;


    protected void Awake()
    {

        rewindManager = FindObjectOfType<RewindManager>();
        if (rewindManager != null)
        {
            body = GetComponent<Rigidbody>();
            body2 = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();

            IsTracking = true;
        }
        else
        {
            Debug.LogError("TimeManager script cannot be found in scene. Time tracking cannot be started. Did you forget to put it into the scene?");
        }

        trackedTransformValues = new CircularBuffer<TransformValues>();
        trackedVelocities = new CircularBuffer<VelocityValues>();
        trackedAnimationTimes = new List<CircularBuffer<AnimationValues>>();
        if (animator != null)
            for (int i = 0; i < animator.layerCount; i++)
                trackedAnimationTimes.Add(new CircularBuffer<AnimationValues>());
        trackedAudioTimes = new CircularBuffer<AudioTrackedData>();
    }

    protected void FixedUpdate()
    {
        if (IsTracking)
            Track();
    }

    #region Transform

    CircularBuffer<TransformValues> trackedTransformValues;
    public struct TransformValues
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    
    /// <summary>
    /// Call this method in Track() if you want to track object Position and Rotation
    /// </summary>
    protected void TrackTransform()
    {
        TransformValues valuesToWrite;
        valuesToWrite.position = transform.position;
        valuesToWrite.rotation = transform.rotation;
        valuesToWrite.scale = transform.localScale;
        trackedTransformValues.WriteLastValue(valuesToWrite);
    }
    /// <summary>
    /// Call this method in GetSnapshotFromSavedValues() to restore Position and Rotation
    /// </summary>
    protected void RestoreTransform(float seconds)
    {
        TransformValues valuesToRead = trackedTransformValues.ReadFromBuffer(seconds);
        transform.SetPositionAndRotation(valuesToRead.position, valuesToRead.rotation);
        transform.localScale= valuesToRead.scale;
    }
    #endregion

    #region Velocity
    public struct VelocityValues
    {
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }
    CircularBuffer<VelocityValues> trackedVelocities;
    /// <summary>
    /// Call this method in Track() if you want to track velocity of Rigidbody
    /// </summary>
    protected void TrackVelocity()
    {
        if (body != null)
        {
            VelocityValues valuesToWrite;
            valuesToWrite.velocity= body.velocity;
            valuesToWrite.angularVelocity = body.angularVelocity;
            trackedVelocities.WriteLastValue(valuesToWrite);            
        }
        else if (body2!=null)
        {
            VelocityValues valuesToWrite;
            valuesToWrite.velocity = body2.velocity;
            valuesToWrite.angularVelocity = new Vector3(body2.angularVelocity,0,0);
            trackedVelocities.WriteLastValue(valuesToWrite);
        }
        else
        {
            Debug.LogError("Cannot find Rigidbody on the object, while TrackVelocity() is being called!!!");
        }
    }
    /// <summary>
    /// Call this method in GetSnapshotFromSavedValues() to velocity of Rigidbody
    /// </summary>
    protected void RestoreVelocity(float seconds)
    {   
        if(body!=null)
        {
            VelocityValues valuesToRead= trackedVelocities.ReadFromBuffer(seconds);
            body.velocity = valuesToRead.velocity;
            body.angularVelocity = valuesToRead.angularVelocity;
        }
        else
        {
            VelocityValues valuesToRead = trackedVelocities.ReadFromBuffer(seconds);
            body2.velocity = valuesToRead.velocity;
            body2.angularVelocity = valuesToRead.angularVelocity.x;
        }
    }
    #endregion

    #region Animator
    List<CircularBuffer<AnimationValues>> trackedAnimationTimes;         //All animator layers are tracked
    public struct AnimationValues
    {
        public float animationStateTime;
        public int animationHash;
    }
    /// <summary>
    /// Call this method in Track() if you want to track Animator states
    /// </summary>
    protected void TrackAnimator()
    {
        if(animator == null)
        {
            Debug.LogError("Cannot find Animator on the object, while TrackAnimator() is being called!!!");
            return;
        }

        animator.speed = 1;

        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo animatorInfo = animator.GetCurrentAnimatorStateInfo(i);

            AnimationValues valuesToWrite;
            valuesToWrite.animationStateTime = animatorInfo.normalizedTime;
            valuesToWrite.animationHash = animatorInfo.shortNameHash;
            trackedAnimationTimes[i].WriteLastValue(valuesToWrite);
        }         
    }
    /// <summary>
    /// Call this method in GetSnapshotFromSavedValues() to restore Animator state
    /// </summary>
    protected void RestoreAnimator(float seconds)
    {
        animator.speed = 0;
        
        for(int i=0;i<animator.layerCount;i++)
        {
            AnimationValues readValues = trackedAnimationTimes[i].ReadFromBuffer(seconds);
            animator.Play(readValues.animationHash,i, readValues.animationStateTime);
        }         
    }
    #endregion

    #region Audio
    CircularBuffer<AudioTrackedData> trackedAudioTimes;
    public struct AudioTrackedData
    {
        public float time;
        public bool isPlaying;
        public bool isEnabled;
    }
    /// <summary>
    /// Call this method in Track() if you want to track Audio
    /// </summary>
    protected void TrackAudio()
    {
        if(audioSource==null)
        {
            Debug.LogError("Cannot find AudioSource on the object, while TrackAudio() is being called!!!");
            return;
        }

        audioSource.volume = 1;
        AudioTrackedData dataToWrite;
        dataToWrite.time = audioSource.time;
        dataToWrite.isEnabled = audioSource.enabled;
        dataToWrite.isPlaying = audioSource.isPlaying;

        trackedAudioTimes.WriteLastValue(dataToWrite);      
    }
    /// <summary>
    /// Call this method in GetSnapshotFromSavedValues() to restore Audio
    /// </summary>
    protected void RestoreAudio(float seconds)
    {
        AudioTrackedData readValues = trackedAudioTimes.ReadFromBuffer(seconds);
        audioSource.enabled = readValues.isEnabled;
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
    List<CircularBuffer<ParticleTrackedData>> trackedParticleTimes = new List<CircularBuffer<ParticleTrackedData>>();
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
        if(particleSettings.particlesData.Any(x=>x.particleSystemEnabler==null||x.particleSystem==null))
        {
            Debug.LogError("Initialized particle system are missing data. Either Particle System or Particle System Enabler is not filled for some values");
        }
        particleSystemsData = particleSettings.particlesData;
        particleTimeLimiter = particleSettings.particleLimiter;
        particleResetTimeTo = particleSettings.particleResetTo;
        particleSystemsData.ForEach(x => trackedParticleTimes.Add(new CircularBuffer<ParticleTrackedData>()));
        foreach (CircularBuffer<ParticleTrackedData> i in trackedParticleTimes)
        {
            ParticleTrackedData trackedData;
            trackedData.particleTime = 0;
            trackedData.isActive = false;
            i.WriteLastValue(trackedData);
        }
    }
    /// <summary>
    /// Call this method in Track() if you want to track Particles (Note that InitializeParticles() must be called beforehand)
    /// </summary>
    protected void TrackParticles()
    {
        if(particleSystemsData==null)
        {
            Debug.LogError("Particles not initialized!!! Call InitializeParticles() before the tracking starts");
            return;
        }
        if(particleSystemsData.Count==0)
            Debug.LogError("Particles Data not filled!!! Fill Particles Data in the Unity Editor");

        try
        {
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
        catch
        {
            Debug.LogError("Particles Data not filled properly!!! Fill both the Particle System and Particle System Enabler fields for each element");
        }

    }
    /// <summary>
    /// Call this method in GetSnapshotFromSavedValues() to Particles
    /// </summary>
    protected void RestoreParticles(float seconds)
    {
        for (int i = 0; i < particleSystemsData.Count; i++)
        {
            GameObject particleEnabler = particleSystemsData[i].particleSystemEnabler;


            ParticleTrackedData particleTracked = trackedParticleTimes[i].ReadFromBuffer(seconds);

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

   
    private void OnTrackingChange(bool val)
    {
        IsTracking = val;
    }
    protected void OnEnable()
    {
        RewindManager.RewindTimeCall += Rewind;
        RewindManager.TrackingStateCall += OnTrackingChange;        
    }
    protected void OnDisable()
    {
        RewindManager.RewindTimeCall -= Rewind;
        RewindManager.TrackingStateCall -= OnTrackingChange;            
    }

    /// <summary>
    /// Main method where all tracking is filled, lets choose here what will be tracked for specific object
    /// </summary>
    protected abstract void Track();


    /// <summary>
    /// Main method where all rewinding is filled, lets choose here what will be rewinded for specific object
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds we want to rewind back</param>
    protected abstract void Rewind(float seconds);

}      