using NUnit.Framework;

public sealed class TaskLibraryEditModeTests
{
    [Test]
    public void BuildActiveTaskSummary_ReturnsFallbackTextWhenNoActiveTask()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修"
        };
        saveData.EnsureDefaults();

        var summary = TaskLibrary.BuildActiveTaskSummary(saveData);

        Assert.That(summary, Does.Contain("暂无新任务"));
    }

    [Test]
    public void TryGetActiveTask_CreatesStateForExistingActiveTask()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            activeTaskId = "task_bandit_route"
        };
        saveData.EnsureDefaults();

        var found = TaskLibrary.TryGetActiveTask(saveData, out var definition, out var state);

        Assert.That(found, Is.True);
        Assert.That(definition, Is.Not.Null);
        Assert.That(definition.Id, Is.EqualTo("task_bandit_route"));
        Assert.That(state, Is.Not.Null);
        Assert.That(state.taskId, Is.EqualTo("task_bandit_route"));
    }

    [Test]
    public void GetProgressValue_UsesInventoryCountForCollectItemObjectives()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            activeTaskId = "task_mist_herbs",
            storageItems = new[]
            {
                new SaveItemStack("mist_mushroom", 2)
            }
        };
        saveData.EnsureDefaults();
        Assert.That(TaskLibrary.TryGetActiveTask(saveData, out var definition, out var state), Is.True);

        var progress = TaskLibrary.GetProgressValue(saveData, definition, state);

        Assert.That(progress, Is.EqualTo(2));
    }

    [Test]
    public void BuildActiveTaskSummary_IncludesCompletionMarkerWhenProgressIsEnough()
    {
        var saveData = new MainMenuSaveData
        {
            heroName = "测试修士",
            archetypeId = "sword",
            archetypeName = "流云剑修",
            activeTaskId = "task_mist_herbs",
            storageItems = new[]
            {
                new SaveItemStack("mist_mushroom", 2)
            }
        };
        saveData.EnsureDefaults();

        var summary = TaskLibrary.BuildActiveTaskSummary(saveData);

        Assert.That(summary, Does.Contain("已可结算"));
        Assert.That(summary, Does.Contain("采回雾隐芝"));
    }
}
