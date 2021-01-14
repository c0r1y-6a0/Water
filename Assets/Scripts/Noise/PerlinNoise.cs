using UnityEngine;
using UnityEditor;
using System;
public static class PerlinNoise
{
    private static readonly int[] permutation = { 151,160,137,91,90,15,                 // Hash lookup table as defined by Ken Perlin.  This is a randomly
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,    // arranged array of all numbers from 0-255 inclusive.
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

    private static readonly int[] p;
    private static readonly float[] b;

    private static int SIZE = 256;

    static PerlinNoise()
    {
        p = new int[SIZE * 2];
        for (int i = 0; i < SIZE * 2; i++)
            p[i] = permutation[i % SIZE];

        b = new float[SIZE * 2];
        for (int i = 0; i < SIZE * 2; i++)
            b[i] = 1f;
    }

    private static void ReinitExpArray(float newBase)
    {
        Debug.Assert(newBase > 1f && newBase < 1.16f);
        b[0] = 1f;
        for (int i = 1; i < SIZE; i++)
            b[i] = b[i - 1] / newBase;
        for (int i = SIZE; i < SIZE * 2; i++)
            b[i] = b[i - SIZE];
    }

    private static float ExpDis(int index, bool enable)
    {
        if (!enable)
            return 1.0f;
        return b[index];
    }

    public static float Gen(float x, float y, float z, int repeat = 0, bool expDistribution = false, float expBase = 1f)
    {
        if (repeat > 0)
        {
            x = x % repeat;
            y = y % repeat;
            z = z % repeat;
        }

        if (expDistribution)
        {
            ReinitExpArray(expBase);
        }

        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        z = Mathf.Abs(z);

        int xi = (int)x & 255;
        int yi = (int)y & 255;
        int zi = (int)z & 255;

        float xf = x - (int)x;
        float yf = y - (int)y;
        float zf = z - (int)z;

        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        int aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = p[p[p[xi] + yi] + zi];
        aba = p[p[p[xi] + Inc(yi)] + zi];
        aab = p[p[p[xi] + yi] + Inc(zi)];
        abb = p[p[p[xi] + Inc(yi)] + Inc(zi)];
        baa = p[p[p[Inc(xi)] + yi] + zi];
        bba = p[p[p[Inc(xi)] + Inc(yi)] + zi];
        bab = p[p[p[Inc(xi)] + yi] + Inc(zi)];
        bbb = p[p[p[Inc(xi)] + Inc(yi)] + Inc(zi)];

        float x1, x2, y1, y2;
        x1 = Lerp(Grad(aaa, xf, yf, zf) * ExpDis(aaa, expDistribution),           // The gradient function calculates the dot product between a pseudorandom
                    Grad(baa, xf - 1, yf, zf) * ExpDis(baa, expDistribution),             // gradient vector and the vector from the input coordinate to the 8
                    u);                                     // surrounding points in its unit cube.
        x2 = Lerp(Grad(aba, xf, yf - 1, zf) * ExpDis(aba, expDistribution),           // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                    Grad(bba, xf - 1, yf - 1, zf) * ExpDis(bba, expDistribution),             // values we made earlier.
                      u);
        y1 = Lerp(x1, x2, v);

        x1 = Lerp(Grad(aab, xf, yf, zf - 1) * ExpDis(aab, expDistribution),
                    Grad(bab, xf - 1, yf, zf - 1) * ExpDis(bab, expDistribution),
                    u);
        x2 = Lerp(Grad(abb, xf, yf - 1, zf - 1) * ExpDis(abb, expDistribution),
                      Grad(bbb, xf - 1, yf - 1, zf - 1) * ExpDis(bbb, expDistribution),
                      u);
        y2 = Lerp(x1, x2, v);

        return (Lerp(y1, y2, w) + 1) / 2;                      // For convenience we bind the result to 0 - 1 (theoretical min/max before is [-1, 1])

    }

    public static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    public static int Inc(int n, int repeat = 0)
    {
        n++;
        if (repeat > 0) n %= repeat;
        return n;
    }

    public static float Grad(int hash, float x, float y, float z)
    {
        switch (hash & 0xF)
        {
            case 0x0: return x + y;
            case 0x1: return -x + y;
            case 0x2: return x - y;
            case 0x3: return -x - y;
            case 0x4: return x + z;
            case 0x5: return -x + z;
            case 0x6: return x - z;
            case 0x7: return -x - z;
            case 0x8: return y + z;
            case 0x9: return -y + z;
            case 0xA: return y - z;
            case 0xB: return -y - z;
            case 0xC: return y + x;
            case 0xD: return -y + z;
            case 0xE: return y - x;
            case 0xF: return -y - z;
            default: return 0; // never happens
        }
    }

    public static float Lerp(float a, float b, float x)
    {
        return a + x * (b - a);
    }
}