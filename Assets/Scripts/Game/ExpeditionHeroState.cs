using System.Collections.Generic;

using UnityEngine;

public sealed class ExpeditionHeroState
{
    public string HeroName;
    public string ArchetypeId;
    public string ArchetypeName;
    public int MaxHealth;
    public int CurrentHealth;
    public int Stress;
    public int AttackBonus;
    public int DefenseBonus;
    public int StressResistBonus;
    public int TalismanCharges;
    public int MedicineCharges;
    public Sprite PortraitImage;
    public int GuardValue;
    public int CounterDamage;
    public ExpeditionEquipmentLoadout Loadout;
    public readonly List<ExpeditionSkillDefinition> Skills = new List<ExpeditionSkillDefinition>();
}
