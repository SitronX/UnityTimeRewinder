using UnityEngine;

public class ShaderRewind : RewindAbstract
{
    [SerializeField] MeshRenderer _meshRenderer;
    CircularBuffer<float> _cableShaderTimeTracked;

    public float AdvanceTimeMultipl { get; set; } = 0.1f;

    private void Start()
    {
        _cableShaderTimeTracked = new CircularBuffer<float>();
    }
    public override void Rewind(float seconds)
    {
        RestoreCableShader(seconds);
    }

    public override void Track()
    {
        TrackCableShader();
    }

    void TrackCableShader()
    {
        _cableShaderTimeTracked.TryReadLastValue(out float currValue);      //Lets read the last value and add to it. We cannot read from system time, as the time in shader might be different due to various rewinding
        currValue+= (Time.fixedDeltaTime * AdvanceTimeMultipl);
        _cableShaderTimeTracked.WriteLastValue(currValue);

        _meshRenderer.material.SetFloat("_OwnTime", currValue); //When the time is flowing, we update the time variable in the shader every fixed frame
    }
    void RestoreCableShader(float timestepMove)
    {
        float readValue = _cableShaderTimeTracked.ReadFromBuffer(timestepMove, out bool wasLastAccessedIndexSame);

        if (wasLastAccessedIndexSame)           //When the time is fully stopped, there is no need to update the time variable in shader in every fixed frame
            return;

        _meshRenderer.material.SetFloat("_OwnTime", readValue);
    }
}
