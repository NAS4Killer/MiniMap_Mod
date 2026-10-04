using System.Diagnostics;
using System.Globalization;
using System.Text;

// Aggregate measurements; one report per interval instead of logging every frame.
internal sealed class MiniMapPerformance
{
    private readonly string[] names = { "Frame", "Originalkarte", "PixelLesen", "Randverlauf", "PixelFarben", "TexturUpload", "Marker" };
    private readonly long[] ticks = new long[7];
    private readonly long[] maxima = new long[7];
    private readonly int[] counts = new int[7];

    internal void Record(int stage, long start)
    {
        long elapsed = Stopwatch.GetTimestamp() - start;
        ticks[stage] += elapsed;
        if (elapsed > maxima[stage]) maxima[stage] = elapsed;
        counts[stage]++;
    }

    internal string Report(bool enabled, bool fog, bool fade)
    {
        var report = new StringBuilder("[MiniMap_Mod] PERF (ms): Enabled=");
        report.Append(enabled).Append(" Fog=").Append(fog).Append(" Fade=").Append(fade);
        for (int i = 0; i < names.Length; i++)
        {
            if (counts[i] == 0) continue;
            double multiplier = 1000.0 / Stopwatch.Frequency;
            report.Append(" | ").Append(names[i]).Append(" n=").Append(counts[i])
                .Append(" avg=").Append((ticks[i] * multiplier / counts[i]).ToString("F2", CultureInfo.InvariantCulture))
                .Append(" max=").Append((maxima[i] * multiplier).ToString("F2", CultureInfo.InvariantCulture));
        }
        System.Array.Clear(ticks, 0, ticks.Length);
        System.Array.Clear(maxima, 0, maxima.Length);
        System.Array.Clear(counts, 0, counts.Length);
        return report.ToString();
    }
}
