using QFramework;

public static class GameLog
{
    private static IGameLogService Service
    {
        get
        {
            CultivationApp.EnsureInitialized();
            return CultivationApp.Interface.GetUtility<IGameLogService>();
        }
    }

    public static void Info(string message)
    {
        Service.Info(message);
    }

    public static void Warning(string message)
    {
        Service.Warning(message);
    }

    public static void Error(string message)
    {
        Service.Error(message);
    }
}
