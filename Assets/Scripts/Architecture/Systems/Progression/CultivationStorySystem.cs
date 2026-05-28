using System.Collections.Generic;
using QFramework;

public sealed class CultivationStorySystem : AbstractSystem
{
    protected override void OnInit()
    {
    }

    public StorySignalResult RecordSignal(CultivationSaveData saveData, StorySignal signal)
    {
        var result = new StorySignalResult();
        if (saveData == null || signal == null || string.IsNullOrWhiteSpace(signal.StoryId))
        {
            return result;
        }

        saveData.EnsureDefaults();
        var flagId = BuildFlagId(signal);
        var alreadySeen = Contains(saveData.storyFlags, flagId);
        saveData.storyFlags = AddUnique(saveData.storyFlags, flagId);
        saveData.storyLog = AddUnique(saveData.storyLog, BuildLogLine(signal));

        result.Recorded = !alreadySeen;
        result.StoryFlag = flagId;
        result.Message = BuildStoryMessage(signal, alreadySeen);
        return result;
    }

    public string BuildStorySummary(CultivationSaveData saveData)
    {
        if (saveData == null || saveData.storyLog == null || saveData.storyLog.Length == 0)
        {
            return "尚无可追溯的经历。";
        }

        var builder = new System.Text.StringBuilder();
        var start = System.Math.Max(0, saveData.storyLog.Length - 5);
        for (var i = start; i < saveData.storyLog.Length; i++)
        {
            if (builder.Length > 0)
            {
                builder.Append('\n');
            }

            builder.Append(saveData.storyLog[i]);
        }

        return builder.ToString();
    }

    private static string BuildFlagId(StorySignal signal)
    {
        return signal.StoryId + ":" + signal.NodeId;
    }

    private static string BuildLogLine(StorySignal signal)
    {
        if (!string.IsNullOrWhiteSpace(signal.Title))
        {
            return signal.Title + " / " + signal.NodeId;
        }

        return signal.StoryId + " / " + signal.NodeId;
    }

    private static string BuildStoryMessage(StorySignal signal, bool alreadySeen)
    {
        if (alreadySeen)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(signal.ResultText))
        {
            return signal.ResultText;
        }

        if (!string.IsNullOrWhiteSpace(signal.Title))
        {
            return "经历已记录：" + signal.Title + "。";
        }

        return "经历已记录。";
    }

    private static bool Contains(string[] values, string target)
    {
        if (values == null || string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    private static string[] AddUnique(string[] values, string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return values ?? System.Array.Empty<string>();
        }

        var merged = new List<string>(values ?? System.Array.Empty<string>());
        if (!merged.Contains(target))
        {
            merged.Add(target);
        }

        return merged.ToArray();
    }
}
