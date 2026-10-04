using System;

// Texture coordinates wrap because the original map uses a scrolling ring buffer.
internal sealed class MiniMapDirtyTiles
{
    internal const int TextureSize = 2048;
    internal const int TileSize = 64;
    private const int TilesPerSide = TextureSize / TileSize;
    private readonly bool[] dirty = new bool[TilesPerSide * TilesPerSide];
    private int cursor;
    internal int Count { get; private set; }

    internal void Mark(int x, int y, int width, int height, int halo = 12)
    {
        if (width <= 0 || height <= 0) return;
        int firstX = (int)Math.Floor((x - halo)/(double)TileSize);
        int lastX = (int)Math.Floor((x + width - 1 + halo)/(double)TileSize);
        int firstY = (int)Math.Floor((y - halo)/(double)TileSize);
        int lastY = (int)Math.Floor((y + height - 1 + halo)/(double)TileSize);
        for (int ty = firstY; ty <= lastY; ty++) for (int tx = firstX; tx <= lastX; tx++)
        {
            int key = Wrap(ty, TilesPerSide)*TilesPerSide + Wrap(tx, TilesPerSide);
            if (!dirty[key]) { dirty[key] = true; Count++; }
        }
    }

    internal bool Take(out int x, out int y)
    {
        x = y = 0;
        if (Count == 0) return false;
        for (int i = 0; i < dirty.Length; i++)
        {
            int key = cursor;
            cursor = (cursor + 1) % dirty.Length;
            if (!dirty[key]) continue;
            dirty[key] = false;
            Count--;
            x = key % TilesPerSide * TileSize;
            y = key / TilesPerSide * TileSize;
            return true;
        }
        return false;
    }

    internal void Clear() { Array.Clear(dirty, 0, dirty.Length); Count = cursor = 0; }
    internal static int Wrap(int value, int size) { int result = value % size; return result < 0 ? result + size : result; }
}
