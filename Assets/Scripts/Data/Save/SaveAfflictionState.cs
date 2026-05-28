using System;

[Serializable]
public sealed class SaveAfflictionState
{
    public string afflictionId;
    public int stacks;

    public SaveAfflictionState()
    {
    }

    public SaveAfflictionState(string afflictionId, int stacks)
    {
        this.afflictionId = afflictionId;
        this.stacks = stacks;
    }
}
