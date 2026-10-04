using System;
using System.Diagnostics;
using UnityEngine;

internal sealed class MiniMapPixelProcessor
{
    private static readonly byte[] fogColors = BuildFogColors();
    private static readonly byte[] fadeAlphas = BuildFadeAlphas();
    private byte[] distance;
    private int index, phase, width;
    private bool fog, fade;
    internal Color32[] Pixels { get; private set; }
    internal bool Running => Pixels != null && phase < 4;

    internal void Begin(Color32[] pixels, int textureWidth, bool useFog, bool useFade)
    {
        Pixels = pixels;
        width = textureWidth;
        fog = useFog;
        fade = useFade && !useFog;
        if (fade && (distance == null || distance.Length != pixels.Length)) distance = new byte[pixels.Length];
        phase = fade ? 0 : 3;
        index = 0;
    }

    internal void Cancel() { Pixels = null; phase = 4; }

    // A hard work limit and a small CPU time budget prevent an entire texture from blocking one frame.
    internal bool Step(int budget = 262144, double maxMilliseconds = 2.0)
    {
        long start = Stopwatch.GetTimestamp();
        int processed = 0;
        while (Running && processed < budget)
        {
            int i = phase == 2 ? Pixels.Length - 1 - index : index;
            if (phase == 0) distance[i] = Pixels[i].a == 0 ? (byte)0 : (byte)32;
            else if (phase == 1)
            {
                if (i % width > 0) distance[i] = (byte)Math.Min(distance[i], distance[i-1] + 1);
                if (i >= width) distance[i] = (byte)Math.Min(distance[i], distance[i-width] + 1);
            }
            else if (phase == 2)
            {
                if (i % width + 1 < width) distance[i] = (byte)Math.Min(distance[i], distance[i+1] + 1);
                if (i + width < Pixels.Length) distance[i] = (byte)Math.Min(distance[i], distance[i+width] + 1);
            }
            else
            {
                Color32 color = Pixels[i];
                if (fog)
                {
                    int offset = color.a << 8;
                    color.r = fogColors[offset + color.r];
                    color.g = fogColors[offset + color.g];
                    color.b = fogColors[offset + color.b];
                    color.a = 255;
                }
                else if (color.a != 0) color.a = fade ? fadeAlphas[distance[i]] : (byte)255;
                Pixels[i] = color;
            }
            processed++;
            if (++index == Pixels.Length) { index = 0; phase++; }
            if ((processed & 4095) == 0 && (Stopwatch.GetTimestamp()-start)*1000.0/Stopwatch.Frequency >= maxMilliseconds) break;
        }
        return !Running;
    }

    private static byte[] BuildFogColors()
    {
        var table = new byte[65536];
        for (int a = 0; a < 256; a++) for (int c = 0; c < 256; c++)
            table[(a << 8)+c] = (byte)((210*(255-a)+c*a)/255);
        return table;
    }

    private static byte[] BuildFadeAlphas()
    {
        var table = new byte[33];
        for (int i = 0; i < table.Length; i++)
        {
            float t = Math.Min(1f, i/12f);
            table[i] = (byte)(255f*t*t*(3f-2f*t));
        }
        return table;
    }
}
