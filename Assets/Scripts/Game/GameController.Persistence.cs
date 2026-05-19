using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class GameController
{
    public void InitializeFromSnapshot(int slotIndex, MainMenuSaveData activeSave, WorldRegionDefinition activeRegion, PersistentExpeditionRuntimeSnapshot snapshot)
    {
        currentSlotIndex = slotIndex;
        saveData = activeSave;
        region = activeRegion;

        if (saveData == null || region == null || snapshot == null)
        {
            Initialize(slotIndex, activeSave, activeRegion);
            return;
        }

        saveData.EnsureDefaults();
        snapshot.EnsureDefaults();
        if (!snapshot.IsUsable())
        {
            Initialize(slotIndex, activeSave, activeRegion);
            return;
        }

        random = CreateExpeditionRandom();
        hero = ExpeditionBuildFactory.CreateHero(saveData, region);
        RestoreHero(snapshot.hero);

        ResetFlowStateMachine(snapshot.phase, false);
        torchlight = snapshot.torchlight;
        supplies = snapshot.supplies;
        pendingQiGain = snapshot.pendingQiGain;
        pendingCrystalGain = snapshot.pendingCrystalGain;
        recenterUsedInCurrentRoom = snapshot.recenterUsedInCurrentRoom;
        logMessage = snapshot.logMessage;
        hintMessage = snapshot.hintMessage;
        pendingItemRewards.Clear();
        AppendPendingItemRewards(snapshot.pendingItemRewards);

        rooms.Clear();
        for (var i = 0; i < snapshot.rooms.Length; i++)
        {
            var room = ExpeditionRoomFactory.Restore(snapshot.rooms[i], region);
            if (room != null)
            {
                rooms.Add(room);
            }
        }

        currentRoomIndex = rooms.Count == 0 ? 0 : Mathf.Clamp(snapshot.currentRoomIndex, 0, rooms.Count - 1);
        combatRound = snapshot.combatRound;
        enemies.Clear();
        for (var i = 0; i < snapshot.enemies.Length; i++)
        {
            var enemy = ExpeditionEnemyFactory.Restore(snapshot.enemies[i]);
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }

        currentEncounterSnapshot.Clear();
        if (enemies.Count > 0)
        {
            currentEncounterSnapshot.AddRange(enemies);
        }

        ClearTransientState();
        shouldResumeOnViewAttach = true;
        SyncExpeditionRuntime();
    }

    private System.Random CreateExpeditionRandom()
    {
        return new System.Random(region.LayoutSeed * 97 + currentSlotIndex * 17 + saveData.realmTier * 13 + region.DangerRank * 19);
    }

    private void RestoreHero(PersistentExpeditionHeroSnapshot snapshot)
    {
        if (hero == null || snapshot == null)
        {
            return;
        }

        snapshot.EnsureDefaults();
        hero.MaxHealth = snapshot.maxHealth;
        hero.CurrentHealth = snapshot.currentHealth;
        hero.Stress = snapshot.stress;
        hero.TalismanCharges = snapshot.talismanCharges;
        hero.MedicineCharges = snapshot.medicineCharges;
        hero.GuardValue = snapshot.guardValue;
        hero.CounterDamage = snapshot.counterDamage;
    }

    private void AppendPendingItemRewards(SaveItemStack[] itemRewards)
    {
        if (itemRewards == null || itemRewards.Length == 0)
        {
            return;
        }

        for (var i = 0; i < itemRewards.Length; i++)
        {
            var stack = itemRewards[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            pendingItemRewards.Add(new SaveItemStack(stack.itemId, stack.quantity));
        }
    }

    private void ClearTransientState()
    {
        activeEventCard = null;
        activeEventResult = null;
    }

    private void SyncExpeditionRuntime()
    {
        if (saveData == null || region == null || hero == null || rooms.Count == 0)
        {
            return;
        }

        CultivationApp.SyncExpeditionRuntime(CreateCombatTurnContext());
        MainMenuSaveStore.SaveExpeditionRuntime(BuildPersistentSnapshot());
    }

    private PersistentExpeditionRuntimeSnapshot BuildPersistentSnapshot()
    {
        var snapshot = new PersistentExpeditionRuntimeSnapshot
        {
            slotIndex = currentSlotIndex,
            regionId = region.Id,
            heroName = saveData.heroName,
            archetypeId = saveData.archetypeId,
            saveRealmTier = saveData.realmTier,
            phase = phase,
            currentRoomIndex = currentRoomIndex,
            combatRound = Mathf.Max(1, combatRound),
            torchlight = torchlight,
            supplies = supplies,
            pendingQiGain = pendingQiGain,
            pendingCrystalGain = pendingCrystalGain,
            recenterUsedInCurrentRoom = recenterUsedInCurrentRoom,
            logMessage = logMessage,
            hintMessage = hintMessage,
            pendingItemRewards = BuildPendingItemRewardSnapshots(),
            rooms = BuildRoomSnapshots(),
            hero = BuildHeroSnapshot(),
            enemies = BuildEnemySnapshots()
        };
        snapshot.EnsureDefaults();
        return snapshot;
    }

    private SaveItemStack[] BuildPendingItemRewardSnapshots()
    {
        if (pendingItemRewards.Count == 0)
        {
            return Array.Empty<SaveItemStack>();
        }

        var snapshots = new List<SaveItemStack>(pendingItemRewards.Count);
        for (var i = 0; i < pendingItemRewards.Count; i++)
        {
            var stack = pendingItemRewards[i];
            if (stack == null || string.IsNullOrWhiteSpace(stack.itemId) || stack.quantity <= 0)
            {
                continue;
            }

            snapshots.Add(new SaveItemStack(stack.itemId, stack.quantity));
        }

        return snapshots.ToArray();
    }

    private PersistentExpeditionRoomSnapshot[] BuildRoomSnapshots()
    {
        if (rooms.Count == 0)
        {
            return Array.Empty<PersistentExpeditionRoomSnapshot>();
        }

        var snapshots = new PersistentExpeditionRoomSnapshot[rooms.Count];
        for (var i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            snapshots[i] = new PersistentExpeditionRoomSnapshot
            {
                index = room.Index,
                kind = room.Kind,
                title = room.Title,
                description = room.Description,
                seed = room.Seed,
                visited = room.Visited,
                resolved = room.Resolved
            };
        }

        return snapshots;
    }

    private PersistentExpeditionHeroSnapshot BuildHeroSnapshot()
    {
        return new PersistentExpeditionHeroSnapshot
        {
            maxHealth = hero.MaxHealth,
            currentHealth = hero.CurrentHealth,
            stress = hero.Stress,
            talismanCharges = hero.TalismanCharges,
            medicineCharges = hero.MedicineCharges,
            guardValue = hero.GuardValue,
            counterDamage = hero.CounterDamage
        };
    }

    private PersistentExpeditionEnemySnapshot[] BuildEnemySnapshots()
    {
        if (enemies.Count == 0)
        {
            return Array.Empty<PersistentExpeditionEnemySnapshot>();
        }

        var snapshots = new PersistentExpeditionEnemySnapshot[enemies.Count];
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            snapshots[i] = new PersistentExpeditionEnemySnapshot
            {
                faction = enemy.Faction,
                name = enemy.Name,
                techniqueName = enemy.TechniqueName,
                maxHealth = enemy.MaxHealth,
                currentHealth = enemy.CurrentHealth,
                damage = enemy.Damage,
                stressDamage = enemy.StressDamage,
                isElite = enemy.IsElite,
                position = enemy.Position,
                armor = enemy.Armor,
                poisonResistance = enemy.PoisonResistance,
                stunResistance = enemy.StunResistance,
                poisonStacks = enemy.PoisonStacks,
                exposedTurns = enemy.ExposedTurns,
                stunnedTurns = enemy.StunnedTurns
            };
        }

        return snapshots;
    }
}
