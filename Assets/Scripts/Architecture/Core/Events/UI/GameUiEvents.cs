public sealed class GameHubStateChangedEvent
{
    public GameHubStateChangedEvent(bool visible, GameHubContext context)
    {
        Visible = visible;
        Context = context;
    }

    public bool Visible { get; }
    public GameHubContext Context { get; }
}

public sealed class PlayerCompendiumStateChangedEvent
{
    public PlayerCompendiumStateChangedEvent(bool visible, PlayerCompendiumMainTab mainTab, string sectionId)
    {
        Visible = visible;
        MainTab = mainTab;
        SectionId = sectionId ?? string.Empty;
    }

    public bool Visible { get; }
    public PlayerCompendiumMainTab MainTab { get; }
    public string SectionId { get; }
}
