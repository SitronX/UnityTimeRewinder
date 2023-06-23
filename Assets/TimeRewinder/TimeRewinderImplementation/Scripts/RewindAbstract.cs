using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class RewindAbstract : MonoBehaviour
{
    Rigidbody body;
    Rigidbody2D body2;
    Animator animator;
    AudioSource audioSource;
    

    public void MainInit()
    {
        body = GetComponent<Rigidbody>();
        body2 = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        trackedActiveStates = new CircularBuffer<bool>();
        trackedTransformValues = new CircularBuffer<TransformValues>();

        if(body!=null||body2!=null)
            trackedVelocities = new CircularBuffer<VelocityValues>();

        if (animator != null)
        {
            trackedAnimationTimes = new List<CircularBuffer<AnimationValues>>();
            for (int i = 0; i < animator.layerCount; i++)
                trackedAnimationTimes.Add(new CircularBuffer<AnimationValues>());
        }
           
        if(audioSource!=null)
            trackedAudioTimes = new CircularBuffer<AudioTrackedData>();
    }
    #region ActiveState
    CircularBuffer<bool> trackedActiveStates;

    /// <summary>
    /// Call this method in Track() if you want to track object active state
    /// </summary>
    protected void TrackObjectActiveState()
    {
        trackedActiveStates.WriteLastValue(gameObject.activeSelf);
    }
    /// <summary>
    /// Call this method in Rewind() to restore object active state
    /// </summary>
    /// <param name="seconds">Use seconds parameter from Rewind() method</param>
    protected void RestoreObjectActiveState(float seconds)
    {
        gameObject.SetActive(trackedActiveStates.ReadFromBuffer(seconds));
    }

    #endregion

    #region Transform

    CircularBuffer<TransformValues> trackedTransformValues;
    public struct TransformValues
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    
    /// <summary>
    /// Call this method in Track() if you want to track object Transforms (position, rotation and scale)
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
    /// Call this method in Rewind() to restore Transform
    /// </summary>
    /// <param name="seconds">Use seconds parameter from Rewind() method</param>
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
        public float angularVelocity2D;
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
            valuesToWrite.angularVelocity2D = 0;
            trackedVelocities.WriteLastValue(valuesToWrite);            
        }
        else if (body2!=null)
        {
            VelocityValues valuesToWrite;
            valuesToWrite.velocity = body2.velocity;
            valuesToWrite.angularVelocity = Vector3.zero;
            valuesToWrite.angularVelocity2D = body2.angularVelocity;
            trackedVelocities.WriteLastValue(valuesToWrite);
        }
        else
        {
            Debug.LogError("Cannot find Rigidbody on the object, while TrackVelocity() is being called!!!");
        }
    }

    /// <summary>
    /// Call this method in Rewind() to restore velocity of Rigidbody
    /// </summary>
    /// <param name="seconds">Use seconds parameter from Rewind() method</param>
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
            body2.angularVelocity = valuesToRead.angularVelocity2D;
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
    /// Call this method in Rewind() to restore Animator state
    /// </summary>
    /// <param name="seconds">Use seconds parameter from Rewind() method</param>
    protected void RestoreAnimator(float seconds)
    {
        animator.speed = 0;
        for (int i=0;i<animator.layerCount;i++)
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
    /// Call this method in Rewind() to restore Audio
    /// </summary>
    /// <param name="seconds">Use seconds parameter from Rewind() method</param>
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

    private List<ParticleSystem> particleSystemsData;

    /// <summary>
    /// Particle settings to setup particles in custom variable tracking
    /// </summary>
    [Serializable]
    public struct ParticlesSetting
    {
        [Tooltip("For long lasting particle systems, set time tracking limit to drastically improve performance ")]
        public float particleTrackingLimit;
        [Tooltip("Variable defining from which second should the particle system be restarted after tracking limit was hit. Play with this variable to get better results, so the tracking resets are not much noticeable.")]
        public float particleRestartFrom;
        public List<ParticleSystem> particleSystems;
    }

    /// <summary>
    /// Use this method first when using particle rewinding implementation
    /// </summary>
    /// <param name="particleSettings"></param>
    protected void InitializeParticles(ParticlesSetting particleSettings)
    {
        if(particleSettings.particleSystems.Any(x=>x==null))
        {
            Debug.LogError("Initialized particle system are missing data. Some particle systems are not filled.");
        }
        particleSystemsData = particleSettings.particleSystems;
        particleTimeLimiter = particleSettings.particleTrackingLimit;
        particleResetTimeTo = particleSettings.particleRestartFrom;
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
                if (particleSystemsData[i].isPaused)
                    particleSystemsData[i].Play();

                ParticleTrackedData lastValue = trackedParticleTimes[i].ReadLastValue();
                float addTime = lastValue.particleTime + Time.fixedDeltaTime;

                ParticleTrackedData particleData;
                particleData.isActive = particleSystemsData[i].gameObject.activeInHierarchy;

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
            Debug.LogError("Particles Data not filled properly!!! Fill both the Particle System and Particle Main Object fields for each element");
        }

    }
    /// <summary>
    /// Call this method in Rewind() to restore Particles
    /// </summary>
    /// <param name="seconds">Use seconds parameter from Rewind() method</param>
    protected void RestoreParticles(float seconds)
    {
        for (int i = 0; i < particleSystemsData.Count; i++)
        {
            ParticleTrackedData particleTracked = trackedParticleTimes[i].ReadFromBuffer(seconds);        
            particleSystemsData[i].Simulate(particleTracked.particleTime, false, true, false);          
        }
    }
    #endregion


    /// <summary>
    /// Main method where all tracking is filled, choose what will be tracked for specific object
    /// </summary>
    public abstract void Track();


    /// <summary>
    /// Main method where all rewinding is filled, choose what will be rewinded for specific object
    /// </summary>
    /// <param name="seconds">Parameter defining how many seconds you want to rewind back</param>
    public abstract void Rewind(float seconds);

}      