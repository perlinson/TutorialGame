using System.Collections.Generic;
using UnityEngine;

public interface IFactionStrategy
{
    ExpeditionEnemyFaction Faction { get; }
    Color FactionColor { get; }
    void ApplyFallbackEnemyData(int index, bool isElite, ExpeditionRoomKind roomKind,
        ref int maxHealth, ref int damage, ref int stressDamage,
        ref int armor, ref int poisonResistance, ref int stunResistance,
        out string name, out string techniqueName);
    void ApplyFallbackLoot(List<SaveItemStack> loot, bool isElite);
}

public sealed class BanditFactionStrategy : IFactionStrategy
{
    public ExpeditionEnemyFaction Faction => ExpeditionEnemyFaction.Bandit;
    public Color FactionColor => new Color(0.46f, 0.34f, 0.24f, 1f);

    public void ApplyFallbackEnemyData(int index, bool isElite, ExpeditionRoomKind roomKind,
        ref int maxHealth, ref int damage, ref int stressDamage,
        ref int armor, ref int poisonResistance, ref int stunResistance,
        out string name, out string techniqueName)
    {
        name = isElite ? "悍匪头目" : index % 2 == 0 ? "山贼刀手" : "黑风弩匪";
        techniqueName = "掷灰夺灯";
        maxHealth -= 1;
        stressDamage = System.Math.Max(2, stressDamage - 2);
        armor = 0;
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, bool isElite)
    {
        AddLoot(loot, "green_spirit_sand", 1);
        AddLoot(loot, "bandit_route_token", isElite ? 1 : 0);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class CultivatorFactionStrategy : IFactionStrategy
{
    public ExpeditionEnemyFaction Faction => ExpeditionEnemyFaction.Cultivator;
    public Color FactionColor => new Color(0.54f, 0.16f, 0.2f, 1f);

    public void ApplyFallbackEnemyData(int index, bool isElite, ExpeditionRoomKind roomKind,
        ref int maxHealth, ref int damage, ref int stressDamage,
        ref int armor, ref int poisonResistance, ref int stunResistance,
        out string name, out string techniqueName)
    {
        name = isElite ? "魔焰祭使" : index % 2 == 0 ? "夺灵邪修" : "血符散修";
        techniqueName = "邪诀侵神";
        poisonResistance = 1;
        armor += 1;
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, bool isElite)
    {
        AddLoot(loot, "blood_talisman_page", 1);
        AddLoot(loot, "evil_cult_notes", isElite ? 1 : 0);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class BeastFactionStrategy : IFactionStrategy
{
    public ExpeditionEnemyFaction Faction => ExpeditionEnemyFaction.Beast;
    public Color FactionColor => new Color(0.22f, 0.42f, 0.2f, 1f);

    public void ApplyFallbackEnemyData(int index, bool isElite, ExpeditionRoomKind roomKind,
        ref int maxHealth, ref int damage, ref int stressDamage,
        ref int armor, ref int poisonResistance, ref int stunResistance,
        out string name, out string techniqueName)
    {
        name = isElite ? "山魈头领" : index % 2 == 0 ? "裂爪妖狼" : "沼鳞蜥妖";
        techniqueName = "扑杀撕咬";
        damage += 1;
        stressDamage = System.Math.Max(2, stressDamage - 1);
        poisonResistance = 1;
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, bool isElite)
    {
        AddLoot(loot, "beast_core_shard", 1);
        AddLoot(loot, "beast_bone", isElite ? 2 : 1);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class HeartDemonFactionStrategy : IFactionStrategy
{
    public ExpeditionEnemyFaction Faction => ExpeditionEnemyFaction.HeartDemon;
    public Color FactionColor => new Color(0.42f, 0.2f, 0.46f, 1f);

    public void ApplyFallbackEnemyData(int index, bool isElite, ExpeditionRoomKind roomKind,
        ref int maxHealth, ref int damage, ref int stressDamage,
        ref int armor, ref int poisonResistance, ref int stunResistance,
        out string name, out string techniqueName)
    {
        name = roomKind == ExpeditionRoomKind.Boss ? "魇念魔主" : index % 2 == 0 ? "心魔残影" : "执念幻身";
        techniqueName = "幻念侵心";
        stressDamage += 2;
        armor = 0;
        poisonResistance = 2;
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, bool isElite)
    {
        AddLoot(loot, "heart_mark_fragment", isElite ? 2 : 1);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public sealed class CorpsePuppetFactionStrategy : IFactionStrategy
{
    public ExpeditionEnemyFaction Faction => ExpeditionEnemyFaction.CorpsePuppet;
    public Color FactionColor => new Color(0.36f, 0.4f, 0.42f, 1f);

    public void ApplyFallbackEnemyData(int index, bool isElite, ExpeditionRoomKind roomKind,
        ref int maxHealth, ref int damage, ref int stressDamage,
        ref int armor, ref int poisonResistance, ref int stunResistance,
        out string name, out string techniqueName)
    {
        name = isElite ? "尸煞督统" : index % 2 == 0 ? "腐甲尸傀" : "阴骨行尸";
        techniqueName = "尸毒重击";
        maxHealth += 2;
        armor += 1;
        poisonResistance = 3;
        stunResistance = 1;
    }

    public void ApplyFallbackLoot(List<SaveItemStack> loot, bool isElite)
    {
        AddLoot(loot, "yin_bone", isElite ? 2 : 1);
        AddLoot(loot, "corpse_core", isElite ? 1 : 0);
    }

    private static void AddLoot(List<SaveItemStack> loot, string itemId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0) return;
        for (var i = 0; i < loot.Count; i++)
        {
            if (loot[i].itemId == itemId) { loot[i].quantity += quantity; return; }
        }
        loot.Add(new SaveItemStack(itemId, quantity));
    }
}

public static class FactionStrategyRegistry
{
    private static readonly Dictionary<ExpeditionEnemyFaction, IFactionStrategy> strategies;
    private static bool initialized;

    static FactionStrategyRegistry()
    {
        strategies = new Dictionary<ExpeditionEnemyFaction, IFactionStrategy>();
    }

    public static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        Register(new BanditFactionStrategy());
        Register(new CultivatorFactionStrategy());
        Register(new BeastFactionStrategy());
        Register(new HeartDemonFactionStrategy());
        Register(new CorpsePuppetFactionStrategy());
    }

    public static void Register(IFactionStrategy strategy)
    {
        if (strategy == null) return;
        strategies[strategy.Faction] = strategy;
    }

    public static IFactionStrategy Get(ExpeditionEnemyFaction faction)
    {
        EnsureInitialized();
        strategies.TryGetValue(faction, out var strategy);
        return strategy;
    }
}
