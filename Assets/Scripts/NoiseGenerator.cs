using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct OctaveData
{
    [Range(0f, 100f)]
    public float Frequency;
    [Range(0f, 100f)]
    public float Amplitude;
}

public enum OctaveMode
{
    Manual,
    FBM,
}

public class NoiseGenerator : MonoBehaviour
{
    public int Width;
    public int Height;

    public int RandomOffsetSeed;

    public Vector2 BaseOffset;

    public Vector2 SizeScale;
    public bool UseUnityPerlinImpl;

    public OctaveMode OM;
    public List<OctaveData> Octaves;
    public uint OctaveN;
    [Range(1f, 20f)]
    public float FrequencyBase;
    [Range(0f, 1f)]
    public float AmplitudeBase;

    public Vector2Int Grid;

    public float[,] NoiseData;

    public Action OnNoiseChange;

    public bool EnableExponentialDistribution;

    [Range(1.001f, 1.1599f)]
    public float ExponentialBase;

    public UnityEngine.UI.RawImage Img;

    public void GenNoise(Vector2Int sampleGridCoordinate, Vector2Int extraSample, bool save = false, bool InvokeOnChange = true)
    {
        switch (OM)
        {
            case OctaveMode.FBM:
                NoiseData = Noise.Gen2DNoiseByPerlin(Width, Height, extraSample, BaseOffset, SizeScale, FBM, sampleGridCoordinate, EnableExponentialDistribution, ExponentialBase, UseUnityPerlinImpl, RandomOffsetSeed);
                break;

            case OctaveMode.Manual:
                NoiseData = Noise.Gen2DNoiseByPerlin(Width, Height, extraSample, BaseOffset, SizeScale, ManualOctave, sampleGridCoordinate, EnableExponentialDistribution, ExponentialBase, UseUnityPerlinImpl, RandomOffsetSeed);
                break;
        }

        var tex = Noise.GenTex2DFromNoise(NoiseData, Width + extraSample.x * 2, Height + extraSample.y * 2);
        Img.texture = tex;
        if (save)
            System.IO.File.WriteAllBytes("Assets/Textures/perlin.png", tex.EncodeToPNG());

        if (InvokeOnChange)
        {
            if (OnNoiseChange != null)
                OnNoiseChange();
        }
    }

    private float ManualOctave(float _u, float _v, bool useUnityImp, bool expDis, float expBase)
    {
        float noiseVal = 0f;
        foreach (var octaveData in Octaves)
        {
            float u = _u * octaveData.Frequency;
            float v = _v * octaveData.Frequency;
            noiseVal += (useUnityImp ? Mathf.PerlinNoise(u, v) : (float)PerlinNoise.Gen(u, v, 0, 0, expDis, expBase)) * octaveData.Amplitude;
        }
        return noiseVal;
    }

    public float FBM(float _u, float _v, bool useUnityImp, bool expDis, float expBase)
    {
        float noiseVal = 0f;
        float frequency = 1f, amplitude = 1f;
        for (int i = 0; i < OctaveN; i++)
        {
            float u = _u * frequency;
            float v = _v * frequency;
            noiseVal += (useUnityImp ? Mathf.PerlinNoise(u, v) : (float)PerlinNoise.Gen(u, v, 0, 0, expDis, expBase)) * amplitude;
            frequency *= FrequencyBase;
            amplitude *= AmplitudeBase;
        }
        return noiseVal;
    }

    void OnValidate()
    {
        if (Width <= 0)
            Width = 1;
        if (Height <= 0)
            Height = 1;
        if (RandomOffsetSeed <= 0)
            RandomOffsetSeed = 1;
        if (SizeScale.x == 0 || SizeScale.y == 0)
            SizeScale = Vector2.one;
    }
}