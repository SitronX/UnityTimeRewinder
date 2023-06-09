using System;
using UnityEngine;
using static RewindAbstract;

[Serializable]
public struct OptionalParticleSettings
{
    [SerializeField] private bool enabled;
    [SerializeField] private ParticlesSetting value;

    public bool Enabled => enabled;
    public ParticlesSetting Value => value;

    public OptionalParticleSettings(ParticlesSetting initialValue)
    {
        enabled = true;
        value = initialValue;
    }
}