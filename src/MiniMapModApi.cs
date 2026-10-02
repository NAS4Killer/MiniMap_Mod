using System;
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
    public static bool Enabled = true;
    public static float Zoom = 1f;
    public static int ArrowSize = 40;
    public static int MapSize = 256;
    public static int FrameStyle;
    public static float Brightness = 1f;
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
                "Brightness=" + Brightness.ToString("0.0", CultureInfo.InvariantCulture)
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
        MapSize = Mathf.Clamp(MapSize, 160, 480);
        FrameStyle = Mathf.Clamp(FrameStyle, 0, 5);
        Brightness = Mathf.Clamp(Brightness, 0.1f, 1f);
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
        base.Update(deltaTime);
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
            if (crosshair != null) crosshair.UiTransform.localEulerAngles = new Vector3(0f, 0f, player.rotation.y);
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

        ViewComponent.Size = new Vector2i(size, size);
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
        brightnessOverlay.ViewComponent.Size = new Vector2i(innerSize, innerSize);
        brightnessOverlay.ViewComponent.Position = new Vector2i(borderWidth, -borderWidth);
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

    public override void updateMapObjects() { }

    public override void Cleanup()
    {
        if (playerCamera != null)
        {
            playerCamera.PreRender -= OnPreRender;
            playerCamera = null;
        }
        if (Instance == this) Instance = null;
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
        Bind("miniMapClose", delegate { xui.playerUI.windowManager.Close("miniMapSettings"); });
        RefreshValues();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        RefreshValues();
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
        RefreshValues();
    }

    private void RefreshValues()
    {
        SetButtonText("miniMapToggle", MiniMapPreferences.Enabled ? "AN" : "AUS");
        SetLabel("zoomValue", MiniMapPreferences.Zoom.ToString("0", CultureInfo.InvariantCulture) + "x");
        SetLabel("arrowValue", MiniMapPreferences.ArrowSize.ToString(CultureInfo.InvariantCulture));
        SetLabel("sizeValue", MiniMapPreferences.MapSize.ToString(CultureInfo.InvariantCulture));
        SetButtonText("frameCycle", FrameNames[MiniMapPreferences.FrameStyle]);
        SetLabel("brightnessValue", Mathf.RoundToInt(MiniMapPreferences.Brightness * 100f) + "%");
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
