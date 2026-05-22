using QFramework;

public sealed class CultivationTaskBoardModel : AbstractModel
{
    public readonly BindableProperty<string> ActiveTaskId = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> ActiveTaskSummary = new BindableProperty<string>(string.Empty);
    public readonly BindableProperty<string> LastBoardMessage = new BindableProperty<string>(string.Empty);

    protected override void OnInit()
    {
    }

    public void Apply(MainMenuSaveData saveData, string boardMessage = null)
    {
        if (saveData == null)
        {
            ActiveTaskId.Value = string.Empty;
            ActiveTaskSummary.Value = "委托：暂无新任务。";
            LastBoardMessage.Value = boardMessage ?? string.Empty;
            return;
        }

        ActiveTaskId.Value = saveData.activeTaskId ?? string.Empty;
        ActiveTaskSummary.Value = TaskLibrary.BuildActiveTaskSummary(saveData);
        if (boardMessage != null)
        {
            LastBoardMessage.Value = boardMessage;
        }
    }
}
