namespace MiniMapMod
{
    public sealed class MiniMapModApi : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            UnityEngine.Debug.Log("[MiniMap_Mod] ModAPI initialized");
        }
    }

}

public class XUiC_MiniMapArea : XUiC_MapArea
{
    private LocalPlayerCamera playerCamera;
    private bool centeredOnce;

    public override void Init()
    {
        UnityEngine.Debug.Log("[MiniMap_Mod] XUiC_MiniMapArea.Init");
        base.Init();
        AlwaysUpdate = true;
    }

    public override void OnOpen()
    {
        // Do not call XUiC_MapArea.OnOpen(): it opens the full-screen map UI.
        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].OnOpen();
        }
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
        if (playerCamera != null)
        {
            playerCamera.PreRender += OnPreRender;
        }

        if (crosshair != null)
        {
            crosshair.IsVisible = true;
        }
    }

    public override void OnClose()
    {
        if (playerCamera != null)
        {
            playerCamera.PreRender -= OnPreRender;
            playerCamera = null;
        }

        for (int i = 0; i < Children.Count; i++)
        {
            Children[i].OnClose();
        }
        ViewComponent?.OnClose();

        isOpen = false;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (xuiTexture != null)
        {
            xuiTexture.Size = new Vector2i(256, 256);
        }

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
                mapMiddlePosPixel = new UnityEngine.Vector2(player.position.x, player.position.z);
                positionMap();
            }

            if (crosshair != null)
            {
                crosshair.UiTransform.localEulerAngles = new UnityEngine.Vector3(0f, 0f, player.rotation.y);
            }
        }
    }

    public override void updateMapObjects()
    {
        // The first version only displays the local player's centered map.
    }

    public override void Cleanup()
    {
        if (playerCamera != null)
        {
            playerCamera.PreRender -= OnPreRender;
            playerCamera = null;
        }

        base.Cleanup();
    }
}
