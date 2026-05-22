using System.Collections.Generic;
using UnityEngine;

public sealed partial class GameController
{
    private const string HitBurstVfxPath = "Vfx/Combat/HitBurst";
    private const string HealBurstVfxPath = "Vfx/Combat/HealBurst";
    private const string EnemyDefeatVfxPath = "Vfx/Combat/EnemyDefeatBurst";
    private const string SpawnPulseVfxPath = "Vfx/Combat/SpawnPulse";

    private readonly struct CombatVisualSnapshot
    {
        public CombatVisualSnapshot(Dictionary<ExpeditionEnemyState, int> enemyHealthByState, int heroHealth)
        {
            EnemyHealthByState = enemyHealthByState;
            HeroHealth = heroHealth;
        }

        public Dictionary<ExpeditionEnemyState, int> EnemyHealthByState { get; }
        public int HeroHealth { get; }
    }

    private void RebuildArenaForCurrentRoom()
    {
        if (arenaRoomContentRoot == null || livePlayer == null || rooms.Count == 0)
        {
            return;
        }

        ClearRoomContent();
        livePlayer.transform.position = region.PlayerSpawn;
        SpawnSpawnEffect(livePlayer.transform.position + new Vector3(0f, 0.28f, 0f), 0.95f);

        var room = rooms[currentRoomIndex];
        SpawnRoomDecor(room);
        if (room.Kind == ExpeditionRoomKind.Battle || room.Kind == ExpeditionRoomKind.Elite || room.Kind == ExpeditionRoomKind.Boss)
        {
            SpawnEnemyActors();
            return;
        }

        SpawnEventActors(room);
    }

    private void ClearRoomContent()
    {
        enemyActorBindings.Clear();
        if (arenaRoomContentRoot == null)
        {
            return;
        }

        for (var childIndex = arenaRoomContentRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(arenaRoomContentRoot.GetChild(childIndex).gameObject);
        }
    }

    private void SpawnRoomDecor(ExpeditionRoomState room)
    {
        var accent = new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.22f);
        GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "RoomAccentLeft", new Vector2(-region.ArenaSize.x * 0.28f, 1.8f), new Vector2(0.72f, 2.1f), accent, -8);
        GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "RoomAccentRight", new Vector2(region.ArenaSize.x * 0.28f, 1.8f), new Vector2(0.72f, 2.1f), accent, -8);

        if (!room.Resolved)
        {
            return;
        }

        GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "ResolvedMark", new Vector2(0f, 0.9f), new Vector2(1.2f, 1.2f), new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.34f), -7);
    }

    private void SpawnEnemyActors()
    {
        if (enemies.Count == 0)
        {
            return;
        }

        var count = enemies.Count;
        var ySpan = Mathf.Min(region.ArenaSize.y * 0.52f, 1.9f + count * 0.8f);
        var xStart = region.ArenaSize.x * 0.18f;
        for (var i = 0; i < count; i++)
        {
            var state = enemies[i];
            var y = count == 1 ? 0f : ySpan * 0.5f - i * (ySpan / (count - 1));
            var x = xStart + (state.IsElite ? 1f : 0f) + i * 0.32f;
            var view = GameArenaBuilder.CreateEnemy(arenaRoomContentRoot, new Vector2(x, y), state.Faction, GetFactionColor(state.Faction), state.IsElite);
            view.Configure(this, livePlayer, 1f + region.DangerRank * 0.08f, state.IsElite ? 2 : region.RequiredRealmTier, state.IsElite ? 1 : 0);
            SpawnSpawnEffect(view.transform.position + new Vector3(0f, 0.24f, 0f), state.IsElite ? 1.2f : 1f);
            enemyActorBindings.Add(new EnemyActorBinding
            {
                State = state,
                View = view
            });
        }
    }

    private void SpawnEventActors(ExpeditionRoomState room)
    {
        var randomSource = new System.Random(room.Seed + currentRoomIndex * 17);
        if (room.Resolved)
        {
            return;
        }

        switch (room.Kind)
        {
            case ExpeditionRoomKind.Scout:
                SpawnSpiritNodes(Mathf.Clamp(1 + region.DangerRank / 2, 1, 3), Mathf.Max(1, 1 + region.RequiredRealmTier / 2), randomSource);
                break;
            case ExpeditionRoomKind.Treasure:
                SpawnRelics(1 + region.DangerRank / 3, 1 + region.RequiredRealmTier, randomSource);
                GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "TreasurePile", new Vector2(2.4f, -0.8f), new Vector2(1.1f, 0.8f), new Color(0.52f, 0.4f, 0.18f, 0.72f), -6);
                break;
            case ExpeditionRoomKind.Herb:
                SpawnHerbs(Mathf.Clamp(1 + region.HerbCount / 3, 1, 3), 1 + region.RequiredRealmTier, 1 + region.RequiredRealmTier, randomSource);
                break;
            case ExpeditionRoomKind.Shrine:
                SpawnSpiritNodes(1, 2 + region.RequiredRealmTier / 2, randomSource);
                GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "ShrineCore", new Vector2(0f, 1.4f), new Vector2(1.2f, 1.6f), new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.4f), -6);
                break;
            case ExpeditionRoomKind.Trap:
                GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "TrapShardA", new Vector2(-1.8f, -0.8f), new Vector2(0.45f, 1.35f), new Color(0.58f, 0.22f, 0.18f, 0.76f), -6);
                GameArenaBuilder.CreateDecor(arenaRoomContentRoot, "TrapShardB", new Vector2(1.5f, 0.2f), new Vector2(0.36f, 1f), new Color(0.58f, 0.22f, 0.18f, 0.76f), -6);
                break;
        }
    }

    private void SpawnSpiritNodes(int count, int qiAmount, System.Random randomSource)
    {
        for (var i = 0; i < count; i++)
        {
            var node = GameArenaBuilder.CreateSpiritNode(arenaRoomContentRoot, SampleArenaPoint(randomSource, -0.1f, 0.26f), Color.Lerp(region.AccentColor, Color.white, 0.38f));
            node.Configure(this, qiAmount);
        }
    }

    private void SpawnHerbs(int count, int healAmount, int qiAmount, System.Random randomSource)
    {
        for (var i = 0; i < count; i++)
        {
            var herb = GameArenaBuilder.CreateSpiritHerb(arenaRoomContentRoot, SampleArenaPoint(randomSource, -0.16f, 0.18f), Color.Lerp(region.InnerGroundColor, Color.green, 0.45f));
            herb.Configure(this, healAmount, qiAmount);
        }
    }

    private void SpawnRelics(int count, int crystalAmount, System.Random randomSource)
    {
        for (var i = 0; i < count; i++)
        {
            var relic = GameArenaBuilder.CreateRelic(arenaRoomContentRoot, SampleArenaPoint(randomSource, 0.02f, 0.34f), Color.Lerp(region.AccentColor, new Color(0.92f, 0.82f, 0.55f, 1f), 0.42f));
            relic.Configure(this, crystalAmount);
        }
    }

    private Vector2 SampleArenaPoint(System.Random randomSource, float xBias, float yBias)
    {
        var xMin = arenaMinBounds.x + 1.8f;
        var xMax = arenaMaxBounds.x - 1.2f;
        var yMin = arenaMinBounds.y + 1.2f;
        var yMax = arenaMaxBounds.y - 0.8f;
        var x = Mathf.Lerp(xMin, xMax, Mathf.Clamp01((float)randomSource.NextDouble() + xBias));
        var y = Mathf.Lerp(yMin, yMax, Mathf.Clamp01((float)randomSource.NextDouble() + yBias));
        return new Vector2(x, y);
    }

    private void SyncEnemyActors()
    {
        for (var i = enemyActorBindings.Count - 1; i >= 0; i--)
        {
            var binding = enemyActorBindings[i];
            if (binding == null || binding.State == null || !binding.State.IsAlive || !enemies.Contains(binding.State))
            {
                if (binding != null && binding.View != null)
                {
                    SpawnEnemyDefeatEffect(binding.View.transform.position + new Vector3(0f, 0.2f, 0f), binding.State != null && binding.State.IsElite ? 1.2f : 1f);
                    Destroy(binding.View.gameObject);
                }

                enemyActorBindings.RemoveAt(i);
            }
        }
    }

    private Color GetFactionColor(ExpeditionEnemyFaction faction)
    {
        switch (faction)
        {
            case ExpeditionEnemyFaction.Bandit:
                return new Color(0.46f, 0.34f, 0.24f, 1f);
            case ExpeditionEnemyFaction.Cultivator:
                return new Color(0.54f, 0.16f, 0.2f, 1f);
            case ExpeditionEnemyFaction.Beast:
                return new Color(0.22f, 0.42f, 0.2f, 1f);
            case ExpeditionEnemyFaction.HeartDemon:
                return new Color(0.42f, 0.2f, 0.46f, 1f);
            default:
                return new Color(0.36f, 0.4f, 0.42f, 1f);
        }
    }

    private void RemoveExpiredEnemyStates()
    {
        enemies.RemoveAll(enemy => enemy == null || !enemy.IsAlive);
    }

    private CombatVisualSnapshot CaptureCombatVisualSnapshot()
    {
        var enemyHealthByState = new Dictionary<ExpeditionEnemyState, int>();
        for (var i = 0; i < enemyActorBindings.Count; i++)
        {
            var binding = enemyActorBindings[i];
            if (binding == null || binding.State == null)
            {
                continue;
            }

            enemyHealthByState[binding.State] = binding.State.CurrentHealth;
        }

        return new CombatVisualSnapshot(enemyHealthByState, hero != null ? hero.CurrentHealth : 0);
    }

    private void ApplyCombatVisualFeedback(CombatVisualSnapshot snapshot)
    {
        var enemyDamaged = false;
        var enemyHeavyHit = false;
        for (var i = 0; i < enemyActorBindings.Count; i++)
        {
            var binding = enemyActorBindings[i];
            if (binding == null || binding.State == null || binding.View == null || snapshot.EnemyHealthByState == null)
            {
                continue;
            }

            int previousHealth;
            if (!snapshot.EnemyHealthByState.TryGetValue(binding.State, out previousHealth) || binding.State.CurrentHealth >= previousHealth)
            {
                continue;
            }

            var damageTaken = previousHealth - binding.State.CurrentHealth;
            enemyDamaged = true;
            enemyHeavyHit |= damageTaken >= 3;
            binding.View.PlayHitFeedback();
            TriggerCombatPresentationImpact(true, damageTaken >= 3);
            SpawnImpactEffect(binding.View.transform.position + new Vector3(0f, 0.24f, 0f), new Color(1f, 0.9f, 0.54f, 0.92f), true);
            SpawnCombatText(
                binding.View.transform.position + new Vector3(0f, 0.96f, 0f),
                "-" + damageTaken,
                new Color(1f, 0.87f, 0.42f, 1f),
                true);
        }

        if (enemyDamaged)
        {
            PlaySound(enemyHeavyHit ? SoundType.CombatHitHeavy : SoundType.CombatHitLight);
        }

        if (hero == null || hero.CurrentHealth == snapshot.HeroHealth)
        {
            return;
        }

        if (hero.CurrentHealth < snapshot.HeroHealth)
        {
            PlaySound(SoundType.HeroDamaged);
            if (livePlayer != null)
            {
                livePlayer.PlayDamageFeedback();
                TriggerCombatPresentationImpact(false, false);
                SpawnImpactEffect(livePlayer.transform.position + new Vector3(0f, 0.3f, 0f), new Color(1f, 0.48f, 0.48f, 0.94f), false);
                SpawnCombatText(livePlayer.transform.position + new Vector3(0f, 1.18f, 0f), "-" + (snapshot.HeroHealth - hero.CurrentHealth), new Color(1f, 0.46f, 0.46f, 1f), false);
            }

            return;
        }

        if (livePlayer != null)
        {
            PlaySound(SoundType.HeroHealed);
            livePlayer.PlayHealFeedback();
            SpawnHealEffect(livePlayer.transform.position + new Vector3(0f, 0.3f, 0f), false);
            SpawnCombatText(livePlayer.transform.position + new Vector3(0f, 1.18f, 0f), "+" + (hero.CurrentHealth - snapshot.HeroHealth), new Color(0.52f, 1f, 0.62f, 1f), false);
        }
    }

    public void SpawnCombatText(Vector3 worldPosition, string message, Color color, bool emphasized)
    {
        if (arenaRoomContentRoot == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        GameArenaBuilder.CreateFloatingCombatText(
            arenaRoomContentRoot,
            new Vector3(worldPosition.x, worldPosition.y, 0f),
            message,
            color,
            emphasized ? 44 : 40,
            emphasized ? 0.14f : 0.11f);
    }

    public void SpawnAttackEffect(Vector3 attackerWorldPosition, Vector3 targetWorldPosition, Color color, bool emphasized)
    {
        if (arenaRoomContentRoot == null)
        {
            return;
        }

        var direction = targetWorldPosition - attackerWorldPosition;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector3.right;
        }

        var slashPosition = Vector3.Lerp(attackerWorldPosition, targetWorldPosition, 0.7f);
        GameArenaBuilder.CreateSlashEffect(
            arenaRoomContentRoot,
            new Vector3(slashPosition.x, slashPosition.y, 0f),
            new Vector2(direction.x, direction.y),
            color,
            emphasized);
        SpawnPrefabVfx(SpawnPulseVfxPath, slashPosition, emphasized ? 0.9f : 0.72f);
    }

    public void SpawnImpactEffect(Vector3 worldPosition, Color color, bool emphasized)
    {
        if (arenaRoomContentRoot == null)
        {
            return;
        }

        GameArenaBuilder.CreateImpactBurst(
            arenaRoomContentRoot,
            new Vector3(worldPosition.x, worldPosition.y, 0f),
            color,
            emphasized);
        SpawnPrefabVfx(HitBurstVfxPath, worldPosition, emphasized ? 1.05f : 0.88f);
    }

    public void SpawnHealEffect(Vector3 worldPosition, bool emphasized)
    {
        if (arenaRoomContentRoot == null)
        {
            return;
        }

        GameArenaBuilder.CreateImpactBurst(
            arenaRoomContentRoot,
            new Vector3(worldPosition.x, worldPosition.y, 0f),
            new Color(0.58f, 1f, 0.7f, 0.92f),
            emphasized);
        SpawnPrefabVfx(HealBurstVfxPath, worldPosition, emphasized ? 1.08f : 0.92f);
    }

    public void SpawnEnemyDefeatEffect(Vector3 worldPosition, float scale = 1f)
    {
        SpawnPrefabVfx(EnemyDefeatVfxPath, worldPosition, Mathf.Max(0.8f, scale));
    }

    public void SpawnSpawnEffect(Vector3 worldPosition, float scale = 1f)
    {
        SpawnPrefabVfx(SpawnPulseVfxPath, worldPosition, Mathf.Max(0.7f, scale));
    }

    private void SpawnPrefabVfx(string resourcePath, Vector3 worldPosition, float scale)
    {
        if (arenaRoomContentRoot == null || string.IsNullOrWhiteSpace(resourcePath))
        {
            return;
        }

        var instance = InstantiatePrefab(resourcePath, arenaRoomContentRoot);
        if (instance == null)
        {
            return;
        }

        instance.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        instance.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
    }

    private void TriggerCombatPresentationImpact(bool enemyHit, bool heavy)
    {
        if (cameraFollow != null)
        {
            cameraFollow.AddImpulse(heavy ? 0.22f : enemyHit ? 0.14f : 0.11f, heavy ? 0.18f : 0.12f);
        }

        if (combatHitStop != null)
        {
            combatHitStop.Trigger(heavy ? 0.06f : 0.035f, enemyHit ? 0.08f : 0.12f);
        }
    }
}
