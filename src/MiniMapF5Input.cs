// Single presses wait briefly so a double press never toggles map visibility.
internal sealed class MiniMapF5Input
{
    private const float DoublePressSeconds = 0.3f;
    private bool pending;
    private float pressedAt;

    public void Reset() { pending = false; }

    // 0: no action, 1: toggle minimap, 2: toggle options.
    public int Update(float now, bool pressed, bool toggleMode, bool menuOpen, bool allowed)
    {
        if (!allowed) { Reset(); return 0; }
        if (!toggleMode || menuOpen)
        {
            Reset();
            return pressed ? 2 : 0;
        }
        if (pressed && pending && now - pressedAt <= DoublePressSeconds)
        {
            Reset();
            return 2;
        }
        bool expired = pending && now - pressedAt > DoublePressSeconds;
        if (expired) Reset();
        if (pressed) { pending = true; pressedAt = now; }
        return expired ? 1 : 0;
    }
}
