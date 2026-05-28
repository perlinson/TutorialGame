using System;

[Serializable]
public sealed class PlayerAttributes
{
    public InnateAttributes innate;
    public CombatAttributes combat;
    public CultivationAttributes cultivation;
    public StatusAttributes status;
    public SocialAttributes social;
    public AttributeBranch[] branches;
}
