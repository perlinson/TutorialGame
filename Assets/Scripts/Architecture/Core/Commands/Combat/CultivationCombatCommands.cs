using QFramework;

public sealed class ResolveDirectAttackTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;
    private readonly ExpeditionEnemyState target;
    private readonly int damage;
    private readonly string missSummary;

    public ResolveDirectAttackTurnCommand(CombatTurnContext context, ExpeditionEnemyState target, int damage, string missSummary)
    {
        this.context = context;
        this.target = target;
        this.damage = damage;
        this.missSummary = missSummary;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveDirectAttackTurn(context, target, damage, missSummary);
    }
}

public sealed class ResolveSkillTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;
    private readonly int skillIndex;

    public ResolveSkillTurnCommand(CombatTurnContext context, int skillIndex)
    {
        this.context = context;
        this.skillIndex = skillIndex;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveSkillTurn(context, skillIndex);
    }
}

public sealed class ResolveTalismanTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;

    public ResolveTalismanTurnCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveTalismanTurn(context);
    }
}

public sealed class ResolveMedicineTurnCommand : AbstractCommand<CombatTurnResult>
{
    private readonly CombatTurnContext context;

    public ResolveMedicineTurnCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override CombatTurnResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveMedicineTurn(context);
    }
}

public sealed class ResolveRoomEventCommand : AbstractCommand<ExpeditionRoomActionResult>
{
    private readonly CombatTurnContext context;

    public ResolveRoomEventCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionRoomActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().ResolveRoomEvent(context);
    }
}

public sealed class UseTorchSupplyCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public UseTorchSupplyCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().UseTorchSupply(context);
    }
}

public sealed class CampAndRecoverCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public CampAndRecoverCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().CampAndRecover(context);
    }
}

public sealed class RecenterMindCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public RecenterMindCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().RecenterMind(context);
    }
}

public sealed class SkipRoomCommand : AbstractCommand<ExpeditionSupportActionResult>
{
    private readonly CombatTurnContext context;

    public SkipRoomCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override ExpeditionSupportActionResult OnExecute()
    {
        return this.GetSystem<CultivationBattleSystem>().SkipRoom(context);
    }
}

public sealed class PreviewEnemyIntentsCommand : AbstractCommand<EnemyIntentPreview[]>
{
    private readonly CombatTurnContext context;

    public PreviewEnemyIntentsCommand(CombatTurnContext context)
    {
        this.context = context;
    }

    protected override EnemyIntentPreview[] OnExecute()
    {
        return this.GetSystem<CultivationEnemyAiSystem>().PreviewIntents(context);
    }
}
