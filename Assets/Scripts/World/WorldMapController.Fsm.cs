public sealed partial class WorldMapController
{
    private const string ModalMusicDuckReason = "WorldMapModal";

    protected override void OnBeforeDestroy()
    {
        base.OnBeforeDestroy();
        SetMusicDuck(ModalMusicDuckReason, false);
        CloseFloatingPanels();
    }
}
