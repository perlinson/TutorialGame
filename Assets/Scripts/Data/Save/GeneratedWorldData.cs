using System;

[Serializable]
public enum GeneratedNpcGender
{
    Unknown,
    Male,
    Female
}

[Serializable]
public enum GeneratedNpcAgeBand
{
    Youth,
    Adult,
    Senior
}

[Serializable]
public enum GeneratedRelationshipType
{
    Neutral,
    FellowDisciple,
    MentorStudent,
    Friend,
    Rival,
    Enemy,
    Benefactor,
    Debtor
}

[Serializable]
public enum WorldIncidentStatus
{
    Active,
    Resolved,
    Expired
}

[Serializable]
public sealed class GeneratedNpcData
{
    public string npcId;
    public string displayName;
    public string title;
    public GeneratedNpcGender gender;
    public GeneratedNpcAgeBand ageBand;
    public int realmTier;
    public int spiritRootGrade;
    public string[] personalityTags;
    public string[] fortuneTags;
    public string homeRegionId;
    public string currentLocationId;
    public string factionId;
    public string factionName;
    public string socialStyle;
    public string growthStyle;
    public bool isAlive;
    public NpcRoleType roleType;
    public NpcSceneType sceneType;
    public string conversationTemplateTitle;

    public void EnsureDefaults()
    {
        npcId ??= string.Empty;
        displayName ??= string.Empty;
        title ??= string.Empty;
        homeRegionId ??= string.Empty;
        currentLocationId ??= string.Empty;
        factionId ??= string.Empty;
        factionName ??= string.Empty;
        socialStyle ??= string.Empty;
        growthStyle ??= string.Empty;
        conversationTemplateTitle ??= string.Empty;
        personalityTags ??= Array.Empty<string>();
        fortuneTags ??= Array.Empty<string>();
    }
}

[Serializable]
public sealed class NpcRelationEdgeData
{
    public string sourceNpcId;
    public string targetNpcId;
    public GeneratedRelationshipType relationshipType;
    public int affinity;
    public int hostility;
    public string recentIncidentId;

    public void EnsureDefaults()
    {
        sourceNpcId ??= string.Empty;
        targetNpcId ??= string.Empty;
        recentIncidentId ??= string.Empty;
    }
}

[Serializable]
public sealed class GeneratedLocationState
{
    public string locationId;
    public string parentLocationId;
    public string displayName;
    public string subtitle;
    public string description;
    public bool isUnlocked;
    public bool isTemporary;
    public string sourceTaskId;
    public string sourceStoryFlagId;
    public string[] residentNpcIds;
    public NpcSceneType sceneType;

    public void EnsureDefaults()
    {
        locationId ??= string.Empty;
        parentLocationId ??= string.Empty;
        displayName ??= string.Empty;
        subtitle ??= string.Empty;
        description ??= string.Empty;
        sourceTaskId ??= string.Empty;
        sourceStoryFlagId ??= string.Empty;
        residentNpcIds ??= Array.Empty<string>();
    }
}

[Serializable]
public sealed class WorldIncidentData
{
    public string incidentId;
    public string templateId;
    public string displayTitle;
    public string description;
    public string locationId;
    public string conversationTitle;
    public string sourceTaskId;
    public string[] participantNpcIds;
    public int startDay;
    public int expireDay;
    public WorldIncidentStatus status;

    public void EnsureDefaults()
    {
        incidentId ??= string.Empty;
        templateId ??= string.Empty;
        displayTitle ??= string.Empty;
        description ??= string.Empty;
        locationId ??= string.Empty;
        conversationTitle ??= string.Empty;
        sourceTaskId ??= string.Empty;
        participantNpcIds ??= Array.Empty<string>();
    }
}
