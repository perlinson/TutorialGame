using System;
using System.Collections.Generic;

[Serializable]
public sealed class SaveNpcState
{
    public string npcId;
    public int affinity;
    public int interactionCount;
    public int lastInteractionDay;
    public string lastChoiceId;
    public string[] flags;

    public SaveNpcState()
    {
    }

    public SaveNpcState(string npcId)
    {
        this.npcId = npcId ?? string.Empty;
        affinity = 0;
        interactionCount = 0;
        lastInteractionDay = 0;
        lastChoiceId = string.Empty;
        flags = Array.Empty<string>();
    }

    public void EnsureDefaults()
    {
        if (npcId == null)
        {
            npcId = string.Empty;
        }

        if (lastChoiceId == null)
        {
            lastChoiceId = string.Empty;
        }

        if (flags == null)
        {
            flags = Array.Empty<string>();
        }
    }

    public bool HasFlag(string flagId)
    {
        if (flags == null || string.IsNullOrWhiteSpace(flagId))
        {
            return false;
        }

        for (var i = 0; i < flags.Length; i++)
        {
            if (flags[i] == flagId)
            {
                return true;
            }
        }

        return false;
    }

    public void AddFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return;
        }

        var merged = new List<string>(flags ?? Array.Empty<string>());
        if (!merged.Contains(flagId))
        {
            merged.Add(flagId);
        }

        flags = merged.ToArray();
    }
}
