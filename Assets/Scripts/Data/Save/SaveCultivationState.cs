using System;

[Serializable]
public sealed class SaveCultivationState
{
    public CultivationLevel level;
    public int cultivationProgress;
    public int usedLifespan;
    public int heartDemonValue;
    public int pillPoison;
    public string[] activeArtifactIds;
}
