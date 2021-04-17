using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Modifying the default Perlin noise generation attributes to make our noise generation more realistic 
[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude; 
}

public class Noise : MonoBehaviour
{
    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale, float offsetX, float offsetZ, Wave[] waves)
    {
        float[,] noiseMap = new float[mapDepth, mapWidth];
        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                float sampleX = (xIndex + offsetX) / scale;
                float sampleZ = (zIndex + offsetZ) / scale;

                float noise = 0f;
                float normalization = 0f;
                foreach (Wave wave in waves)
                {
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
                    normalization += wave.amplitude;
                }

                // Normalize to [0, 1] by saving the amplification we did 
                noise /= normalization;
                noiseMap[zIndex, xIndex] = noise;
            }
        }
        return noiseMap;
    }
}