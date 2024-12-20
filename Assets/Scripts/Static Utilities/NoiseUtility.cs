using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class NoiseUtility
{
    public static float OctavePerlin(float x, float z, NoiseSettings settings)
    {
        x = x * settings.zoom + settings.zoomOffset;
        z = z * settings.zoom + settings.zoomOffset;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0; 
        for (int i = 0; i < settings.octaves; i++)
        {
            // perlin noise returns the same output for all int values, so add some small offset to make sure the input we give is float
            total += Mathf.PerlinNoise((settings.noiseOffset + x) * frequency, (settings.noiseOffset + z) * frequency)
                //amplitude determines how much influence each layer noise has on the final noise.
                * amplitude;
            amplitudeSum += amplitude;
            // noise values change over space, which leads to different patterns or levels of detail
            // that we are stacking on one another
            frequency *= 2;
            amplitude *= settings.amplitudeMultiplier;
        }
        // normalize result
        return total / amplitudeSum;
    }

    public static int NormalizeToChunkHeight(float n)
    {
        if (n > 1)
            Debug.LogError("n is bigger than 1, normalization wlll not be correct");
        return (int)(n * EnvironmentConstants.chunkHeight);
    }
    // Make the noise non-linear to create natural looking results
    public static float Redistribution(float noise, NoiseSettings settings)
    {
        return Mathf.Pow(noise, settings.smoothness);
    }
    //Normalize from 0 to chunk hights. We are working with voxel units, so the value must be int
    public static int GetNormalizedNoise(Vector2 globalPos, NoiseSettings settings)
    {
        float noise = OctavePerlin(globalPos.x, globalPos.y, settings);
        noise = Redistribution(noise, settings);
        return (int)NormalizeToChunkHeight(noise);
    }

    public static float GetNoise(Vector2 globalPos, NoiseSettings settings)
    {
        float noise = OctavePerlin(globalPos.x, globalPos.y, settings);
        return  Redistribution(noise, settings);
    }
}
