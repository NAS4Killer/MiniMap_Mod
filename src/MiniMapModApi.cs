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
    public const string Version = "0.2.0.0";
    public static bool F5ToggleMode;
    public static bool Enabled = true;
    public static float Zoom = 1f;
    public static int ArrowSize = 40;
    public static int ArrowColor;
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
    public static int TransparencyPercent = 6;
    public static bool EdgeFade;
    public static bool NorthUp = true;
    public static bool CoordinatesEnabled;
    public static bool CoordinatesAbove;
    public static bool DirectionsEnabled;
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
                else if (key == "F5ToggleMode") F5ToggleMode = value == "1";
                else if (key == "Zoom") Zoom = ParseFloat(value, Zoom);
                else if (key == "ArrowSize") ArrowSize = ParseInt(value, ArrowSize);
                else if (key == "ArrowColor") ArrowColor = ParseInt(value, ArrowColor);
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
                else if (key == "CoordinatesEnabled") CoordinatesEnabled = value == "1";
                else if (key == "CoordinatesAbove") CoordinatesAbove = value == "1";
                else if (key == "DirectionsEnabled") DirectionsEnabled = value == "1";
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
                "F5ToggleMode=" + (F5ToggleMode ? "1" : "0"),
                "Zoom=" + Zoom.ToString("0.0", CultureInfo.InvariantCulture),
                "ArrowSize=" + ArrowSize,
                "ArrowColor=" + ArrowColor,
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
                "NorthUp=" + (NorthUp ? "1" : "0"),
                "CoordinatesEnabled=" + (CoordinatesEnabled ? "1" : "0"),
                "CoordinatesAbove=" + (CoordinatesAbove ? "1" : "0"),
                "DirectionsEnabled=" + (DirectionsEnabled ? "1" : "0")
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
        ArrowColor = Mathf.Clamp(ArrowColor, 0, 4);
        MapSize = Mathf.Clamp(MapSize, 128, 480);
        FrameStyle = Mathf.Clamp(FrameStyle, 0, 4);
        Brightness = Mathf.Clamp(Brightness, 0.1f, 1f);
        if (PositionStep != 1 && PositionStep != 10 && PositionStep != 100) PositionStep = 10;
        MapShape = Mathf.Clamp(MapShape, 0, 1);
        TransparencyPercent = Mathf.Clamp(Mathf.RoundToInt(TransparencyPercent / 2f) * 2, 2, 20);
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
    private XUiV_Label coordinatesLabel;
    private readonly XUiV_Label[] directionLabels = new XUiV_Label[4];
    private const int CoordinatesHeight = 20;
    private const int CoordinatesGap = 4;
    private Vector2i lastPlayerChunk = new Vector2i(int.MinValue, int.MinValue);
    private Material originalMapMaterial;
    private Material transparentMapMaterial;
    private TextureWrapMode originalMapWrapMode;
    private bool makeMapOpaqueAfterRedraw;
    private float mapHeading;
    private Texture2D displayMapTexture;
    private readonly MiniMapF5Input f5Input = new MiniMapF5Input();
    private readonly MiniMapMarkers miniMapMarkers = new MiniMapMarkers();
    private readonly MiniMapPixelProcessor pixelProcessor = new MiniMapPixelProcessor();
    private bool pixelsDirty = true;
    private Vector2 pendingTextureCenter, pendingTextureScroll;
    private Vector2 displayTextureCenter, displayTextureScroll;
    private Color32[] pixelBuffer;
    private readonly MiniMapDirtyTiles dirtyTiles = new MiniMapDirtyTiles();
    private readonly MiniMapPixelProcessor tileProcessor = new MiniMapPixelProcessor();
    private readonly Color32[] tilePixels = new Color32[64 * 64];
    private readonly Color32[] tileWithHalo = new Color32[88 * 88];
    private Texture2D tileTexture;
    private bool copyTextureUnavailable;
    private bool settingsSnapshotRequested;
    private Texture2D alternateFogTexture;
    private bool preparedEdgeFade;

    public void BeginSettingsPreview()
    {
        settingsSnapshotRequested = true;
    }

    public void EndSettingsPreview()
    {
        if (pixelProcessor.Running) pixelsDirty = true;
        settingsSnapshotRequested = false;
        if (alternateFogTexture != null) UnityEngine.Object.Destroy(alternateFogTexture);
        alternateFogTexture = null;
        pixelProcessor.ReleaseScratch();
        // Resume the existing incremental exploration refresh after leaving the menu.
        lastPlayerChunk = new Vector2i(int.MinValue, int.MinValue);
    }

    public bool TogglePreparedFog()
    {
        if (alternateFogTexture == null || settingsSnapshotRequested) return false;
        Texture2D previous = displayMapTexture;
        displayMapTexture = alternateFogTexture;
        alternateFogTexture = previous;
        MiniMapPreferences.FogEnabled = !MiniMapPreferences.FogEnabled;
        UpdateMapRendering();
        return true;
    }

    private void PrepareSettingsSnapshot()
    {
        // This full preparation/upload is restricted to the open options menu.
        if (!settingsSnapshotRequested || !xui.playerUI.windowManager.IsWindowOpen("miniMapSettings")) return;
        pixelProcessor.Cancel();
        if (alternateFogTexture != null) UnityEngine.Object.Destroy(alternateFogTexture);
        alternateFogTexture = null;
        var source = new Color32[mapTexture.width * mapTexture.height];
        mapTexture.GetRawTextureData<Color32>().CopyTo(source);
        for (int variant = 0; variant < 2; variant++)
        {
            var pixels = (Color32[])source.Clone();
            bool fog = variant == 0 ? MiniMapPreferences.FogEnabled : !MiniMapPreferences.FogEnabled;
            pixelProcessor.Begin(pixels, mapTexture.width, fog, MiniMapPreferences.EdgeFade);
            pixelProcessor.Step(int.MaxValue, double.PositiveInfinity);
            Texture2D texture;
            if (variant == 0 && displayMapTexture != null) texture = displayMapTexture;
            else texture = new Texture2D(mapTexture.width, mapTexture.height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            if (variant == 0) displayMapTexture = texture;
            else alternateFogTexture = texture;
        }
        pixelProcessor.Cancel();
        pixelProcessor.ReleaseScratch();
        displayTextureCenter = mapMiddlePosChunks;
        displayTextureScroll = mapScrollTextureOffset;
        preparedEdgeFade = MiniMapPreferences.EdgeFade;
        settingsSnapshotRequested = false;
        pixelsDirty = false;
        makeMapOpaqueAfterRedraw = false;
        dirtyTiles.Clear();
    }

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
        coordinatesLabel = GetChildById("miniMapCoordinates")?.ViewComponent as XUiV_Label;
        string[] directionIds = { "miniMapNorth", "miniMapEast", "miniMapSouth", "miniMapWest" };
        for (int i = 0; i < directionLabels.Length; i++)
            directionLabels[i] = GetChildById(directionIds[i])?.ViewComponent as XUiV_Label;
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
        EndSettingsPreview();
        f5Input.Reset();
        pixelProcessor.Cancel();
        miniMapMarkers.Clear();
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
        GUIWindowManager inputManager = xui.playerUI.windowManager;
        int f5Action = f5Input.Update(Time.unscaledTime, Input.GetKeyDown(KeyCode.F5),
            MiniMapPreferences.F5ToggleMode, inputManager.IsWindowOpen("miniMapSettings"),
            isOpen && !inputManager.IsWindowOpen("ingameMenu"));
        if (f5Action == 2) ToggleSettings();
        else if (f5Action == 1)
        {
            MiniMapPreferences.Enabled = !MiniMapPreferences.Enabled;
            MiniMapPreferences.Save();
            ApplyPreferences();
        }

        if (xui.playerUI.windowManager.IsWindowOpen("miniMapSettings") && xui.playerUI.windowManager.IsWindowOpen("map"))
            xui.playerUI.windowManager.Close("map");

        bool escapeMenuOpen = xui.playerUI.windowManager.IsWindowOpen("ingameMenu");
        bool mapVisible = MiniMapPreferences.Enabled && !escapeMenuOpen;
        mapView.ViewComponent.IsVisible = mapVisible;
        frame.ViewComponent.IsVisible = mapVisible && MiniMapPreferences.FrameStyle != 0;

        EntityPlayerLocal player = GameManager.Instance?.World?.GetPrimaryPlayer();
        float playerHeading = MiniMapHeading.Get(player);
        if (crosshair != null) crosshair.Color = MiniMapArrowColor.Get(player, MiniMapPreferences.ArrowColor);
        UpdateCoordinates(player, mapVisible);
        UpdateDirections(player, mapVisible);
        if ((!MiniMapPreferences.Enabled && !settingsSnapshotRequested) || !isOpen || player == null)
        {
            if (isOpen) UpdateMapViews(deltaTime);
            miniMapMarkers.Update(xui, transformSpritesParent, prefabMapSprite, player, mapMiddlePosPixel,
                mapHeading, zoomScale, MiniMapPreferences.MapSize, false);
            return;
        }
        if (alternateFogTexture != null && !settingsSnapshotRequested)
        {
            // Keep both snapshots fixed only while the menu is open; no second map is maintained in play.
            mapHeading = MiniMapPreferences.NorthUp ? 0f : playerHeading;
            if (crosshair != null) crosshair.UiTransform.localEulerAngles = new Vector3(0f, 0f,
                MiniMapPreferences.NorthUp ? -playerHeading : 0f);
            xuiTexture.uiTexture.MarkAsChanged();
            UpdateMapRendering();
            int previewBorder = MiniMapPreferences.FrameStyle == 0 ? 0 : 5;
            miniMapMarkers.Update(xui, transformSpritesParent, prefabMapSprite, player, mapMiddlePosPixel,
                mapHeading, zoomScale, MiniMapPreferences.MapSize - previewBorder * 2, mapVisible && isOpen);
            UpdateMapViews(deltaTime);
            return;
        }
        bool redrawWasPending = bShouldRedrawMap;
        base.Update(deltaTime);
        if (redrawWasPending || makeMapOpaqueAfterRedraw) pixelsDirty = true;
        makeMapOpaqueAfterRedraw = false;
        if (player != null)
        {
            Vector2i currentPlayerChunk = World.toChunkXZ(player.position);
            if (currentPlayerChunk != lastPlayerChunk)
            {
                lastPlayerChunk = currentPlayerChunk;
                RefreshPlayerChunks(currentPlayerChunk, player.position);
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
            if (settingsSnapshotRequested) PrepareSettingsSnapshot();
            else MakeKnownMapOpaque();
            UpdateMapRendering();
            float heading = MiniMapPreferences.NorthUp ? 0f : playerHeading;
            if (!Mathf.Approximately(mapHeading, heading))
            {
                mapHeading = heading;
                xuiTexture.uiTexture.MarkAsChanged();
            }
            if (crosshair != null) crosshair.UiTransform.localEulerAngles = new Vector3(0f, 0f, MiniMapPreferences.NorthUp ? -playerHeading : 0f);
        }
        int markerBorder = MiniMapPreferences.FrameStyle == 0 ? 0 : 5;
        miniMapMarkers.Update(xui, transformSpritesParent, prefabMapSprite, player, mapMiddlePosPixel,
            mapHeading, zoomScale, MiniMapPreferences.MapSize - markerBorder * 2, mapVisible && isOpen);
    }

    private void UpdateMapViews(float deltaTime)
    {
        // Apply queued positions, sizes, colors and visibility without XUiC_MapArea's map work.
        if (ViewComponent != null && ViewComponent.IsVisible) ViewComponent.Update(deltaTime);
        for (int i = 0; i < Children.Count; i++) Children[i].Update(deltaTime);
    }

    private void UpdateCoordinates(EntityPlayerLocal player, bool mapVisible)
    {
        if (coordinatesLabel == null) return;
        coordinatesLabel.IsVisible = mapVisible && MiniMapPreferences.CoordinatesEnabled && player != null;
        if (!coordinatesLabel.IsVisible) return;
        Vector3 position = player.GetPosition();
        string text = string.Format(CultureInfo.InvariantCulture, "X: {0}  Y: {1}  Z: {2}",
            Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z), Mathf.FloorToInt(position.y));
        if (coordinatesLabel.Text != text) coordinatesLabel.Text = text;
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

    private void UpdateDirections(EntityPlayerLocal player, bool mapVisible)
    {
        bool visible = mapVisible && MiniMapPreferences.DirectionsEnabled && player != null;
        float heading = MiniMapPreferences.NorthUp ? 0f : MiniMapHeading.Get(player);
        float center = MiniMapPreferences.MapSize / 2f;
        float radius = Mathf.Max(0f, center - 18f);
        for (int i = 0; i < directionLabels.Length; i++)
        {
            XUiV_Label label = directionLabels[i];
            if (label == null) continue;
            label.IsVisible = visible;
            if (!visible) continue;
            float angle = (i * 90f - heading) * Mathf.Deg2Rad;
            label.Position = new Vector2i(Mathf.RoundToInt(center + Mathf.Sin(angle) * radius - 12f),
                Mathf.RoundToInt(-center + Mathf.Cos(angle) * radius + 12f));
        }
    }

    public void ApplyPreferences()
    {
        if (alternateFogTexture != null)
        {
            // Only the terrain edge fade changes the cached pixels; layout and brightness do not.
            if (preparedEdgeFade == MiniMapPreferences.EdgeFade)
            {
                ApplyLayoutPreferences();
                return;
            }
            settingsSnapshotRequested = true;
        }
        pixelProcessor.Cancel();
        pixelsDirty = true;
        ApplyLayoutPreferences();
        makeMapOpaqueAfterRedraw = true;
        bShouldRedrawMap = true;
    }

    private void ApplyLayoutPreferences()
    {
        int size = MiniMapPreferences.MapSize;
        int borderWidth = MiniMapPreferences.FrameStyle == 0 ? 0 : 5;
        int innerSize = size - borderWidth * 2;
        float brightness = MiniMapPreferences.Brightness;
        Vector2i screenSize = xui.GetXUiScreenSize();
        MiniMapPreferences.PositionX = Mathf.Clamp(MiniMapPreferences.PositionX, 0, Math.Max(0, screenSize.x - size));
        int coordinateSpace = MiniMapPreferences.CoordinatesEnabled ? CoordinatesHeight + CoordinatesGap : 0;
        int topSpace = MiniMapPreferences.CoordinatesAbove ? coordinateSpace : 0;
        int bottomSpace = MiniMapPreferences.CoordinatesAbove ? 0 : coordinateSpace;
        MiniMapPreferences.PositionY = Mathf.Clamp(MiniMapPreferences.PositionY,
            -Math.Max(topSpace, screenSize.y - size - bottomSpace), -topSpace);

        ViewComponent.Size = new Vector2i(size, size);
        ViewComponent.Position = new Vector2i(MiniMapPreferences.PositionX, MiniMapPreferences.PositionY);
        if (coordinatesLabel != null)
        {
            coordinatesLabel.Size = new Vector2i(size, CoordinatesHeight);
            coordinatesLabel.Position = new Vector2i(0, MiniMapPreferences.CoordinatesAbove
                ? CoordinatesHeight + CoordinatesGap : -size - CoordinatesGap);
            UpdateCoordinates(GameManager.Instance?.World?.GetPrimaryPlayer(),
                MiniMapPreferences.Enabled && !xui.playerUI.windowManager.IsWindowOpen("ingameMenu"));
        }
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
        crosshair.Color = MiniMapArrowColor.Get(GameManager.Instance?.World?.GetPrimaryPlayer(), MiniMapPreferences.ArrowColor);
        clippingPanel.ViewComponent.Size = new Vector2i(innerSize, innerSize);
        clippingPanel.ViewComponent.Position = new Vector2i(borderWidth, -borderWidth);
        if (clippingPanel.ViewComponent is XUiV_Panel panel)
        {
            panel.ClippingSize = new Vector2(innerSize, innerSize);
            panel.ClippingCenter = new Vector2(innerSize / 2f, -innerSize / 2f);
        }
        // Equal multiplicative changes in the visible magnification between menu steps.
        zoomScale = 6.15f * Mathf.Pow(0.35f / 6.15f, MiniMapPreferences.Zoom / 10f);
        targetZoomScale = zoomScale;
        mapScale = 336f * zoomScale / 2048f;
        // Always retain the source exploration alpha. FogEnabled changes only its presentation.
        // Disabling this mask makes previously masked pixels opaque and loses the transparency boundary.
        bFowMaskEnabled = !GameManager.Instance.IsEditMode();
        if (localPlayer != null) positionMap();
        UpdateMapRendering();
        MarkShapeGeometryChanged();
        UpdateDirections(GameManager.Instance?.World?.GetPrimaryPlayer(),
            MiniMapPreferences.Enabled && !xui.playerUI.windowManager.IsWindowOpen("ingameMenu"));
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
        // Map opacity is applied in FillMapGeometry, independently of the game's UI color handling.
        xuiTexture.Color = new Color(brightness, brightness, brightness, 1f);
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
        if (displayMapTexture != null)
        {
            xuiTexture.Texture = displayMapTexture;
            // XUiV_Texture applies its queued properties during the view update.
            // The prepared-menu branch skips base.Update, so bind the renderer now too.
            if (xuiTexture.uiTexture.mainTexture != displayMapTexture)
            {
                xuiTexture.uiTexture.mainTexture = displayMapTexture;
                xuiTexture.uiTexture.MarkAsChanged();
            }
        }
        if (displayMapTexture != null)
        {
            float halfMargin = (2048f - 336f * zoomScale) * 0.5f;
            xuiTexture.UVRect = new Rect(
                (halfMargin + mapMiddlePosPixel.x - displayTextureCenter.x) / 2048f + displayTextureScroll.x,
                (halfMargin + mapMiddlePosPixel.y - displayTextureCenter.y) / 2048f + displayTextureScroll.y,
                mapScale, mapScale);
        }
        else xuiTexture.UVRect = new Rect(mapPos.x, mapPos.y, mapScale, mapScale);
        // Apply UV changes directly as well: the cached-menu path skips the normal view update.
        if (xuiTexture.uiTexture.uvRect != xuiTexture.UVRect)
        {
            xuiTexture.uiTexture.uvRect = xuiTexture.UVRect;
            xuiTexture.uiTexture.MarkAsChanged();
        }
    }

    private void RefreshPlayerChunks(Vector2i chunk, Vector3 position)
    {
        int dx = chunk.x - Mathf.FloorToInt(mapMiddlePosChunks.x / 16f);
        int dz = chunk.y - Mathf.FloorToInt(mapMiddlePosChunks.y / 16f);
        if (Math.Abs(dx) >= 128 || Math.Abs(dz) >= 128)
        {
            PositionMapAt(position);
            pixelsDirty = true;
            return;
        }
        if (dx != 0 || dz != 0)
        {
            int oldScrollX = mapScrollTextureChunksOffsetX * 16;
            int oldScrollZ = mapScrollTextureChunksOffsetZ * 16;
            mapMiddlePosChunks += new Vector2(dx * 16f, dz * 16f);
            updateMapForScroll(dx, dz);
            if (dx != 0) dirtyTiles.Mark(dx > 0 ? oldScrollX : mapScrollTextureChunksOffsetX * 16,
                0, Math.Abs(dx) * 16, 2048);
            if (dz != 0) dirtyTiles.Mark(0,
                dz > 0 ? oldScrollZ : mapScrollTextureChunksOffsetZ * 16, 2048, Math.Abs(dz) * 16);
        }
        // Refresh the explored chunk and its neighbours: their fog boundaries can also change.
        int startX = (chunk.x - 1) * 16, startZ = (chunk.y - 1) * 16;
        int textureX = Utils.WrapIndex(startX - (int)mapMiddlePosChunks.x + 1024 + mapScrollTextureChunksOffsetX * 16, 2048);
        int textureZ = Utils.WrapIndex(startZ - (int)mapMiddlePosChunks.y + 1024 + mapScrollTextureChunksOffsetZ * 16, 2048);
        updateMapSection(startX, startZ, startX + 48, startZ + 48,
            textureX, textureZ, Utils.WrapIndex(textureX + 48, 2048), Utils.WrapIndex(textureZ + 48, 2048));
        dirtyTiles.Mark(textureX, textureZ, 48, 48);
        mapTexture.Apply(false, false);
    }

    private void MakeKnownMapOpaque()
    {
        if (!pixelProcessor.Running)
        {
            if (!pixelsDirty && displayMapTexture != null)
            {
                UpdateDirtyTiles();
                return;
            }
            if (pixelBuffer == null || pixelBuffer.Length != mapTexture.width * mapTexture.height)
                pixelBuffer = new Color32[mapTexture.width * mapTexture.height];
            mapTexture.GetRawTextureData<Color32>().CopyTo(pixelBuffer);
            pixelProcessor.Begin(pixelBuffer, mapTexture.width,
                MiniMapPreferences.FogEnabled, MiniMapPreferences.EdgeFade);
            pendingTextureCenter = mapMiddlePosChunks;
            pendingTextureScroll = mapScrollTextureOffset;
            pixelsDirty = false;
            dirtyTiles.Clear();
        }
        bool finished = pixelProcessor.Step();
        if (!finished) return;
        if (displayMapTexture == null)
        {
            displayMapTexture = new Texture2D(mapTexture.width, mapTexture.height, TextureFormat.RGBA32, false);
            displayMapTexture.wrapMode = TextureWrapMode.Repeat;
        }
        displayMapTexture.SetPixels32(pixelProcessor.Pixels);
        displayMapTexture.Apply(false, false);
        displayTextureCenter = pendingTextureCenter;
        displayTextureScroll = pendingTextureScroll;
        pixelProcessor.Cancel();
    }

    private void UpdateDirtyTiles()
    {
        if (dirtyTiles.Count == 0) return;
        const int tileSize = 64, halo = 12, readSize = tileSize + halo * 2;
        long batchStart = System.Diagnostics.Stopwatch.GetTimestamp();
        var raw = mapTexture.GetRawTextureData<Color32>();
        bool useHalo = MiniMapPreferences.EdgeFade && !MiniMapPreferences.FogEnabled;
        for (int tile = 0; tile < 4 && dirtyTiles.Take(out int x, out int y); tile++)
        {
            int readWidth = useHalo ? readSize : tileSize;
            int margin = useHalo ? halo : 0;
            Color32[] buffer = useHalo ? tileWithHalo : tilePixels;
            for (int row = 0; row < readWidth; row++)
            {
                int sourceY = MiniMapDirtyTiles.Wrap(y + row - margin, 2048);
                int sourceX = MiniMapDirtyTiles.Wrap(x - margin, 2048);
                int first = Math.Min(readWidth, 2048 - sourceX);
                Unity.Collections.NativeArray<Color32>.Copy(raw, sourceY * 2048 + sourceX, buffer, row * readWidth, first);
                if (first < readWidth)
                    Unity.Collections.NativeArray<Color32>.Copy(raw, sourceY * 2048, buffer, row * readWidth + first, readWidth - first);
            }
            tileProcessor.Begin(buffer, readWidth, MiniMapPreferences.FogEnabled, useHalo);
            tileProcessor.Step(40000, double.PositiveInfinity);
            if (useHalo)
                for (int row = 0; row < tileSize; row++)
                    Array.Copy(buffer, (row + halo) * readSize + halo, tilePixels, row * tileSize, tileSize);
            // Keep the CPU copy current too, so a fallback or later full upload cannot restore stale pixels.
            displayMapTexture.SetPixels32(x, y, tileSize, tileSize, tilePixels);
            if (!copyTextureUnavailable && (SystemInfo.copyTextureSupport & UnityEngine.Rendering.CopyTextureSupport.Basic) != 0)
            {
                if (tileTexture == null) tileTexture = new Texture2D(tileSize, tileSize, TextureFormat.RGBA32, false);
                tileTexture.SetPixels32(tilePixels);
                tileTexture.Apply(false, false);
                try { Graphics.CopyTexture(tileTexture, 0, 0, 0, 0, tileSize, tileSize, displayMapTexture, 0, 0, x, y); }
                catch (Exception exception)
                {
                    copyTextureUnavailable = true;
                    Debug.LogWarning("[MiniMap_Mod] Partial GPU copy unavailable; using full upload: " + exception.Message);
                    displayMapTexture.Apply(false, false);
                }
            }
            else displayMapTexture.Apply(false, false);
            displayTextureCenter = mapMiddlePosChunks;
            displayTextureScroll = mapScrollTextureOffset;
            tileProcessor.Cancel();
            if ((System.Diagnostics.Stopwatch.GetTimestamp() - batchStart) * 1000.0 / System.Diagnostics.Stopwatch.Frequency >= 2.0) break;
        }
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
        tint = MiniMapRenderTint.Apply(tint, isMap, MiniMapPreferences.MapTransparent,
            MiniMapPreferences.TransparencyPercent);
        verts.RemoveRange(offset, verts.Count - offset);
        uvs.RemoveRange(offset, uvs.Count - offset);
        colors.RemoveRange(offset, colors.Count - offset);
        const int segments = 256;
        bool viewportFade = MiniMapPreferences.EdgeFade;
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
        if (MiniMapPreferences.MapShape == 0)
        {
            MiniMapBorderGeometry.Fill(widget, offset, verts, uvs, colors, 5f);
            return;
        }
        if (verts.Count <= offset || uvs.Count <= offset || colors.Count <= offset) return;

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
        EndSettingsPreview();
        pixelProcessor.Cancel();
        tileProcessor.Cancel();
        dirtyTiles.Clear();
        if (tileTexture != null) UnityEngine.Object.Destroy(tileTexture);
        miniMapMarkers.Clear();
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
    private string positionHint = "";

    public override void Init()
    {
        base.Init();
        Bind("miniMapToggle", delegate { MiniMapPreferences.Enabled = !MiniMapPreferences.Enabled; Changed(); });
        Bind("f5Mode", delegate { MiniMapPreferences.F5ToggleMode = !MiniMapPreferences.F5ToggleMode; Changed(); });
        Bind("zoomDown", delegate { MiniMapPreferences.Zoom -= 2f; Changed(); });
        Bind("zoomUp", delegate { MiniMapPreferences.Zoom += 2f; Changed(); });
        Bind("arrowDown", delegate { MiniMapPreferences.ArrowSize -= 4; Changed(); });
        Bind("arrowUp", delegate { MiniMapPreferences.ArrowSize += 4; Changed(); });
        Bind("arrowYellow", delegate { MiniMapPreferences.ArrowColor = 1; Changed(); });
        Bind("arrowWhite", delegate { MiniMapPreferences.ArrowColor = 2; Changed(); });
        Bind("arrowGreen", delegate { MiniMapPreferences.ArrowColor = 4; Changed(); });
        Bind("arrowRed", delegate { MiniMapPreferences.ArrowColor = 3; Changed(); });
        Bind("arrowOriginal", delegate { MiniMapPreferences.ArrowColor = 0; Changed(); });
        Bind("sizeDown", delegate { MiniMapPreferences.MapSize -= 32; Changed(); });
        Bind("sizeUp", delegate { MiniMapPreferences.MapSize += 32; Changed(); });
        Bind("frameBlack", delegate { MiniMapPreferences.FrameStyle = 1; Changed(); });
        Bind("frameWhite", delegate { MiniMapPreferences.FrameStyle = 2; Changed(); });
        Bind("frameGreen", delegate { MiniMapPreferences.FrameStyle = 4; Changed(); });
        Bind("frameRed", delegate { MiniMapPreferences.FrameStyle = 3; Changed(); });
        Bind("frameNone", delegate { MiniMapPreferences.FrameStyle = 0; Changed(); });
        Bind("brightnessDown", delegate { MiniMapPreferences.Brightness -= 0.1f; Changed(); });
        Bind("brightnessUp", delegate { MiniMapPreferences.Brightness += 0.1f; Changed(); });
        Bind("mapShape", delegate { MiniMapPreferences.MapShape = 1 - MiniMapPreferences.MapShape; Changed(); });
        Bind("fogToggle", delegate
        {
            if (XUiC_MiniMapArea.Instance?.TogglePreparedFog() == true)
            {
                MiniMapPreferences.Save();
                RefreshValues();
            }
        });
        Bind("transparencyToggle", delegate { MiniMapPreferences.MapTransparent = !MiniMapPreferences.MapTransparent; Changed(); });
        Bind("transparencyDown", delegate { MiniMapPreferences.TransparencyPercent -= 2; Changed(); });
        Bind("transparencyUp", delegate { MiniMapPreferences.TransparencyPercent += 2; Changed(); });
        Bind("edgeFadeToggle", delegate { MiniMapPreferences.EdgeFade = !MiniMapPreferences.EdgeFade; Changed(); });
        Bind("northToggle", delegate { MiniMapPreferences.NorthUp = !MiniMapPreferences.NorthUp; Changed(); });
        Bind("coordinatesToggle", delegate { MiniMapPreferences.CoordinatesEnabled = !MiniMapPreferences.CoordinatesEnabled; Changed(); });
        Bind("coordinatesPosition", delegate { MiniMapPreferences.CoordinatesAbove = !MiniMapPreferences.CoordinatesAbove; Changed(); });
        Bind("directionsToggle", delegate { MiniMapPreferences.DirectionsEnabled = !MiniMapPreferences.DirectionsEnabled; Changed(); });
        Bind("positionLeft", delegate { MiniMapPreferences.PositionX -= MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionRight", delegate { MiniMapPreferences.PositionX += MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionUp", delegate { MiniMapPreferences.PositionY += MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionDown", delegate { MiniMapPreferences.PositionY -= MiniMapPreferences.PositionStep; Changed(); });
        Bind("positionReset", delegate { MiniMapPreferences.PositionX = MiniMapPreferences.DefaultPositionX; MiniMapPreferences.PositionY = MiniMapPreferences.DefaultPositionY; Changed(); });
        Bind("positionSave", delegate { MiniMapPreferences.DefaultPositionX = MiniMapPreferences.PositionX; MiniMapPreferences.DefaultPositionY = MiniMapPreferences.PositionY; Changed(); });
        Bind("positionStep1", delegate { MiniMapPreferences.PositionStep = 1; Changed(); });
        Bind("positionStep10", delegate { MiniMapPreferences.PositionStep = 10; Changed(); });
        Bind("positionStep100", delegate { MiniMapPreferences.PositionStep = 100; Changed(); });
        Bind("miniMapUpdate", delegate { if (MiniMapUpdater.Press()) Application.OpenURL("https://github.com/NAS4Killer/MiniMap_Mod"); });
        Bind("miniMapReleaseNotes", delegate { string url = MiniMapUpdater.ReleaseNotesUrl; if (url != null) Application.OpenURL(url); });
        BindPositionHint("positionReset", "Reset: Map Position zurücksetzen.");
        BindPositionHint("positionSave", "Speichern: Aktuelle Map Position als Standard setzen.");
        MoveCaptionDown("positionUp");
        MoveCaptionDown("positionDown");
        foreach (string id in new[] { "frameBlack", "frameWhite", "frameGreen", "frameRed", "frameNone",
                                     "arrowYellow", "arrowWhite", "arrowGreen", "arrowRed", "arrowOriginal",
                                     "positionStep1", "positionStep10", "positionStep100" })
        {
            if (GetChildById(id + "Outline")?.ViewComponent is XUiV_Sprite border)
                border.Sprite.onPostFill = delegate(UIWidget widget, int offset, List<Vector3> verts, List<Vector2> uvs, List<Color> colors)
                    { MiniMapBorderGeometry.Fill(widget, offset, verts, uvs, colors, 3f); };
        }
        RefreshValues();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        XUiC_MiniMapArea.Instance?.BeginSettingsPreview();
        positionHint = "";
        MiniMapUpdater.EnsureChecked();
        RefreshValues();
    }

    public override void OnClose()
    {
        XUiC_MiniMapArea.Instance?.EndSettingsPreview();
        base.OnClose();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (Input.GetKeyDown(KeyCode.Escape) && xui.playerUI.windowManager.IsWindowOpen("miniMapSettings"))
        {
            xui.playerUI.windowManager.Close("miniMapSettings");
            return;
        }
        SetButtonText("miniMapUpdate", MiniMapUpdater.Caption);
        if (GetChildById("miniMapUpdate")?.GetChildById("btnLabel")?.ViewComponent is XUiV_Label updateLabel)
            updateLabel.Color = MiniMapUpdater.IsGreen ? new Color32(70, 230, 70, 255) : Color.white;
        SetLabel("miniMapUpdateStatus", MiniMapUpdater.Status);
        if (GetChildById("miniMapUpdateStatus")?.ViewComponent is XUiV_Label updateStatus)
            updateStatus.Color = MiniMapUpdater.NeedsRestart ? new Color32(255, 60, 60, 255) : new Color32(180, 180, 180, 255);
        XUiController releaseNotes = GetChildById("miniMapReleaseNotes");
        if (releaseNotes?.ViewComponent != null) releaseNotes.ViewComponent.IsVisible = MiniMapUpdater.ReleaseNotesUrl != null;
        XUiController hint = GetChildById("positionHint");
        if (hint?.ViewComponent != null)
        {
            hint.ViewComponent.IsVisible = positionHint.Length > 0;
            hint.ViewComponent.Position = new Vector2i(170, MiniMapUpdater.ReleaseNotesUrl != null ? -718 : -682);
        }
        SetLabel("positionHint", positionHint);
        if (GetChildById("miniMapUpdateStatus")?.ViewComponent != null)
            GetChildById("miniMapUpdateStatus").ViewComponent.IsVisible = positionHint.Length == 0;
        ConfigureFrameFill("frameBlack", Color.black);
        ConfigureFrameFill("frameWhite", Color.white);
        ConfigureFrameFill("frameGreen", new Color32(35, 150, 65, 255));
        ConfigureFrameFill("frameRed", new Color32(190, 35, 35, 255));
        ConfigureFrameFill("frameNone", new Color32(92, 92, 92, 255));
        RefreshArrowColors();
        SetSelectionBorder("frameBlack", MiniMapPreferences.FrameStyle == 1);
        SetSelectionBorder("frameWhite", MiniMapPreferences.FrameStyle == 2);
        SetSelectionBorder("frameGreen", MiniMapPreferences.FrameStyle == 4);
        SetSelectionBorder("frameRed", MiniMapPreferences.FrameStyle == 3);
        SetSelectionBorder("frameNone", MiniMapPreferences.FrameStyle == 0);
    }

    private void Bind(string id, Action action)
    {
        XUiController controller = GetChildById(id);
        if (controller == null)
        {
            Debug.LogWarning("[MiniMap_Mod] Menu button missing: " + id + ". Update the server menu files to match the client version.");
            return;
        }
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
        SetButtonText("f5Mode", MiniMapPreferences.F5ToggleMode ? "Ein/Aus + Doppel-F5" : "Direkt ins Menü");
        SetButtonText("miniMapToggle", MiniMapPreferences.Enabled ? "AN" : "AUS");
        SetLabel("zoomValue", MiniMapPreferences.Zoom.ToString("0", CultureInfo.InvariantCulture) + "x");
        SetLabel("arrowValue", MiniMapPreferences.ArrowSize.ToString(CultureInfo.InvariantCulture));
        RefreshArrowColors();
        SetLabel("sizeValue", MiniMapPreferences.MapSize.ToString(CultureInfo.InvariantCulture));
        SetSelectionBorder("frameBlack", MiniMapPreferences.FrameStyle == 1);
        SetSelectionBorder("frameWhite", MiniMapPreferences.FrameStyle == 2);
        SetSelectionBorder("frameGreen", MiniMapPreferences.FrameStyle == 4);
        SetSelectionBorder("frameRed", MiniMapPreferences.FrameStyle == 3);
        SetSelectionBorder("frameNone", MiniMapPreferences.FrameStyle == 0);
        SetLabel("brightnessValue", Mathf.RoundToInt(MiniMapPreferences.Brightness * 100f) + "%");
        SetButtonText("mapShape", MiniMapPreferences.MapShape == 1 ? "Kreis" : "Quadrat");
        SetButtonText("fogToggle", MiniMapPreferences.FogEnabled ? "AN" : "AUS");
        SetButtonText("transparencyToggle", MiniMapPreferences.MapTransparent ? "AN" : "AUS");
        SetLabel("transparencyValue", MiniMapPreferences.TransparencyPercent + "%");
        SetButtonText("edgeFadeToggle", MiniMapPreferences.EdgeFade ? "AN" : "AUS");
        SetButtonText("northToggle", MiniMapPreferences.NorthUp ? "AN" : "AUS");
        SetButtonText("coordinatesToggle", MiniMapPreferences.CoordinatesEnabled ? "AN" : "AUS");
        SetButtonText("coordinatesPosition", MiniMapPreferences.CoordinatesAbove ? "Über der Map" : "Unter der Map");
        SetButtonText("directionsToggle", MiniMapPreferences.DirectionsEnabled ? "AN" : "AUS");
        SetSelectionBorder("positionStep1", MiniMapPreferences.PositionStep == 1);
        SetSelectionBorder("positionStep10", MiniMapPreferences.PositionStep == 10);
        SetSelectionBorder("positionStep100", MiniMapPreferences.PositionStep == 100);
    }

    private void SetLabel(string id, string value)
    {
        XUiController controller = GetChildById(id);
        if (controller?.ViewComponent is XUiV_Label label) label.Text = value;
    }

    private void RefreshArrowColors()
    {
        ConfigureFrameFill("arrowYellow", Color.yellow);
        ConfigureFrameFill("arrowWhite", Color.white);
        ConfigureFrameFill("arrowGreen", new Color32(90, 255, 110, 255));
        ConfigureFrameFill("arrowRed", new Color32(190, 35, 35, 255));
        ConfigureFrameFill("arrowOriginal", new Color32(92, 92, 92, 255));
        SetSelectionBorder("arrowYellow", MiniMapPreferences.ArrowColor == 1);
        SetSelectionBorder("arrowWhite", MiniMapPreferences.ArrowColor == 2);
        SetSelectionBorder("arrowGreen", MiniMapPreferences.ArrowColor == 4);
        SetSelectionBorder("arrowRed", MiniMapPreferences.ArrowColor == 3);
        SetSelectionBorder("arrowOriginal", MiniMapPreferences.ArrowColor == 0);
    }

    private void SetSelectionBorder(string id, bool selected)
    {
        if (GetChildById(id + "Outline")?.ViewComponent is XUiV_Sprite border)
            border.Color = selected ? new Color32(0, 95, 255, 255) : new Color32(0, 0, 0, 255);
    }

    private void BindPositionHint(string id, string text)
    {
        if (!(GetChildById(id) is XUiC_SimpleButton button)) return;
        button.OnHovered += delegate(XUiController sender, bool over)
        {
            if (over) positionHint = text;
            else if (positionHint == text) positionHint = "";
        };
    }

    private void MoveCaptionDown(string id)
    {
        if (GetChildById(id)?.GetChildById("btnLabel")?.ViewComponent is XUiV_Label label)
            label.Position = new Vector2i(label.Position.x, label.Position.y - 3);
    }

    private void ConfigureFrameFill(string id, Color color)
    {
        if (GetChildById(id) is XUiC_SimpleButton button && button.Button != null)
        {
            button.Button.DefaultSpriteColor = color;
            button.Button.HoverSpriteColor = color;
            button.Button.SelectedSpriteColor = color;
            button.Button.ManualColors = true;
            button.Button.CurrentColor = color;
        }
    }

    private void SetButtonText(string id, string value)
    {
        if (GetChildById(id) is XUiC_SimpleButton button) button.Text = value;
    }
}

internal static class MiniMapArrowColor
{
    internal static Color Get(EntityPlayerLocal player, int style)
    {
        if (style == 1) return Color.yellow;
        if (style == 2) return Color.white;
        if (style == 3) return new Color32(190, 35, 35, 255);
        if (style == 4) return new Color32(90, 255, 110, 255);
        NavObject nav = player?.NavObject;
        if (nav == null) return Color.white;
        return nav.hiddenOnCompass ? Color.grey
            : nav.UseOverrideColor ? nav.OverrideColor : nav.CurrentMapSettings?.Color ?? Color.white;
    }
}

internal static class MiniMapHeading
{
    internal static float Get(EntityPlayerLocal player)
    {
        if (player == null) return 0f;
        // Attached players retain their own yaw; the vehicle maintains its current world yaw.
        return player.AttachedToEntity is EntityVehicle vehicle ? vehicle.rotation.y : player.rotation.y;
    }
}

internal static class MiniMapRenderTint
{
    internal static Color Apply(Color tint, bool isMap, bool transparent, int percent)
    {
        if (isMap && transparent) tint.a *= 1f - Mathf.Clamp(percent, 2, 20) / 100f;
        return tint;
    }
}

internal static class MiniMapBorderGeometry
{
    internal static void Fill(UIWidget widget, int offset, List<Vector3> verts, List<Vector2> uvs, List<Color> colors, float thickness)
    {
        if (verts.Count <= offset || uvs.Count <= offset || colors.Count <= offset) return;
        float left = float.MaxValue, right = float.MinValue, bottom = float.MaxValue, top = float.MinValue;
        Color color = colors[offset];
        Vector2 uv = uvs[offset];
        for (int i = offset; i < verts.Count; i++)
        {
            left = Mathf.Min(left, verts[i].x); right = Mathf.Max(right, verts[i].x);
            bottom = Mathf.Min(bottom, verts[i].y); top = Mathf.Max(top, verts[i].y);
        }
        verts.RemoveRange(offset, verts.Count - offset);
        uvs.RemoveRange(offset, uvs.Count - offset);
        colors.RemoveRange(offset, colors.Count - offset);
        float border = Mathf.Min(thickness, Mathf.Min(right-left, top-bottom)/2f);
        Add(verts, uvs, colors, left, right, top-border, top, uv, color);
        Add(verts, uvs, colors, left, right, bottom, bottom+border, uv, color);
        Add(verts, uvs, colors, left, left+border, bottom+border, top-border, uv, color);
        Add(verts, uvs, colors, right-border, right, bottom+border, top-border, uv, color);
    }

    private static void Add(List<Vector3> verts, List<Vector2> uvs, List<Color> colors,
                            float left, float right, float bottom, float top, Vector2 uv, Color color)
    {
        verts.Add(new Vector3(left, bottom)); verts.Add(new Vector3(left, top));
        verts.Add(new Vector3(right, top)); verts.Add(new Vector3(right, bottom));
        for (int i = 0; i < 4; i++) { uvs.Add(uv); colors.Add(color); }
    }
}
