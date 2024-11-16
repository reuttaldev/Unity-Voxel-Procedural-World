using System.Collections;
using System.Collections.Generic;
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
            total += Mathf.PerlinNoise((settings.noiseOffset + x) * frequency, (settings.noiseOffset + z) * frequency) * amplitude;
            amplitudeSum += amplitude;
            frequency *= 2;
            amplitude *= settings.amplitudeMultiplier;
        }
        // normalize result
        return total / amplitudeSum;
    }

    public static float Redistribution(float noise, NoiseSettings settings)
    {
        return Mathf.Pow(noise *settings.redistributionMultiplier, settings.expo);
    }
    public static int NormalizeToChunkHeight(float n)
    {
        return (int)(n * EnvironmentConstants.chunkHeight);
    }
    public static int GetNoise(float x, float z, NoiseSettings settings)
    {
        float noise = NoiseUtility.OctavePerlin(x, z, settings);
        noise = NoiseUtility.Redistribution(noise,settings);
        return (int)NoiseUtility.NormalizeToChunkHeight(noise);
    }


}
