using QFramework;

public sealed partial class CultivationBattleSystem : AbstractSystem
{
    private CultivationSaveSystem saveSystem;
    private CultivationFactionSystem factionSystem;
    private CultivationTaskSystem taskSystem;
    private CultivationRewardSystem rewardSystem;
    private CultivationMindStateSystem mindStateSystem;
    private CultivationEnemyAiSystem enemyAiSystem;

    protected override void OnInit()
    {
        saveSystem = this.GetSystem<CultivationSaveSystem>();
        factionSystem = this.GetSystem<CultivationFactionSystem>();
        taskSystem = this.GetSystem<CultivationTaskSystem>();
        rewardSystem = this.GetSystem<CultivationRewardSystem>();
        mindStateSystem = this.GetSystem<CultivationMindStateSystem>();
        enemyAiSystem = this.GetSystem<CultivationEnemyAiSystem>();
    }
}
