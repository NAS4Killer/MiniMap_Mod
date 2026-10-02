using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace MiniMapMod
{
    public class Api : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            MiniMapPreferences.Initialize();
            Debug.Log("[MiniMap_Mod] ModAPI initialized");
        }
    }
}

internal static class MiniMapPreferences
{
    public const string Version = "0.0.5.1";
    public static bool Enabled = true;
    public static float Zoom = 1f;
    public static int ArrowSize = 40;
    public static int MapSize = 256;
    public static int FrameStyle;
    public static float Brightness = 1f;
    public static int PositionX = 20;
    public static int PositionY = -20;
    public static int DefaultPositionX = 20;
    public static int DefaultPositionY = -20;
    public static int PositionStep = 10;
    public static int MapShape;
    public static bool FogEnabled = true;
    public static bool MapTransparent;
    public static int TransparencyPercent = 30;
    public static bool EdgeFade;
    public static bool NorthUp = true;
    private static string SettingsPath;

    public static void Initialize()
    {
        SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "7DaysToDie", "MiniMap_Mod.cfg");
        Load();
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;
            foreach (string line in File.ReadAllLines(SettingsPath))
            {
                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2) continue;
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                if (key == "Enabled") bool.TryParse(value, out Enabled);
                else if (key == "Zoom") Zoom = ParseFloat(value, Zoom);
                else if (key == "ArrowSize") ArrowSize = ParseInt(value, ArrowSize);
                else if (key == "MapSize") MapSize = ParseInt(value, MapSize);
                else if (key == "FrameStyle") FrameStyle = ParseInt(value, FrameStyle);
                else if (key == "Brightness") Brightness = ParseFloat(value, Brightness);
                else if (key == "PositionX") PositionX = ParseInt(value, PositionX);
                else if (key == "PositionY") PositionY = ParseInt(value, PositionY);
                else if (key == "DefaultPositionX") DefaultPositionX = ParseInt(value, DefaultPositionX);
                else if (key == "DefaultPositionY") DefaultPositionY = ParseInt(value, DefaultPositionY);
                else if (key == "PositionStep") PositionStep = ParseInt(value, PositionStep);
                else if (key == "MapShape") MapShape = ParseInt(value, MapShape);
                else if (key == "FogEnabled") FogEnabled = ParseInt(value, FogEnabled ? 1 : 0) != 0;
                else if (key == "MapTransparent") MapTransparent = value == "1";
                else if (key == "TransparencyPercent") TransparencyPercent = ParseInt(value, TransparencyPercent);
                else if (key == "EdgeFade") EdgeFade = value == "1";
                else if (key == "NorthUp") NorthUp = value != "0";
            }
            Normalize();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[MiniMap_Mod] Could not load settings: " + exception.Message);
        }
    }

    public static void Save()
    {
        try
        {
            Normalize();
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllLines(SettingsPath, new[]
            {
                "Enabled=" + Enabled,
                "Zoom=" + Zoom.ToString("0.0", CultureInfo.InvariantCulture),
                "ArrowSize=" + ArrowSize,
                "MapSize=" + MapSize,
                "FrameStyle=" + FrameStyle,
                "Brightness=" + Brightness.ToString("0.0", CultureInfo.InvariantCulture),
                "PositionX=" + PositionX,
                "PositionY=" + PositionY,
                "DefaultPositionX=" + DefaultPositionX,
                "DefaultPositionY=" + DefaultPositionY,
                "PositionStep=" + PositionStep,
                "MapShape=" + MapShape,
                "FogEnabled=" + (FogEnabled ? "1" : "0"),
                "MapTransparent=" + (MapTransparent ? "1" : "0"),
                "TransparencyPercent=" + TransparencyPercent,
                "EdgeFade=" + (EdgeFade ? "1" : "0"),
                "NorthUp=" + (NorthUp ? "1" : "0")
            });
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[MiniMap_Mod] Could not save settings: " + exception.Message);
        }
    }

    private static void Normalize()
    {
        Zoom = Mathf.Clamp(Mathf.Round(Zoom / 2f) * 2f, 0f, 10f);
        ArrowSize = Mathf.Clamp(ArrowSize, 16, 80);
        MapSize = Mathf.Clamp(MapSize, 128, 480);
        FrameStyle = Mathf.Clamp(FrameStyle, 0, 5);
        Brightness = Mathf.Clamp(Brightness, 0.1f, 1f);
        if (PositionStep != 1 && PositionStep != 10 && PositionStep != 100) PositionStep = 10;
        MapShape = Mathf.Clamp(MapShape, 0, 1);
        TransparencyPercent = Mathf.Clamp(Mathf.RoundToInt(TransparencyPercent / 5f) * 5, 0, 50);
    }

    private static float ParseFloat(string value, float fallback)
    {
        float parsed;
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ? parsed : fallback;
    }

    private static int ParseInt(string value, int fallback)
    {
        int parsed;
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : fallback;
    }
}

public class XUiC_MiniMapArea : XUiC_MapArea
{
    public static XUiC_MiniMapArea Instance;
    private LocalPlayerCamera playerCamera;
    private bool centeredOnce;
    private XUiController frame;
    private XUiController clippingPanel;
    private XUiController background;
    private XUiController brightnessOverlay;
    private Vector2i lastPlayerChunk = new Vector2i(int.MinValue, int.MinValue);
    private Material originalMapMaterial;
    private Material transparentMapMaterial;
    private TextureWrapMode originalMapWrapMode;
    private bool makeMapOpaqueAfterRedraw;
    private float mapHeading;
    private Texture2D displayMapTexture;

    public override void Init()
    {
        Debug.Log("[MiniMap_Mod] XUiC_MiniMapArea.Init");
        base.Init();
        AlwaysUpdate = true;
        Instance = this;
        frame = GetChildById("miniMapFrame");
        clippingPanel = GetChildById("clippingPanel");
        background = GetChildById("backgroundMain");
        brightnessOverlay = GetChildById("brightnessOverlay");
        originalMapMaterial = xuiTexture.Material;
        originalMapWrapMode = mapTexture.wrapMode;
        InstallShapeGeometry();
        ApplyPreferences();
    }

    public override void OnOpen()
    {
        // XUiC_MapArea.OnOpen also opens the full-screen map, so only open this view tree.
        for (int i = 0; i < Children.Count; i++) Children[i].OnOpen();
        ViewComponent?.OnOpen();
        RefreshBindings();
        isOpen = true;
        localPlayer = xui.playerUI.entityPlayer;
        bFowMaskEnabled = !GameManager.Instance.IsEditMode();
        bFowMaskEnabled = MiniMapPreferences.FogEnabled && bFowMaskEnabled;
        initMap();
        if (localPlayer != null)
        {
            PositionMapAt(localPlayer.GetPosition());
            centeredOnce = true;
        }
        playerCamera = xui.playerUI.GetComponentInParent<LocalPlayerCamera>();
        if (playerCamera != null) playerCamera.PreRender += OnPreRender;
        if (crosshair != null) crosshair.IsVisible = true;
        ApplyPreferences();
    }

    public override void OnClose()
    {
        if (playerCamera != null)
        {
            playerCamera.PreRender -= OnPreRender;
            playerCamera = null;
        }
        for (int i = 0; i < Children.Count; i++) Children[i].OnClose();
        ViewComponent?.OnClose();
        isOpen = false;
    }

    public override void Update(float deltaTime)
    {
        bool redrawWasPending = bShouldRedrawMap;
        base.Update(deltaTime);
        if (redrawWasPending || makeMapOpaqueAfterRedraw || displayMapTexture == null)
        {
            MakeKnownMapOpaque();
            makeMapOpaqueAfterRedraw = false;
        }
        if (Input.GetKeyDown(KeyCode.F5)) ToggleSettings();

        if (xui.playerUI.windowManager.IsWindowOpen("miniMapSettings") && xui.playerUI.windowManager.IsWindowOpen("map"))
            xui.playerUI.windowManager.Close("map");

        bool escapeMenuOpen = xui.playerUI.windowManager.IsWindowOpen("ingameMenu");
        bool mapVisible = MiniMapPreferences.Enabled && !escapeMenuOpen;
        mapView.ViewComponent.IsVisible = mapVisible;
        frame.ViewComponent.IsVisible = mapVisible && MiniMapPreferences.FrameStyle != 0;

        EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
        if (player != null)
        {
            Vector2i currentPlayerChunk = World.toChunkXZ(player.position);
            if (currentPlayerChunk != lastPlayerChunk)
            {
                lastPlayerChunk = currentPlayerChunk;
                bShouldRedrawMap = true;
            }

            if (!centeredOnce)
            {
                PositionMapAt(player.position);
                centeredOnce = true;
            }
            else
            {
                mapMiddlePosPixel = new Vector2(player.position.x, player.position.z);
                positionMap();
            }
            UpdateMapRendering();
            float heading = MiniMapPreferences.NorthUp ? 0f : player.rotation.y;
            if (!Mathf.Approximately(mapHeading, heading))
            {
                mapHeading = heading;
                xuiTexture.uiTexture.MarkAsChanged();
            }
            if (crosshair != null) crosshair.UiTransform.localEulerAngles = new Vector3(0f, 0f, MiniMapPreferences.NorthUp ? -player.rotation.y : 0f);
        }
    }

    private void ToggleSettings()
    {
        GUIWindowManager manager = xui.playerUI.windowManager;
        if (manager.IsWindowOpen("miniMapSettings")) manager.Close("miniMapSettings");
        else
        {
            if (manager.IsWindowOpen("map")) manager.Close("map");
            manager.Open("miniMapSettings", true);
        }
    }

    public void ApplyPreferences()
    {
        int size = MiniMapPreferences.MapSize;
        int borderWidth = MiniMapPreferences.FrameStyle == 0 ? 0 : 5;
        int innerSize = size - borderWidth * 2;
        float brightness = MiniMapPreferences.Brightness;
        Vector2i screenSize = xui.GetXUiScreenSize();
        MiniMapPreferences.PositionX = Mathf.Clamp(MiniMapPreferences.PositionX, 0, Math.Max(0, screenSize.x - size));
        MiniMapPreferences.PositionY = Mathf.Clamp(MiniMapPreferences.PositionY, -Math.Max(0, screenSize.y - size), 0);

        ViewComponent.Size = new Vector2i(size, size);
        ViewComponent.Position = new Vector2i(MiniMapPreferences.PositionX, MiniMapPreferences.PositionY);
        mapView.ViewComponent.Size = new Vector2i(size, size);
        mapView.ViewComponent.IsVisible = MiniMapPreferences.Enabled;
        frame.ViewComponent.Size = new Vector2i(size, size);
        frame.ViewComponent.IsVisible = MiniMapPreferences.Enabled && MiniMapPreferences.FrameStyle != 0;
        if (frame.ViewComponent is XUiV_Sprite frameSprite) frameSprite.Color = GetFrameColor(MiniMapPreferences.FrameStyle);

        xuiTexture.Size = new Vector2i(innerSize, innerSize);
        xuiTexture.Position = new Vector2i(borderWidth, -borderWidth);
        xuiTexture.Color = new Color(brightness, brightness, brightness, 1f);
        background.ViewComponent.Size = new Vector2i(innerSize, innerSize);
        background.ViewComponent.Position = new Vector2i(borderWidth, -borderWidth);
        background.ViewComponent.IsVisible = MiniMapPreferences.FogEnabled;
        background.ViewComponent.UiTransform.gameObject.SetActive(false);
        brightnessOverlay.ViewComponent.Size = new Vector2i(innerSize, innerSize);
        brightnessOverlay.ViewComponent.Position = new Vector2i(borderWidth, -borderWidth);
        brightnessOverlay.ViewComponent.IsVisible = MiniMapPreferences.FogEnabled;
        brightnessOverlay.ViewComponent.UiTransform.gameObject.SetActive(false);
        if (brightnessOverlay.ViewComponent is XUiV_Sprite overlay)
            overlay.Color = new Color(0f, 0f, 0f, 1f - brightness);
        crosshair.Size = new Vector2i(MiniMapPreferences.ArrowSize, MiniMapPreferences.ArrowSize);
        crosshair.Position = new Vector2i(size / 2, -size / 2);
        crosshair.Color = Color.white;
        clippingPanel.ViewComponent.Size = new Vector2i(innerSize, innerSize);
        clippingPanel.ViewComponent.Position = new Vector2i(borderWidth, -borderWidth);
        if (clippingPanel.ViewComponent is XUiV_Panel panel)
        {
            panel.ClippingSize = new Vector2(innerSize, innerSize);
            panel.ClippingCenter = new Vector2(innerSize / 2f, -innerSize / 2f);
        }
        zoomScale = Mathf.Lerp(6.15f, 0.7f, MiniMapPreferences.Zoom / 10f);
        targetZoomScale = zoomScale;
        bFowMaskEnabled = MiniMapPreferences.FogEnabled && !GameManager.Instance.IsEditMode();
        makeMapOpaqueAfterRedraw = true;
        UpdateMapRendering();
        MarkShapeGeometryChanged();
        bShouldRedrawMap = true;
    }

    private static Color GetFrameColor(int style)
    {
        if (style == 1) return new Color32(0, 0, 0, 235);
        if (style == 2) return new Color32(255, 255, 255, 235);
        if (style == 3) return new Color32(190, 35, 35, 235);
        if (style == 4) return new Color32(35, 150, 65, 235);
        return new Color32(35, 90, 190, 235);
    }

    private void InstallShapeGeometry()
    {
        xuiTexture.uiTexture.onPostFill = FillMapGeometry;
        if (background.ViewComponent is XUiV_Sprite backgroundSprite)
            backgroundSprite.Sprite.onPostFill = FillMapGeometry;
        if (brightnessOverlay.ViewComponent is XUiV_Sprite overlaySprite)
            overlaySprite.Sprite.onPostFill = FillMapGeometry;
        if (frame.ViewComponent is XUiV_Sprite frameSprite)
            frameSprite.Sprite.onPostFill = FillCircleFrameWhenSelected;
    }

    private void UpdateMapRendering()
    {
        xuiTexture.GlobalOpacityModifier = 0f;
        float brightness = MiniMapPreferences.Brightness;
        xuiTexture.Color = new Color(brightness, brightness, brightness,
            MiniMapPreferences.MapTransparent ? 1f - MiniMapPreferences.TransparencyPercent / 100f : 1f);
        if (xuiTexture.Material != null && xuiTexture.Material != transparentMapMaterial)
            originalMapMaterial = xuiTexture.Material;
        if (transparentMapMaterial == null)
        {
            Shader shader = GlobalAssets.FindShader("Unlit/Transparent Colored");
            if (shader == null)
            {
                Debug.LogError("[MiniMap_Mod] Transparent map shader not found");
                return;
            }
            transparentMapMaterial = new Material(shader);
            transparentMapMaterial.name = "MiniMap_Mod.TransparentMap";
        }
        if (xuiTexture.Material != transparentMapMaterial) xuiTexture.Material = transparentMapMaterial;
        if (displayMapTexture != null) xuiTexture.Texture = displayMapTexture;
        xuiTexture.UVRect = new Rect(mapPos.x, mapPos.y, mapScale, mapScale);
    }

    private void MakeKnownMapOpaque()
    {
        Color32[] pixels = mapTexture.GetPixels32();
        int width = mapTexture.width, height = mapTexture.height;
        int[] distance = null;
        if (MiniMapPreferences.EdgeFade && !MiniMapPreferences.FogEnabled)
        {
            distance = new int[pixels.Length];
            for (int i = 0; i < distance.Length; i++) distance[i] = pixels[i].a == 0 ? 0 : 32;
            // Two distance passes fade known pixels toward their nearest unexplored neighbor.
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                if (x > 0) distance[i] = Math.Min(distance[i], distance[i - 1] + 1);
                if (y > 0) distance[i] = Math.Min(distance[i], distance[i - width] + 1);
            }
            for (int y = height - 1; y >= 0; y--) for (int x = width - 1; x >= 0; x--)
            {
                int i = y * width + x;
                if (x + 1 < width) distance[i] = Math.Min(distance[i], distance[i + 1] + 1);
                if (y + 1 < height) distance[i] = Math.Min(distance[i], distance[i + width] + 1);
            }
        }
        for (int i = 0; i < pixels.Length; i++)
        {
            if (MiniMapPreferences.FogEnabled)
            {
                float a = pixels[i].a / 255f;
                pixels[i].r = (byte)Mathf.Lerp(210f, pixels[i].r, a);
                pixels[i].g = (byte)Mathf.Lerp(210f, pixels[i].g, a);
                pixels[i].b = (byte)Mathf.Lerp(210f, pixels[i].b, a);
                pixels[i].a = 255;
            }
            else if (pixels[i].a > 0)
                pixels[i].a = distance == null ? (byte)255 : (byte)(255f * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(distance[i] / 12f)));
        }
        if (displayMapTexture == null)
        {
            displayMapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            displayMapTexture.wrapMode = TextureWrapMode.Repeat;
        }
        displayMapTexture.SetPixels32(pixels);
        displayMapTexture.Apply(false, false);
    }

    private void MarkShapeGeometryChanged()
    {
        xuiTexture.uiTexture.MarkAsChanged();
        if (background.ViewComponent is XUiV_Sprite backgroundSprite) backgroundSprite.Sprite.MarkAsChanged();
        if (brightnessOverlay.ViewComponent is XUiV_Sprite overlaySprite) overlaySprite.Sprite.MarkAsChanged();
        if (frame.ViewComponent is XUiV_Sprite frameSprite) frameSprite.Sprite.MarkAsChanged();
    }

    // Keep the viewport fixed; rotate sample coordinates around the player.
    private void FillMapGeometry(UIWidget widget, int offset, List<Vector3> verts, List<Vector2> uvs, List<Color> colors)
    {
        if (verts.Count <= offset) return;
        bool isMap = widget == xuiTexture.uiTexture;
        float left = float.MaxValue, bottom = float.MaxValue, right = float.MinValue, top = float.MinValue;
        float u0 = float.MaxValue, v0 = float.MaxValue, u1 = float.MinValue, v1 = float.MinValue;
        for (int i = offset; i < verts.Count; i++)
        {
            left = Mathf.Min(left, verts[i].x); right = Mathf.Max(right, verts[i].x);
            bottom = Mathf.Min(bottom, verts[i].y); top = Mathf.Max(top, verts[i].y);
            u0 = Mathf.Min(u0, uvs[i].x); u1 = Mathf.Max(u1, uvs[i].x);
            v0 = Mathf.Min(v0, uvs[i].y); v1 = Mathf.Max(v1, uvs[i].y);
        }
        Vector4 d = new Vector4(left, bottom, right, top);
        Rect uv = new Rect(u0, v0, u1-u0, v1-v0);
        Color tint = colors[offset];
        verts.RemoveRange(offset, verts.Count - offset);
        uvs.RemoveRange(offset, uvs.Count - offset);
        colors.RemoveRange(offset, colors.Count - offset);
        const int segments = 256;
        bool viewportFade = MiniMapPreferences.EdgeFade && MiniMapPreferences.FogEnabled;
        int rings = viewportFade ? 12 : 1;
        float angle = isMap ? -mapHeading * Mathf.Deg2Rad : 0f;
        float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
        for (int ring = 0; ring < rings; ring++)
        {
            float inner = ring == 0 ? 0f : 0.8f + 0.2f * (ring - 1) / (rings - 1);
            float outer = rings == 1 ? 1f : 0.8f + 0.2f * ring / (rings - 1);
            for (int s = 0; s < segments; s++)
            {
                for (int corner = 0; corner < 4; corner++)
                {
                    float a = (s + (corner >= 2 ? 1 : 0)) * Mathf.PI * 2f / segments;
                    float radius = corner == 0 || corner == 3 ? outer : inner;
                    float x = Mathf.Cos(a), y = Mathf.Sin(a);
                    if (MiniMapPreferences.MapShape == 0)
                    {
                        float divisor = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                        x /= divisor; y /= divisor;
                    }
                    x *= radius * 0.5f; y *= radius * 0.5f;
                    verts.Add(new Vector3(Mathf.Lerp(d.x, d.z, x + 0.5f), Mathf.Lerp(d.y, d.w, y + 0.5f)));
                    uvs.Add(new Vector2(uv.x + (0.5f + cos * x - sin * y) * uv.width,
                        uv.y + (0.5f + sin * x + cos * y) * uv.height));
                    Color c = tint;
                    if (viewportFade) c.a *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((radius - 0.8f) / 0.2f));
                    colors.Add(c);
                }
            }
        }
    }

    private static void FillCircleWhenSelected(UIWidget widget, int offset, List<Vector3> verts, List<Vector2> uvs, List<Color> colors)
    {
        if (MiniMapPreferences.MapShape != 1 || verts.Count <= offset || uvs.Count <= offset || colors.Count <= offset) return;

        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        float minU = float.MaxValue, maxU = float.MinValue, minV = float.MaxValue, maxV = float.MinValue;
        Color color = colors[offset];
        for (int i = offset; i < verts.Count; i++)
        {
            minX = Mathf.Min(minX, verts[i].x); maxX = Mathf.Max(maxX, verts[i].x);
            minY = Mathf.Min(minY, verts[i].y); maxY = Mathf.Max(maxY, verts[i].y);
            minU = Mathf.Min(minU, uvs[i].x); maxU = Mathf.Max(maxU, uvs[i].x);
            minV = Mathf.Min(minV, uvs[i].y); maxV = Mathf.Max(maxV, uvs[i].y);
        }
        verts.RemoveRange(offset, verts.Count - offset);
        uvs.RemoveRange(offset, uvs.Count - offset);
        colors.RemoveRange(offset, colors.Count - offset);

        const int strips = 512;
        for (int strip = 0; strip < strips; strip++)
        {
            float y0 = strip / (float)strips;
            float y1 = (strip + 1) / (float)strips;
            float ny0 = y0 * 2f - 1f;
            float ny1 = y1 * 2f - 1f;
            float halfWidth = Mathf.Min(Mathf.Sqrt(Mathf.Max(0f, 1f - ny0 * ny0)), Mathf.Sqrt(Mathf.Max(0f, 1f - ny1 * ny1))) * 0.5f;
            float x0 = 0.5f - halfWidth;
            float x1 = 0.5f + halfWidth;
            AddQuad(verts, uvs, colors, minX, maxX, minY, maxY, minU, maxU, minV, maxV, x0, x1, y0, y1, color);
        }
    }

    private static void FillCircleFrameWhenSelected(UIWidget widget, int offset, List<Vector3> verts, List<Vector2> uvs, List<Color> colors)
    {
        if (MiniMapPreferences.MapShape != 1 || verts.Count <= offset || uvs.Count <= offset || colors.Count <= offset) return;

        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        Vector2 sampleUv = uvs[offset];
        Color color = colors[offset];
        for (int i = offset; i < verts.Count; i++)
        {
            minX = Mathf.Min(minX, verts[i].x); maxX = Mathf.Max(maxX, verts[i].x);
            minY = Mathf.Min(minY, verts[i].y); maxY = Mathf.Max(maxY, verts[i].y);
        }
        verts.RemoveRange(offset, verts.Count - offset);
        uvs.RemoveRange(offset, uvs.Count - offset);
        colors.RemoveRange(offset, colors.Count - offset);

        const int segments = 512;
        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;
        float outerX = (maxX - minX) * 0.5f;
        float outerY = (maxY - minY) * 0.5f;
        float innerX = Mathf.Max(0f, outerX - 5f);
        float innerY = Mathf.Max(0f, outerY - 5f);
        for (int segment = 0; segment < segments; segment++)
        {
            float a0 = segment * Mathf.PI * 2f / segments;
            float a1 = (segment + 1) * Mathf.PI * 2f / segments;
            verts.Add(new Vector3(centerX + Mathf.Cos(a0) * outerX, centerY + Mathf.Sin(a0) * outerY));
            verts.Add(new Vector3(centerX + Mathf.Cos(a0) * innerX, centerY + Mathf.Sin(a0) * innerY));
            verts.Add(new Vector3(centerX + Mathf.Cos(a1) * innerX, centerY + Mathf.Sin(a1) * innerY));
            verts.Add(new Vector3(centerX + Mathf.Cos(a1) * outerX, centerY + Mathf.Sin(a1) * outerY));
            for (int i = 0; i < 4; i++) { uvs.Add(sampleUv); colors.Add(color); }
        }
    }

    private static void AddQuad(List<Vector3> verts, List<Vector2> uvs, List<Color> colors,
        float minX, float maxX, float minY, float maxY, float minU, float maxU, float minV, float maxV,
        float x0, float x1, float y0, float y1, Color color)
    {
        verts.Add(new Vector3(Mathf.Lerp(minX, maxX, x0), Mathf.Lerp(minY, maxY, y0)));
        verts.Add(new Vector3(Mathf.Lerp(minX, maxX, x0), Mathf.Lerp(minY, maxY, y1)));
        verts.Add(new Vector3(Mathf.Lerp(minX, maxX, x1), Mathf.Lerp(minY, maxY, y1)));
        verts.Add(new Vector3(Mathf.Lerp(minX, maxX, x1), Mathf.Lerp(minY, maxY, y0)));
        uvs.Add(new Vector2(Mathf.Lerp(minU, maxU, x0), Mathf.Lerp(minV, maxV, y0)));
        uvs.Add(new Vector2(Mathf.Lerp(minU, maxU, x0), Mathf.Lerp(minV, maxV, y1)));
        uvs.Add(new Vector2(Mathf.Lerp(minU, maxU, x1), Mathf.Lerp(minV, maxV, y1)));
        uvs.Add(new Vector2(Mathf.Lerp(minU, maxU, x1), Mathf.Lerp(minV, maxV, y0)));
        for (int i = 0; i < 4; i++) colors.Add(color);
    }

    public override void updateMapObjects() { }

    public override void Cleanup()
    {
        if (playerCamera != null)
        {
            playerCamera.PreRender -= OnPreRender;
            playerCamera = null;
        }
        if (Instance == this) Instance = null;
        if (displayMapTexture != null) UnityEngine.Object.Destroy(displayMapTexture);
        if (transparentMapMaterial != null)
        {
            UnityEngine.Object.Destroy(transparentMapMaterial);
            transparentMapMaterial = null;
        }
        base.Cleanup();
    }
}

public class XUiC_MiniMapSettings : XUiController
{
    private static readonly string[] FrameNames = { "Rahmenlos", "Schwarz", "Weiß", "Rot", "Grün", "Blau" };

    public override void Init()
    {
        base.Init();
        Bind("miniMapToggle", delegate { MiniMapPreferences.Enabled = !MiniMapPreferences.Enabled; Changed(); });
        Bind("zoomDown", delegate { MiniMapPreferences.Zoom -= 2f; Changed(); });
        Bind("zoomUp", delegate { MiniMapPreferences.Zoom += 2f; Changed(); });
        Bind("arrowDown", delegate { MiniMapPreferences.ArrowSize -= 4; Changed(); });
        Bind("arrowUp", delegate { MiniMapPreferences.ArrowSize += 4; Changed(); });
        Bind("sizeDown", delegate { MiniMapPreferences.MapSize -= 32; Changed(); });
        Bind("sizeUp", delegate { MiniMapPreferences.MapSize += 32; Changed(); });
        Bind("frameCycle", delegate { MiniMapPreferences.FrameStyle = (MiniMapPreferences.FrameStyle + 1) % FrameNames.Length; Changed(); });
        Bind("brightnessDown", delegate { MiniMapPreferences.Brightness -= 0.1f; Changed(); });
        Bind("brightnessUp", delegate { MiniMapPreferences.Brightness += 0.1f; Changed(); });
        Bind("mapShape", delegate { MiniMapPreferences.MapShape = 1 - MiniMapPreferences.MapShape; Changed(); });
        Bind("fogToggle", delegate { MiniMapPreferences.FogEnabled = !MiniMapPreferences.FogEnabled; Changed(); });
        Bind("transparencyToggle", delegate { MiniMapPreferences.MapTransparent = !MiniMapPreferences.MapTransparent; Changed(); });
        Bind("transparencyDown", delegate { MiniMapPreferences.TransparencyPercent -= 5; Changed(); });
        Bind("transparencyUp", delegate { MiniMapPreferences.TransparencyPercent += 5; Changed(); });
        Bind("edgeFadeToggle", delegate { MiniMapPreferences.EdgeFade = !MiniMapPreferences.EdgeFade; Changed(); });
        Bind("northToggle", delegate { MiniMapPreferences.NorthUp = !MiniMapPreferences.NorthUp; Changed(); });
        Bind("positionLeft", delegate { MiniMapPreferences.PositionX -= MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionRight", delegate { MiniMapPreferences.PositionX += MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionUp", delegate { MiniMapPreferences.PositionY += MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionDown", delegate { MiniMapPreferences.PositionY -= MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionReset", delegate { MiniMapPreferences.PositionX = MiniMapPreferences.DefaultPositionX; MiniMapPreferences.PositionY = MiniMapPreferences.DefaultPositionY; Changed(); });
        Bind("positionSave", delegate { MiniMapPreferences.DefaultPositionX = MiniMapPreferences.PositionX; MiniMapPreferences.DefaultPositionY = MiniMapPreferences.PositionY; Changed(); });
        Bind("positionStep1", delegate { MiniMapPreferences.PositionStep = 1; Changed(); });
        Bind("positionStep10", delegate { MiniMapPreferences.PositionStep = 10; Changed(); });
        Bind("positionStep100", delegate { MiniMapPreferences.PositionStep = 100; Changed(); });
        Bind("miniMapClose", delegate { xui.playerUI.windowManager.Close("miniMapSettings"); });
        Bind("miniMapUpdate", delegate { if (MiniMapUpdater.Press()) Application.OpenURL("https://github.com/NAS4Killer/MiniMap_Mod"); });
        RefreshValues();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        MiniMapUpdater.EnsureChecked();
        RefreshValues();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        SetButtonText("miniMapUpdate", MiniMapUpdater.Caption);
        if (GetChildById("miniMapUpdate")?.GetChildById("btnLabel")?.ViewComponent is XUiV_Label updateLabel)
            updateLabel.Color = MiniMapUpdater.IsGreen ? new Color32(70, 230, 70, 255) : Color.white;
        SetLabel("miniMapUpdateStatus", MiniMapUpdater.Status);
    }

    private void Bind(string id, Action action)
    {
        XUiController controller = GetChildById(id);
        if (controller is XUiC_SimpleButton simpleButton)
            simpleButton.OnPressed += delegate(XUiController sender, int mouseButton) { action(); };
        else
            controller.OnPress += delegate(XUiController sender, int mouseButton) { action(); };
    }

    private void Changed()
    {
        MiniMapPreferences.Save();
        XUiC_MiniMapArea.Instance?.ApplyPreferences();
        MiniMapPreferences.Save();
        RefreshValues();
    }

    private void RefreshValues()
    {
        SetLabel("miniMapVersion", "Version " + MiniMapPreferences.Version);
        SetButtonText("miniMapToggle", MiniMapPreferences.Enabled ? "AN" : "AUS");
        SetLabel("zoomValue", MiniMapPreferences.Zoom.ToString("0", CultureInfo.InvariantCulture) + "x");
        SetLabel("arrowValue", MiniMapPreferences.ArrowSize.ToString(CultureInfo.InvariantCulture));
        SetLabel("sizeValue", MiniMapPreferences.MapSize.ToString(CultureInfo.InvariantCulture));
        SetButtonText("frameCycle", FrameNames[MiniMapPreferences.FrameStyle]);
        SetLabel("brightnessValue", Mathf.RoundToInt(MiniMapPreferences.Brightness * 100f) + "%");
        SetButtonText("mapShape", MiniMapPreferences.MapShape == 1 ? "Kreis" : "Quadrat");
        SetButtonText("fogToggle", MiniMapPreferences.FogEnabled ? "AN" : "AUS");
        SetButtonText("transparencyToggle", MiniMapPreferences.MapTransparent ? "AN" : "AUS");
        SetLabel("transparencyValue", MiniMapPreferences.TransparencyPercent + "%");
        SetButtonText("edgeFadeToggle", MiniMapPreferences.EdgeFade ? "AN" : "AUS");
        SetButtonText("northToggle", MiniMapPreferences.NorthUp ? "AN" : "AUS");
        SetButtonText("positionStep1", MiniMapPreferences.PositionStep == 1 ? "[1 px]" : "1 px");
        SetButtonText("positionStep10", MiniMapPreferences.PositionStep == 10 ? "[10 px]" : "10 px");
        SetButtonText("positionStep100", MiniMapPreferences.PositionStep == 100 ? "[100 px]" : "100 px");
    }

    private void SetLabel(string id, string value)
    {
        XUiController controller = GetChildById(id);
        if (controller?.ViewComponent is XUiV_Label label) label.Text = value;
    }

    private void SetButtonText(string id, string value)
    {
        if (GetChildById(id) is XUiC_SimpleButton button) button.Text = value;
    }
}
