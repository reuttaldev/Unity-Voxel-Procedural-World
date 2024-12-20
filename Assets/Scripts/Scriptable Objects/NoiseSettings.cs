using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Noise Settings", menuName = "Scriptable Objects/ Noise Settings")]

public class NoiseSettings : ScriptableObject
{
    // everything has a serialize field marking and a private setter to ensure setting vales 
    // can only be adjusted through the editor, and not through scripts in runtime
    [Header("Noise")]
    [field: SerializeField]
    public float zoom { get; private set; } = 0.01f;
    [field: SerializeField]
    public float zoomOffset { get; private set; } = 0.01f;
    [field: SerializeField]
    public float noiseOffset { get; private set; } = -100;
    [field: SerializeField]
    // noise layers to combine 
    public int octaves { get; private set; } = 5;
    [field: SerializeField]
    public float amplitudeMultiplier { get; private set; } = 0.5f;
    [field: SerializeField]
    public float smoothness { get; private set; } = 1;
}
