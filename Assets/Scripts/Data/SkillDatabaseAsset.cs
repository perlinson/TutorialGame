using UnityEngine;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Cultivation/Data/Skill Database")]
public sealed class SkillDatabaseAsset : ScriptableObject
{
    public SkillDefinition[] skills;
}
