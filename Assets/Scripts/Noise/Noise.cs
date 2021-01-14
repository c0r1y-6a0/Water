using UnityEngine;
using System.Collections.Generic;


public static class Noise
{
    public static float Gen2DNoiseByPerlin(Bounds b, Vector2 samplePoint, int octavesN, float frequencyBase, float amplitudeBase, bool useUnityImp = false, int randomOffsetSeed = 0, bool expDis = false, float expBase = 1f)
    {
        float noiseVal = 0f;
        float frequency = 1f, amplitude = 1f;
        float offsetX = b.size.x * samplePoint.x;
        float offsetY = b.size.y * samplePoint.y;
        for (int n = 0; n < octavesN; n++)
        {
            float u = (b.min.x + offsetX) * frequency;
            float v = (b.min.y + offsetY) * frequency;
            float rawNoiseVal = (useUnityImp ? Mathf.PerlinNoise(u, v) : (float)PerlinNoise.Gen(u, v, 0, 0, expDis, expBase)) * amplitude;
            amplitude *= amplitudeBase;
            frequency *= frequencyBase;
            noiseVal += rawNoiseVal;

        }
        return noiseVal;
    }

    public enum OctaveMode
    {
        Manual,
        Auto,
    }

    private static Vector2Int DEFAULT_EXTRA_SAMPLE = new Vector2Int(0, 0);
    public static float[,] Gen2DNoiseByPerlin(int width, int height, Vector2Int extraSample, Vector2 baseOffset, Vector2 sampleSizeScale, System.Func<float, float, bool, bool, float, float> OctaveFunc, Vector2Int gridOffset, bool expDis = false, float expBase = 1f, bool useUnityImp = false, int randomOffsetSeed = 0)
    {
        float offsetX = baseOffset.x;
        float offsetY = baseOffset.y;

        if (randomOffsetSeed != 0)
        {
            UnityEngine.Random.InitState(randomOffsetSeed);
            offsetX += UnityEngine.Random.Range(-100f, 100f);
            offsetY += UnityEngine.Random.Range(-100f, 100f);
        }

        width += extraSample.x * 2;
        height += extraSample.y * 2;

        float minNoiseVal = float.MaxValue, maxNoiseVal = float.MinValue;

        float[,] noise = new float[height, width];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float noiseVal = 0f;
                float u = ((offsetX + (col - extraSample.x) / (float)(width - 1 - 2 * extraSample.x) - 0.5f) + gridOffset.x) * sampleSizeScale.x;
                float v = ((offsetY + (row - extraSample.y) / (float)(height - 1 - 2 * extraSample.y) - 0.5f) + gridOffset.y) * sampleSizeScale.y;

                noiseVal = OctaveFunc(u, v, useUnityImp, expDis, expBase);


                noise[row, col] = noiseVal;

                if (noiseVal > maxNoiseVal)
                    maxNoiseVal = noiseVal;
                if (noiseVal < minNoiseVal)
                    minNoiseVal = noiseVal;
            }
        }

        /*
        for(int y = 0 ; y < height ; y++)
        {
            for(int x = 0 ; x < width ; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseVal, maxNoiseVal, noise[x, y]);
            }
        }
        */

        return noise;
    }

    public static Texture2D GenTex2DFromNoise(float[,] noise, int texWidth, int texHeight)
    {
        Texture2D tex = new Texture2D(texWidth, texHeight);
        Color[] colours = new Color[texWidth * texHeight];
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int row = 0; row < texHeight; row++)
        {
            for (int col = 0; col < texWidth; col++)
            {
                colours[row * texWidth + col] = Color.Lerp(Color.white, Color.black, noise[row, col]);
            }
        }
        tex.SetPixels(colours);
        tex.Apply();
        return tex;
    }
}