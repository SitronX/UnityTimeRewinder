using UnityEngine;

public class GenericRewind : RewindAbstract
{
    [Tooltip("Tracking Position,Rotation and Scale")]
    [SerializeField] bool trackTransform;
    [SerializeField] bool trackVelocity;
    [SerializeField] bool trackAnimator;
    [SerializeField] bool trackAudio;

    [Tooltip("Enable checkbox on right side to track particles")]
    [SerializeField] OptionalParticleSettings trackParticles;

    protected override void Rewind(float seconds)
    {

        if (trackTransform)
            RestoreTransform(seconds);
        if (trackVelocity)
            RestoreVelocity(seconds);
        if (trackAnimator)
            RestoreAnimator(seconds);
        if (trackParticles.Enabled)
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
        if (trackParticles.Enabled)
            TrackParticles();
        if (trackAudio)
            TrackAudio();
    }
    private void Start()
    {
        InitializeParticles(trackParticles.Value);
    }

}

