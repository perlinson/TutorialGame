public readonly struct MainMenuConfig
{
    public MainMenuConfig(string gameplaySceneName, string title, string subtitle, string description)
    {
        GameplaySceneName = gameplaySceneName;
        Title = title;
        Subtitle = subtitle;
        Description = description;
    }

    public string GameplaySceneName { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Description { get; }
}
