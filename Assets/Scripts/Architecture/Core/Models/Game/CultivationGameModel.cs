using QFramework;

public sealed class CultivationGameModel : AbstractModel
{
    public readonly BindableProperty<int> WorldDay = new BindableProperty<int>(1);
    public readonly BindableProperty<int> WorldTimeIndex = new BindableProperty<int>(4);
    public readonly BindableProperty<string> WorldTimeText = new BindableProperty<string>("太初历 第1日 · 辰时");
    public readonly BindableProperty<GameHubContext> CurrentHubContext = new BindableProperty<GameHubContext>(GameHubContext.WorldMap);
    public readonly BindableProperty<bool> IsHubVisible = new BindableProperty<bool>(false);
    public readonly BindableProperty<bool> IsPlayerCompendiumVisible = new BindableProperty<bool>(false);
    public readonly BindableProperty<PlayerCompendiumMainTab> PlayerCompendiumMainTab = new BindableProperty<PlayerCompendiumMainTab>(global::PlayerCompendiumMainTab.Character);
    public readonly BindableProperty<string> PlayerCompendiumSectionId = new BindableProperty<string>(string.Empty);

    protected override void OnInit()
    {
    }

    public void Apply(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            ClearTimeOnly();
            return;
        }

        CultivationGameTime.EnsureDefaults(saveData);
        WorldDay.Value = saveData.worldDay;
        WorldTimeIndex.Value = saveData.worldTimeIndex;
        WorldTimeText.Value = CultivationGameTime.Format(saveData);
    }

    public void SetHubState(bool visible, GameHubContext context)
    {
        IsHubVisible.Value = visible;
        CurrentHubContext.Value = context;
        this.SendEvent(new GameHubStateChangedEvent(visible, context));
    }

    public void SetPlayerCompendiumVisible(bool visible)
    {
        IsPlayerCompendiumVisible.Value = visible;
        this.SendEvent(new PlayerCompendiumStateChangedEvent(visible, PlayerCompendiumMainTab.Value, PlayerCompendiumSectionId.Value));
    }

    public void SetPlayerCompendiumSelection(global::PlayerCompendiumMainTab mainTab, string sectionId)
    {
        PlayerCompendiumMainTab.Value = mainTab;
        PlayerCompendiumSectionId.Value = sectionId ?? string.Empty;
        this.SendEvent(new PlayerCompendiumStateChangedEvent(IsPlayerCompendiumVisible.Value, mainTab, PlayerCompendiumSectionId.Value));
    }

    public void Clear()
    {
        ClearTimeOnly();
        SetHubState(false, GameHubContext.WorldMap);
        SetPlayerCompendiumSelection(global::PlayerCompendiumMainTab.Character, string.Empty);
        SetPlayerCompendiumVisible(false);
    }

    private void ClearTimeOnly()
    {
        WorldDay.Value = 1;
        WorldTimeIndex.Value = 4;
        WorldTimeText.Value = "太初历 第1日 · 辰时";
    }
}
