using UnityEngine;

public static class WaveFunctions
{
    public static float SinX(Vector3 position,
        float speed,
        float scale,
        float waveDistance,
        float noiseStrength,
        float noiseWalk,
        float timeSinceStart)
    {
        float x = position.x, y = 0f, z = position.z;

        // float straightWaves = x or z
        // float upDownWaves = y
        // float rollingWaves = x + y + z
        // float movingSeaNoWaves = x * z

        float wave = z;

        y += Mathf.Sin((timeSinceStart * speed + wave) / waveDistance) * scale;

        y += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;

        return y;
    }
}
