using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Timeline;
using static RewindAbstract;

public class GenericRewind : RewindAbstract
{
    [SerializeField] bool trackPositionRotation;
    [SerializeField] bool trackVelocity;
    [SerializeField] bool trackAnimator;
    [SerializeField] bool trackAudio;
    [SerializeField] bool trackParticles;

    [Tooltip("Fill particle options only if you check Track Particles")]
    [SerializeField] ParticlesSetting particleSettings;

    protected override void AdditionalResets()
    {
        
    }

    protected override void GetSnapshotFromSavedValues(float timestepMove)
    {
        float position = timestepMove * howManyRecordsPerSecond;

        if (trackPositionRotation)
            RestorePositionAndRotation(position);
        if (trackVelocity)
            RestoreVelocity(position);
        if (trackAnimator)
            RestoreAnimator(position);
        if (trackParticles)
            RestoreParticles(position);
        if(trackAudio)
            RestoreAudio(position);
    }

    protected override void Track()
    {
        if (trackPositionRotation)
            TrackPositionAndRotation();
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

    protected override void AdditionalRestores(float val)
    {
    }
}

