using System;
using System.Collections.Generic;

[Serializable]
public sealed class SaveTaskState
{
    public string taskId;
    public int progress;
    public bool completed;
    public bool rewardClaimed;
    public string[] progressFlags;
    public string[] triggeredEventIds;
    public string[] chosenOptionIds;

    public SaveTaskState()
    {
    }

    public SaveTaskState(string newTaskId)
    {
        taskId = newTaskId;
        progress = 0;
        completed = false;
        rewardClaimed = false;
        progressFlags = Array.Empty<string>();
        triggeredEventIds = Array.Empty<string>();
        chosenOptionIds = Array.Empty<string>();
    }

    public void EnsureDefaults()
    {
        if (progressFlags == null)
        {
            progressFlags = Array.Empty<string>();
        }

        if (triggeredEventIds == null)
        {
            triggeredEventIds = Array.Empty<string>();
        }

        if (chosenOptionIds == null)
        {
            chosenOptionIds = Array.Empty<string>();
        }
    }

    public bool HasFlag(string flagId)
    {
        return Contains(progressFlags, flagId);
    }

    public void AddFlag(string flagId)
    {
        progressFlags = AddUnique(progressFlags, flagId);
    }

    public bool HasTriggeredEvent(string eventId)
    {
        return Contains(triggeredEventIds, eventId);
    }

    public void MarkTriggeredEvent(string eventId)
    {
        triggeredEventIds = AddUnique(triggeredEventIds, eventId);
    }

    public bool HasChosenOption(string optionId)
    {
        return Contains(chosenOptionIds, optionId);
    }

    public void MarkChosenOption(string optionId)
    {
        chosenOptionIds = AddUnique(chosenOptionIds, optionId);
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
            return values ?? Array.Empty<string>();
        }

        var merged = new List<string>(values ?? Array.Empty<string>());
        if (!merged.Contains(target))
        {
            merged.Add(target);
        }

        return merged.ToArray();
    }
}
