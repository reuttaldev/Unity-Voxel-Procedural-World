using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Noise Settings", menuName = "Scriptable Objects/ Noise Settings")]

public class NoiseSettings : ScriptableObject
{
    [Header("Noise")]
    public float zoom = 0.01f;
    public float zoomOffset = 0.01f;
    public float noiseOffset = -100;
    // noise layers to combine 
    public int octaves = 5;
    public float amplitudeMultiplier = 0.5f;
    public float intensity = 1.0f;
    public float smoothness = 5;
}
