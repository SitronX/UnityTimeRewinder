using UnityEngine;

public class GenericRewind : RewindAbstract
{
    [Tooltip("Tracking Position,Rotation and Scale")]
    [SerializeField] bool trackTransform;
    [SerializeField] bool trackVelocity;
    [SerializeField] bool trackAnimator;
    [SerializeField] bool trackAudio;
    [SerializeField] bool trackParticles;

    [Tooltip("Fill particle settings only if you check Track Particles")]
    [SerializeField] ParticlesSetting particleSettings;

    protected override void Rewind(float seconds)
    {

        if (trackTransform)
            RestoreTransform(seconds);
        if (trackVelocity)
            RestoreVelocity(seconds);
        if (trackAnimator)
            RestoreAnimator(seconds);
        if (trackParticles)
            RestoreParticles(seconds);
        if(trackAudio)
            RestoreAudio(seconds);
    }

    protected override void Track()
    {
        if (trackTransform)
            TrackTransform();
        if (trackVelocity)
            TrackVelocity();
        if (trackAnimator)
            TrackAnimator();
        if (trackParticles)
            TrackParticles();
        if (trackAudio)
            TrackAudio();
    }
    private void Start()
    {
        InitializeParticles(particleSettings);
    }

}

