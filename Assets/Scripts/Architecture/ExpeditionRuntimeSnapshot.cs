using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PersistentExpeditionRuntimeSnapshot
{
    public int slotIndex = -1;
    public string regionId = string.Empty;
    public string heroName = string.Empty;
    public string archetypeId = string.Empty;
    public int saveRealmTier;
    public ExpeditionFlowPhase phase;
    public int currentRoomIndex;
    public int combatRound = 1;
    public int torchlight;
    public int supplies;
    public int pendingQiGain;
    public int pendingCrystalGain;
    public bool recenterUsedInCurrentRoom;
    public string logMessage = string.Empty;
    public string hintMessage = string.Empty;
    public SaveItemStack[] pendingItemRewards = Array.Empty<SaveItemStack>();
    public PersistentExpeditionRoomSnapshot[] rooms = Array.Empty<PersistentExpeditionRoomSnapshot>();
    public PersistentExpeditionHeroSnapshot hero = new PersistentExpeditionHeroSnapshot();
    public PersistentExpeditionEnemySnapshot[] enemies = Array.Empty<PersistentExpeditionEnemySnapshot>();

    public void EnsureDefaults()
    {
        slotIndex = Mathf.Max(-1, slotIndex);
        regionId = regionId ?? string.Empty;
        heroName = heroName ?? string.Empty;
        archetypeId = archetypeId ?? string.Empty;
        saveRealmTier = Mathf.Max(0, saveRealmTier);
        combatRound = Mathf.Max(1, combatRound);
        torchlight = Mathf.Clamp(torchlight, 0, 100);
        supplies = Mathf.Max(0, supplies);
        pendingQiGain = Mathf.Max(0, pendingQiGain);
        pendingCrystalGain = Mathf.Max(0, pendingCrystalGain);
        logMessage = logMessage ?? string.Empty;
        hintMessage = hintMessage ?? string.Empty;
        hero = hero ?? new PersistentExpeditionHeroSnapshot();
        hero.EnsureDefaults();
        pendingItemRewards = NormalizeItems(pendingItemRewards);
        rooms = NormalizeRooms(rooms);
        enemies = NormalizeEnemies(enemies);
        currentRoomIndex = rooms.Length == 0 ? 0 : Mathf.Clamp(currentRoomIndex, 0, rooms.Length - 1);
    }

    public bool IsUsable()
    {
        return slotIndex >= 0
               && !string.IsNullOrWhiteSpace(regionId)
               && !string.IsNullOrWhiteSpace(heroName)
               && !string.IsNullOrWhiteSpace(archetypeId)
               && rooms != null
               && rooms.Length > 0
               && hero != null;
    }

    private static SaveItemStack[] NormalizeItems(SaveItemStack[] source)
    {
        if (source == null || source.Length == 0)
        {
            return Array.Empty<SaveItemStack>();
        }

        var normalized = new List<SaveItemStack>(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            var stack = source[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            normalized.Add(new SaveItemStack(stack.itemId, stack.quantity));
        }

        return normalized.ToArray();
    }

    private static PersistentExpeditionRoomSnapshot[] NormalizeRooms(PersistentExpeditionRoomSnapshot[] source)
    {
        if (source == null || source.Length == 0)
        {
            return Array.Empty<PersistentExpeditionRoomSnapshot>();
        }

        var normalized = new List<PersistentExpeditionRoomSnapshot>(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            var room = source[i];
            if (room == null)
            {
                continue;
            }

            room.EnsureDefaults();
            normalized.Add(room);
        }

        return normalized.ToArray();
    }

    private static PersistentExpeditionEnemySnapshot[] NormalizeEnemies(PersistentExpeditionEnemySnapshot[] source)
    {
        if (source == null || source.Length == 0)
        {
            return Array.Empty<PersistentExpeditionEnemySnapshot>();
        }

        var normalized = new List<PersistentExpeditionEnemySnapshot>(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            var enemy = source[i];
            if (enemy == null)
            {
                continue;
            }

            enemy.EnsureDefaults();
            normalized.Add(enemy);
        }

        return normalized.ToArray();
    }
}

[Serializable]
public sealed class PersistentExpeditionRoomSnapshot
{
    public int index;
    public ExpeditionRoomKind kind;
    public string title = string.Empty;
    public string description = string.Empty;
    public int seed;
    public bool visited;
    public bool resolved;

    public void EnsureDefaults()
    {
        index = Mathf.Max(0, index);
        title = title ?? string.Empty;
        description = description ?? string.Empty;
    }
}

[Serializable]
public sealed class PersistentExpeditionHeroSnapshot
{
    public int maxHealth;
    public int currentHealth;
    public int stress;
    public int talismanCharges;
    public int medicineCharges;
    public int guardValue;
    public int counterDamage;

    public void EnsureDefaults()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        stress = Mathf.Max(0, stress);
        talismanCharges = Mathf.Max(0, talismanCharges);
        medicineCharges = Mathf.Max(0, medicineCharges);
        guardValue = Mathf.Max(0, guardValue);
        counterDamage = Mathf.Max(0, counterDamage);
    }
}

[Serializable]
public sealed class PersistentExpeditionEnemySnapshot
{
    public ExpeditionEnemyFaction faction;
    public string name = string.Empty;
    public string techniqueName = string.Empty;
    public int maxHealth;
    public int currentHealth;
    public int damage;
    public int stressDamage;
    public bool isElite;
    public int position;
    public int armor;
    public int poisonResistance;
    public int stunResistance;
    public int poisonStacks;
    public int exposedTurns;
    public int stunnedTurns;

    public void EnsureDefaults()
    {
        name = name ?? string.Empty;
        techniqueName = techniqueName ?? string.Empty;
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        damage = Mathf.Max(1, damage);
        stressDamage = Mathf.Max(0, stressDamage);
        position = Mathf.Max(0, position);
        armor = Mathf.Max(0, armor);
        poisonResistance = Mathf.Max(0, poisonResistance);
        stunResistance = Mathf.Max(0, stunResistance);
        poisonStacks = Mathf.Max(0, poisonStacks);
        exposedTurns = Mathf.Max(0, exposedTurns);
        stunnedTurns = Mathf.Max(0, stunnedTurns);
    }
}
