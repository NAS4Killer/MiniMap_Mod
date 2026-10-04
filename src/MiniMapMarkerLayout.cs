using System;

// Pure projection shared by the renderer and its tests. The map spans 336 * zoom world metres.
internal static class MiniMapMarkerLayout
{
    internal static void Project(float dx, float dz, float heading, float size, float zoom,
        out float x, out float y)
    {
        double angle = heading * Math.PI / 180.0;
        float scale = size / (336f * zoom);
        x = (float)(Math.Cos(angle) * dx - Math.Sin(angle) * dz) * scale;
        y = (float)(Math.Sin(angle) * dx + Math.Cos(angle) * dz) * scale;
    }

    internal static bool Fits(float x, float y, float halfExtent, float size, bool circle)
    {
        float half = size * 0.5f;
        float farX = Math.Abs(x) + halfExtent;
        float farY = Math.Abs(y) + halfExtent;
        return circle ? farX * farX + farY * farY <= half * half
            : farX <= half && farY <= half;
    }
}
