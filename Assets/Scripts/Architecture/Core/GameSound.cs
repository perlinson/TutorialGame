using QFramework;

public enum CultivationButtonSound
{
    None,
    Click,
    Confirm,
    Cancel
}

public static class GameSound
{
    internal static ISoundSystem Instance
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetSystem<ISoundSystem>();
        }
    }

    public static void Play(SoundType type)
    {
        Instance?.PlaySound(type);
    }

    public static void Play(string filename)
    {
        Instance?.PlaySound(filename);
    }

    public static void PlayMainMenuMusic()
    {
        Instance?.PlayMainMenuMusic();
    }

    public static void PlayWorldMapMusic()
    {
        Instance?.PlayWorldMapMusic();
    }

    public static void PlayExpeditionMusic(WorldRegionDefinition region)
    {
        Instance?.PlayExpeditionMusic(region);
    }

    public static void StopAll()
    {
        Instance?.StopAllSound();
    }
}
