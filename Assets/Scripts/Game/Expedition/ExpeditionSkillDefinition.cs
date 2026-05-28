using UnityEngine;

public sealed class ExpeditionSkillDefinition
{
    public ExpeditionSkillDefinition(string id, string name, string description, Sprite iconImage = null)
    {
        Id = id;
        Name = name;
        Description = description;
        IconImage = iconImage;
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public Sprite IconImage { get; }
}
