using System.Collections.Generic;
using UnityEngine;

// Uses the game's existing navigation objects, icons and visibility rules without modifying them.
internal sealed class MiniMapMarkers
{
    private sealed class Marker
    {
        internal GameObject Root;
        internal UISprite Sprite;
    }

    private readonly Dictionary<int, Marker> markers = new Dictionary<int, Marker>();
    private readonly HashSet<int> alive = new HashSet<int>();
    private readonly List<int> removed = new List<int>();

    internal void Update(XUi xui, Transform parent, GameObject prefab, EntityPlayerLocal player,
        Vector2 center, float heading, float zoom, int size, bool visible)
    {
        if (!visible || player == null || prefab == null || NavObjectManager.Instance == null)
        {
            foreach (Marker marker in markers.Values) marker.Root.SetActive(false);
            return;
        }
        alive.Clear();
        List<NavObject> objects = NavObjectManager.Instance.NavObjectList;
        for (int i = 0; i < objects.Count; i++)
        {
            NavObject nav = objects[i];
            if (nav == null || nav.hiddenOnMap || !nav.IsValid()
                || (nav.TrackedEntity != null && nav.TrackedEntity.entityId == player.entityId)) continue;
            NavObjectMapSettings settings = nav.CurrentMapSettings;
            if (settings == null) continue;
            string icon = nav.GetSpriteName(settings);
            if (string.IsNullOrEmpty(icon)) continue;

            Vector3 world = nav.GetPosition() + Origin.position;
            MiniMapMarkerLayout.Project(world.x - center.x, world.z - center.y, heading,
                size, zoom, out float dx, out float dy);
            Vector3 iconScale = settings.IconScaleVector;
            int width = Mathf.Clamp(Mathf.RoundToInt(24f * iconScale.x), 9, 40);
            int height = Mathf.Clamp(Mathf.RoundToInt(24f * iconScale.y), 9, 40);
            if (settings.AdjustCenter) { dx += width * 0.5f; dy += height * 0.5f; }
            float rotation = settings.UseRotation ? heading - nav.Rotation.y : 0f;
            // Conservative rotated footprint: no icon may spill out of the circle or square.
            float angle = rotation * Mathf.Deg2Rad;
            float halfExtent = 0.5f * Mathf.Max(
                Mathf.Abs(Mathf.Cos(angle)) * width + Mathf.Abs(Mathf.Sin(angle)) * height,
                Mathf.Abs(Mathf.Sin(angle)) * width + Mathf.Abs(Mathf.Cos(angle)) * height);
            if (!MiniMapMarkerLayout.Fits(dx, dy, halfExtent, size, MiniMapPreferences.MapShape == 1)) continue;

            if (!markers.TryGetValue(nav.Key, out Marker marker))
            {
                GameObject root = parent.gameObject.AddChild(prefab);
                root.name = "MiniMapMarker";
                UISprite sprite = root.transform.Find("Sprite")?.GetComponent<UISprite>();
                if (sprite == null) { Object.Destroy(root); continue; }
                Transform name = root.transform.Find("Name");
                if (name != null) name.gameObject.SetActive(false);
                TweenAlpha blinking = sprite.GetComponent<TweenAlpha>();
                if (blinking != null) blinking.enabled = false;
                marker = new Marker { Root = root, Sprite = sprite };
                markers.Add(nav.Key, marker);
            }
            UISprite widget = marker.Sprite;
            if (widget.spriteName != icon)
            {
                widget.atlas = xui.GetAtlasByName(((Object)widget.atlas).name, icon);
                widget.spriteName = icon;
            }
            widget.depth = 20 + settings.Layer;
            widget.width = width;
            widget.height = height;
            widget.color = nav.hiddenOnCompass ? Color.grey
                : nav.UseOverrideColor ? nav.OverrideColor : settings.Color;
            widget.transform.localEulerAngles = new Vector3(0f, 0f, rotation);
            marker.Root.transform.localPosition = new Vector3(size * 0.5f + dx, -size * 0.5f + dy, 0f);
            marker.Root.SetActive(true);
            alive.Add(nav.Key);
        }
        removed.Clear();
        foreach (KeyValuePair<int, Marker> pair in markers)
            if (!alive.Contains(pair.Key)) removed.Add(pair.Key);
        foreach (int key in removed)
        {
            Object.Destroy(markers[key].Root);
            markers.Remove(key);
        }
    }

    internal void Clear()
    {
        foreach (Marker marker in markers.Values) Object.Destroy(marker.Root);
        markers.Clear();
        alive.Clear();
        removed.Clear();
    }
}
