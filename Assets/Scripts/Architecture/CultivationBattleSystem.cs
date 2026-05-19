using QFramework;

public sealed partial class CultivationBattleSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
    }
}
