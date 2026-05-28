using System;

[Serializable]
public sealed class SavePlayerAttributes
{
    public InnateAttributes innate;
    public int[] branchLevels;
    public int[] branchExperiences;
    public string[] learnedDivinePowerIds;
    public string[] learnedArtifactIds;
}
