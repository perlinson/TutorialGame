using UnityEngine;

public sealed class ExpeditionEnemyState
{
    public ExpeditionEnemyState(ExpeditionEnemyFaction faction, string name, string techniqueName, Sprite portraitImage, int maxHealth, int damage, int stressDamage, bool isElite, int armor, int poisonResistance, int stunResistance, int position)
    {
        Faction = faction;
        Name = name;
        TechniqueName = techniqueName;
        PortraitImage = portraitImage;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Damage = damage;
        StressDamage = stressDamage;
        IsElite = isElite;
        Armor = armor;
        PoisonResistance = poisonResistance;
        StunResistance = stunResistance;
        Position = position;
    }

    public ExpeditionEnemyFaction Faction { get; }
    public string Name { get; }
    public string TechniqueName { get; }
    public Sprite PortraitImage { get; }
    public int MaxHealth { get; }
    public int Damage { get; }
    public int StressDamage { get; }
    public bool IsElite { get; }
    public int Position { get; set; }
    public int Armor { get; set; }
    public int PoisonResistance { get; }
    public int StunResistance { get; }
    public int PoisonStacks { get; set; }
    public int ExposedTurns { get; set; }
    public int StunnedTurns { get; set; }
    public int CurrentHealth { get; set; }
    public bool IsAlive => CurrentHealth > 0;

    public string FactionLabel
    {
        get
        {
            switch (Faction)
            {
                case ExpeditionEnemyFaction.Bandit:
                    return "匪寇";
                case ExpeditionEnemyFaction.Cultivator:
                    return "邪修";
                case ExpeditionEnemyFaction.Beast:
                    return "妖兽";
                case ExpeditionEnemyFaction.HeartDemon:
                    return "心魔";
                default:
                    return "尸傀";
            }
        }
    }

    public int GetEffectiveArmor()
    {
        return ExposedTurns > 0 ? 0 : Armor;
    }
}
