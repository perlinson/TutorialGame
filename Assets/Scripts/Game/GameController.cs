using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameController : MonoBehaviour
{
    private sealed class EnemyActorBinding
    {
        public ExpeditionEnemyState State;
        public SpiritEnemy View;
    }

    [SerializeField] private string worldMapSceneName = "WorldMap";
    [SerializeField] private string mainSceneName = "Main";

    private readonly List<ExpeditionRoomState> rooms = new List<ExpeditionRoomState>();
    private readonly List<ExpeditionEnemyState> enemies = new List<ExpeditionEnemyState>();
    private readonly List<EnemyActorBinding> enemyActorBindings = new List<EnemyActorBinding>();

    private MainMenuSaveData saveData;
    private WorldRegionDefinition region;
    private ExpeditionView view;
    private ExpeditionHeroState hero;
    private ExpeditionFlowPhase phase;
    private System.Random random;
    private int currentSlotIndex = -1;
    private int currentRoomIndex;
    private int torchlight;
    private int supplies;
    private int pendingQiGain;
    private int pendingCrystalGain;
    private readonly List<SaveItemStack> pendingItemRewards = new List<SaveItemStack>();
    private readonly List<ExpeditionEnemyState> currentEncounterSnapshot = new List<ExpeditionEnemyState>();
    private int combatRound;
    private bool recenterUsedInCurrentRoom;
    private string logMessage = string.Empty;
    private string hintMessage = string.Empty;
    private ExpeditionEventCardResult activeEventCard;
    private ExpeditionEventOptionResult activeEventResult;
    private PlayerCultivator livePlayer;
    private Transform arenaRoomContentRoot;
    private Vector2 arenaMinBounds;
    private Vector2 arenaMaxBounds;

    public int RoomCount => rooms.Count;

    public void Initialize(int slotIndex, MainMenuSaveData activeSave, WorldRegionDefinition activeRegion)
    {
        currentSlotIndex = slotIndex;
        saveData = activeSave;
        region = activeRegion;

        if (saveData == null || region == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        random = new System.Random(region.LayoutSeed * 97 + currentSlotIndex * 17 + saveData.realmTier * 13 + region.DangerRank * 19);
        hero = ExpeditionBuildFactory.CreateHero(saveData, region);

        torchlight = 82 + hero.Loadout.StartingTorchBonus;
        supplies = 2 + Mathf.CeilToInt(region.DangerRank * 0.5f) + hero.Loadout.StartingSupplyBonus;
        pendingQiGain = 0;
        pendingCrystalGain = 0;
        pendingItemRewards.Clear();
        currentEncounterSnapshot.Clear();
        currentRoomIndex = 0;

        rooms.Clear();
        rooms.AddRange(CultivationApp.BuildExpeditionRooms(region, saveData, random));
        SyncExpeditionRuntime();
    }

    public void SetView(ExpeditionView expeditionView)
    {
        view = expeditionView;
        if (view == null)
        {
            return;
        }

        EnterRoom(0);
    }

    public void AttachArena(GameArenaRuntimeBindings arena)
    {
        if (arena == null || hero == null || region == null)
        {
            return;
        }

        arenaRoomContentRoot = arena.RoomContentRoot;
        livePlayer = arena.Player;
        arenaMinBounds = new Vector2(-region.ArenaSize.x * 0.5f + 0.9f, -region.ArenaSize.y * 0.5f + 0.9f);
        arenaMaxBounds = new Vector2(region.ArenaSize.x * 0.5f - 0.9f, region.ArenaSize.y * 0.5f - 0.9f);

        if (livePlayer != null)
        {
            livePlayer.Configure(this, arenaMinBounds, arenaMaxBounds, hero.AttackBonus, saveData.vitalityLevel, hero.CurrentHealth, hero.MaxHealth);
            livePlayer.transform.position = region.PlayerSpawn;
        }
    }

    public void HandlePlayerAttack(Vector2 origin, float range, int damage)
    {
        if (phase != ExpeditionFlowPhase.CombatPlayerTurn)
        {
            return;
        }

        EnemyActorBinding targetBinding = null;
        var bestDistance = float.MaxValue;
        for (var i = 0; i < enemyActorBindings.Count; i++)
        {
            var binding = enemyActorBindings[i];
            if (binding == null || binding.State == null || !binding.State.IsAlive || binding.View == null)
            {
                continue;
            }

            var distance = Vector2.Distance(origin, binding.View.transform.position);
            if (distance > range || distance >= bestDistance)
            {
                continue;
            }

            targetBinding = binding;
            bestDistance = distance;
        }

        if (targetBinding == null)
        {
            logMessage = "你挥出近身法器，但没能逼到敌方身前。";
            RefreshView();
            return;
        }

        targetBinding.View.PlayHitFeedback();
        var result = CultivationApp.ResolveDirectAttackTurn(
            CreateCombatTurnContext(),
            targetBinding.State,
            damage + TorchAttackBonus(),
            "你挥出近身法器，但没能逼到敌方身前。");
        ApplyCombatTurnResult(result);
    }

    public void OnSpiritCollected(SpiritNode node, int qiAmount)
    {
        pendingQiGain += Mathf.Max(1, qiAmount);
        torchlight = Mathf.Min(100, torchlight + 2);
        SyncExpeditionRuntime();
        logMessage = "你收拢了一缕散逸灵机，修为 +" + Mathf.Max(1, qiAmount) + "。";
        RefreshView();
    }

    public void OnHerbCollected(SpiritHerb herb, int healAmount, int qiAmount)
    {
        if (livePlayer != null)
        {
            livePlayer.Heal(healAmount);
        }
        else
        {
            HealHero(healAmount);
        }

        pendingQiGain += Mathf.Max(1, qiAmount);
        ApplyStress(-4);
        SyncExpeditionRuntime();
        logMessage = "采下一株可用灵草，气血恢复 " + Mathf.Max(1, healAmount) + "，修为 +" + Mathf.Max(1, qiAmount) + "。";
        RefreshView();
    }

    public void OnRelicRecovered(TrialRelic relic, int crystalAmount)
    {
        pendingCrystalGain += Mathf.Max(1, crystalAmount);
        SyncExpeditionRuntime();
        logMessage = "你从残损遗物中剥离出灵石 +" + Mathf.Max(1, crystalAmount) + "。";
        RefreshView();
    }

    public void OnEnemyDefeated(SpiritEnemy enemy)
    {
        for (var i = 0; i < enemyActorBindings.Count; i++)
        {
            var binding = enemyActorBindings[i];
            if (binding == null || binding.View != enemy || binding.State == null)
            {
                continue;
            }

            binding.State.CurrentHealth = 0;
            RemoveExpiredEnemyStates();
            SyncEnemyActors();
            RefreshView();
            return;
        }
    }

    public void OnPlayerHealthChanged(int currentHp, int maxHp)
    {
        if (hero == null)
        {
            return;
        }

        hero.MaxHealth = Mathf.Max(hero.MaxHealth, maxHp);
        hero.CurrentHealth = Mathf.Clamp(currentHp, 0, hero.MaxHealth);
        if (hero.CurrentHealth <= 0 && phase != ExpeditionFlowPhase.Failed && phase != ExpeditionFlowPhase.Completed && phase != ExpeditionFlowPhase.Retreated)
        {
            FailExpedition("远征队在 " + region.DisplayName + " 深处彻底溃散。");
            return;
        }

        SyncExpeditionRuntime();
        RefreshView();
    }

    private void Update()
    {
        if (view != null && view.IsEventOverlayVisible)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && activeEventResult != null)
            {
                ConfirmOpenEventResult();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Failed || phase == ExpeditionFlowPhase.Retreated)
            {
                ReturnToWorldMap();
            }
            else
            {
                RetreatExpedition();
            }

            return;
        }

        if (phase == ExpeditionFlowPhase.CombatPlayerTurn)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PerformSkill(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PerformSkill(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PerformSkill(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                PerformSkill(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                UseTalisman();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                UseMedicine();
            }
        }
        else if ((phase == ExpeditionFlowPhase.AfterRoom || phase == ExpeditionFlowPhase.RoomDecision) && Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceOrSearch();
        }
    }

    private void EnterRoom(int index)
    {
        ApplyTraversalResult(CultivationApp.EnterRoom(CreateTraversalContext(index)));
    }

    private void StartCombat(ExpeditionRoomState room)
    {
        enemies.Clear();
        combatRound = 1;
        enemies.AddRange(CultivationApp.BuildEncounterEnemies(region, room, saveData, random));
        currentEncounterSnapshot.Clear();
        currentEncounterSnapshot.AddRange(enemies);

        phase = ExpeditionFlowPhase.CombatPlayerTurn;
        logMessage = room.Kind == ExpeditionRoomKind.Boss
            ? "前方核心灵压暴涨，凶煞、邪修与残阵气息纠缠在一起。"
            : "黑暗中有气机锁定了远征队，只能当场开战。";
        SetHint("战斗重点不是无脑输出，而是看门派技能和随身法器如何稳住节奏。");
        RebuildArenaForCurrentRoom();
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void AdvanceOrSearch()
    {
        HandleAdvanceResult(CultivationApp.AdvanceExpedition(CreateAdvanceContext()));
    }

    private void SearchCurrentRoom()
    {
        var room = rooms[currentRoomIndex];
        if (room.Resolved)
        {
            phase = ExpeditionFlowPhase.AfterRoom;
            SyncExpeditionRuntime();
            RefreshView();
            return;
        }

        activeEventCard = CultivationApp.OpenRoomEvent(CreateCombatTurnContext());
        activeEventResult = null;
        if (activeEventCard == null || !string.IsNullOrWhiteSpace(activeEventCard.FailureReason))
        {
            logMessage = activeEventCard != null ? activeEventCard.FailureReason : "当前没有可处理的历练事件。";
            RefreshView();
            return;
        }

        SetHint("从这张事件卡里挑一种处理方式。");
        RefreshView();
        view.ShowEventCard(activeEventCard, OnEventOptionSelected);
    }

    private void PerformSkill(int skillIndex)
    {
        if (phase != ExpeditionFlowPhase.CombatPlayerTurn || skillIndex < 0 || skillIndex >= hero.Skills.Count)
        {
            return;
        }
        ApplyCombatTurnResult(CultivationApp.ResolveSkillTurn(CreateCombatTurnContext(), skillIndex));
    }

    private void UseTalisman()
    {
        if (phase != ExpeditionFlowPhase.CombatPlayerTurn || hero.TalismanCharges <= 0)
        {
            return;
        }
        ApplyCombatTurnResult(CultivationApp.ResolveTalismanTurn(CreateCombatTurnContext()));
    }

    private void UseMedicine()
    {
        if (phase != ExpeditionFlowPhase.CombatPlayerTurn || hero.MedicineCharges <= 0)
        {
            return;
        }
        ApplyCombatTurnResult(CultivationApp.ResolveMedicineTurn(CreateCombatTurnContext()));
    }

    private CombatTurnContext CreateCombatTurnContext()
    {
        var room = currentRoomIndex >= 0 && currentRoomIndex < rooms.Count
            ? rooms[currentRoomIndex]
            : null;

        return new CombatTurnContext
        {
            SaveData = saveData,
            Region = region,
            Room = room,
            Hero = hero,
            Enemies = enemies,
            CurrentEncounterSnapshot = currentEncounterSnapshot,
            PendingItemRewards = pendingItemRewards,
            CurrentRoomIndex = currentRoomIndex,
            CombatRound = combatRound,
            Torchlight = torchlight,
            Supplies = supplies,
            PendingQiGain = pendingQiGain,
            PendingCrystalGain = pendingCrystalGain
        };
    }

    private void ApplyCombatTurnResult(CombatTurnResult result)
    {
        if (result == null)
        {
            return;
        }

        combatRound = result.CombatRound;
        torchlight = result.Torchlight;
        supplies = result.Supplies;
        pendingQiGain = result.PendingQiGain;
        pendingCrystalGain = result.PendingCrystalGain;

        RemoveExpiredEnemyStates();
        SyncEnemyActors();
        SyncPlayerHealthVisual();

        if (result.ExpeditionFailed)
        {
            FailExpedition(string.IsNullOrWhiteSpace(result.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : result.FailureReason);
            return;
        }

        if (result.CombatCleared)
        {
            phase = ExpeditionFlowPhase.AfterRoom;
            logMessage = result.LogMessage;
            SetHint(result.HintMessage);
            RebuildArenaForCurrentRoom();
            SyncExpeditionRuntime();
            RefreshView();
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.LogMessage))
        {
            phase = ExpeditionFlowPhase.CombatPlayerTurn;
            logMessage = result.LogMessage;
            if (!string.IsNullOrWhiteSpace(result.HintMessage))
            {
                SetHint(result.HintMessage);
            }

            SyncExpeditionRuntime();
            RefreshView();
        }
    }


    private void CompleteExpedition()
    {
        if (phase == ExpeditionFlowPhase.Completed)
        {
            ReturnToWorldMap();
            return;
        }

        var result = CultivationApp.CompleteExpedition(
            currentSlotIndex,
            saveData,
            region,
            hero,
            torchlight,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards);

        phase = ExpeditionFlowPhase.Completed;
        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearEventOverlayState();
        ClearRoomContent();
        CultivationApp.ClearExpeditionRuntime();
        RefreshView();
    }

    private void RetreatExpedition()
    {
        if (phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            ReturnToWorldMap();
            return;
        }

        var result = CultivationApp.RetreatExpedition(
            currentSlotIndex,
            saveData,
            region,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards);

        phase = ExpeditionFlowPhase.Retreated;
        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearEventOverlayState();
        ClearRoomContent();
        CultivationApp.ClearExpeditionRuntime();
        RefreshView();
    }

    private void FailExpedition(string reason)
    {
        currentEncounterSnapshot.Clear();
        ClearEventOverlayState();
        var result = CultivationApp.FailExpedition(currentSlotIndex, saveData, region, reason, pendingItemRewards);

        phase = ExpeditionFlowPhase.Failed;
        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearRoomContent();
        CultivationApp.ClearExpeditionRuntime();
        RefreshView();
    }

    private void ReturnToWorldMap()
    {
        var targetScene = Application.CanStreamedLevelBeLoaded(worldMapSceneName)
            ? worldMapSceneName
            : mainSceneName;
        SceneManager.LoadScene(targetScene);
    }

    private void RefreshView()
    {
        if (view == null || saveData == null || region == null || hero == null || rooms.Count == 0)
        {
            return;
        }

        var snapshot = ExpeditionUiComposer.Build(
            saveData,
            region,
            hero,
            rooms,
            currentRoomIndex,
            enemies,
            torchlight,
            supplies,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards,
            phase,
            logMessage,
            hintMessage);
        view.SetHeader(
            snapshot.HeaderTitle,
            snapshot.HeaderSubtitle,
            snapshot.HeaderStatus,
            snapshot.HeaderResources,
            snapshot.PhaseName);

        view.SetRoomContent(
            snapshot.RoomTitle,
            snapshot.RoomDescription,
            snapshot.LoadoutSummary,
            snapshot.EnemySummary,
            snapshot.LogMessage,
            snapshot.SkillSummary,
            snapshot.HintMessage);
        view.SetVisuals(
            snapshot.RoomIllustration,
            snapshot.RoomIllustrationTitle,
            snapshot.HeroPortrait,
            snapshot.HeroPortraitTitle,
            snapshot.EnemyPortrait,
            snapshot.EnemyPortraitTitle);

        view.SetTrack(rooms, currentRoomIndex);
        ConfigureActions();
    }

    private void ConfigureActions()
    {
        view.ClearActions();

        switch (phase)
        {
            case ExpeditionFlowPhase.RoomDecision:
                view.SetAction(0, "搜查此室", true, SearchCurrentRoom);
                view.SetAction(1, "整备灵灯", supplies > 0, UseTorchSupply);
                view.SetAction(2, "短暂扎营", supplies > 0, CampAndRecover);
                view.SetAction(3, "谨慎通过", true, SkipCurrentRoom);
                view.SetAction(4, "整理行囊", true, RecenterMind);
                view.SetAction(5, "提前撤离", true, RetreatExpedition);
                break;
            case ExpeditionFlowPhase.CombatPlayerTurn:
                for (var i = 0; i < hero.Skills.Count && i < 4; i++)
                {
                    var capturedIndex = i;
                    view.SetAction(i, hero.Skills[i].Name, true, () => PerformSkill(capturedIndex), hero.Skills[i].IconImage);
                }

                view.SetAction(4, hero.Loadout.TalismanName, hero.TalismanCharges > 0, UseTalisman);
                view.SetAction(5, hero.Loadout.MedicineName, hero.MedicineCharges > 0, UseMedicine);
                break;
            case ExpeditionFlowPhase.AfterRoom:
                view.SetAction(0, currentRoomIndex >= rooms.Count - 1 ? "结束远征" : "前往下一室", true, AdvanceToNextPhase);
                view.SetAction(1, "整备灵灯", supplies > 0, UseTorchSupply);
                view.SetAction(2, "短暂扎营", supplies > 0, CampAndRecover);
                view.SetAction(3, "整理行囊", true, RecenterMind);
                view.SetAction(4, "提前撤离", true, RetreatExpedition);
                break;
            default:
                view.SetAction(0, "返回山海图", true, ReturnToWorldMap);
                break;
        }
    }

    private void UseTorchSupply()
    {
        if (supplies <= 0 || phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            return;
        }

        ApplySupportActionResult(CultivationApp.UseTorchSupply(CreateCombatTurnContext()));
    }

    private void CampAndRecover()
    {
        if (supplies <= 0 || phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            return;
        }

        ApplySupportActionResult(CultivationApp.CampAndRecover(CreateCombatTurnContext()));
    }

    private void RecenterMind()
    {
        if (recenterUsedInCurrentRoom)
        {
            logMessage = "这一室里你已经整理过一次行囊，再拖延只会让煞气逼近。";
            RefreshView();
            return;
        }

        recenterUsedInCurrentRoom = true;
        ApplySupportActionResult(CultivationApp.RecenterMind(CreateCombatTurnContext()));
    }

    private void SkipCurrentRoom()
    {
        ApplySupportActionResult(CultivationApp.SkipRoom(CreateCombatTurnContext()), ExpeditionFlowPhase.AfterRoom);
    }

    private void AdvanceToNextPhase()
    {
        HandleAdvanceResult(CultivationApp.AdvanceExpedition(CreateAdvanceContext()));
    }

    private void OnEventOptionSelected(string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId) || activeEventCard == null)
        {
            return;
        }

        activeEventResult = CultivationApp.ResolveEventOption(CreateCombatTurnContext(), activeEventCard.EventId, optionId);
        if (activeEventResult == null)
        {
            return;
        }

        torchlight = activeEventResult.Torchlight;
        supplies = activeEventResult.Supplies;
        pendingQiGain = activeEventResult.PendingQiGain;
        pendingCrystalGain = activeEventResult.PendingCrystalGain;
        SyncPlayerHealthVisual();

        if (activeEventResult.ExpeditionFailed)
        {
            FailExpedition(string.IsNullOrWhiteSpace(activeEventResult.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : activeEventResult.FailureReason);
            return;
        }

        view.ShowEventResult(activeEventCard, activeEventResult, ConfirmOpenEventResult);
    }

    private void ConfirmOpenEventResult()
    {
        if (activeEventResult == null)
        {
            ClearEventOverlayState();
            RefreshView();
            return;
        }

        if (currentRoomIndex >= 0 && currentRoomIndex < rooms.Count)
        {
            rooms[currentRoomIndex].Resolved = activeEventResult.RoomResolved;
        }

        phase = ExpeditionFlowPhase.AfterRoom;
        logMessage = activeEventResult.LogMessage;
        if (!string.IsNullOrWhiteSpace(activeEventResult.HintMessage))
        {
            SetHint(activeEventResult.HintMessage);
        }

        ClearEventOverlayState();
        RebuildArenaForCurrentRoom();
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void RebuildArenaForCurrentRoom()
    {
        if (arenaRoomContentRoot == null || livePlayer == null || rooms.Count == 0)
        {
            return;
        }

        ClearRoomContent();
        livePlayer.transform.position = region.PlayerSpawn;

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
            var view = GameArenaBuilder.CreateEnemy(arenaRoomContentRoot, new Vector2(x, y), GetFactionColor(state.Faction), state.IsElite);
            view.Configure(this, livePlayer, 1f + region.DangerRank * 0.08f, state.IsElite ? 2 : region.RequiredRealmTier, state.IsElite ? 1 : 0);
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

    private int TorchAttackBonus()
    {
        return torchlight >= 65 ? 1 : 0;
    }

    private void HealHero(int amount)
    {
        hero.CurrentHealth = Mathf.Min(hero.MaxHealth, hero.CurrentHealth + Mathf.Max(0, amount));
        SyncPlayerHealthVisual();
    }

    private void ReceiveDamage(int amount)
    {
        hero.CurrentHealth = Mathf.Max(0, hero.CurrentHealth - Mathf.Max(0, amount));
        SyncPlayerHealthVisual();
        if (hero.CurrentHealth <= 0 && livePlayer == null)
        {
            FailExpedition("远征队在 " + region.DisplayName + " 深处彻底溃散。");
        }
    }

    private void ApplyStress(int amount)
    {
        var mindResult = CultivationApp.ApplyMindStress(CreateCombatTurnContext(), amount);
        if (mindResult.ExpeditionFailed)
        {
            SyncPlayerHealthVisual();
            FailExpedition(string.IsNullOrWhiteSpace(mindResult.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : mindResult.FailureReason);
            return;
        }

        SyncPlayerHealthVisual();
        if (!string.IsNullOrWhiteSpace(mindResult.Message) && mindResult.BreakdownTriggered && phase != ExpeditionFlowPhase.Failed)
        {
            logMessage += string.IsNullOrEmpty(logMessage) ? string.Empty : "\n";
            logMessage += mindResult.Message;
        }
    }

    private void SetHint(string message)
    {
        hintMessage = message;
    }

    private ExpeditionTraversalContext CreateTraversalContext(int roomIndex)
    {
        var clampedRoomIndex = rooms.Count > 0 ? Mathf.Clamp(roomIndex, 0, rooms.Count - 1) : 0;
        return new ExpeditionTraversalContext
        {
            Region = region,
            Hero = hero,
            Room = rooms.Count > 0 ? rooms[clampedRoomIndex] : null,
            RoomIndex = clampedRoomIndex,
            RoomCount = rooms.Count,
            Torchlight = torchlight
        };
    }

    private ExpeditionAdvanceContext CreateAdvanceContext()
    {
        return new ExpeditionAdvanceContext
        {
            Phase = phase,
            CurrentRoomIndex = currentRoomIndex,
            RoomCount = rooms.Count
        };
    }

    private void ApplyTraversalResult(ExpeditionTraversalResult result)
    {
        if (result == null)
        {
            return;
        }

        if (result.ExpeditionFailed)
        {
            FailExpedition(string.IsNullOrWhiteSpace(result.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : result.FailureReason);
            return;
        }

        currentRoomIndex = result.RoomIndex;
        torchlight = result.Torchlight;
        phase = result.Phase;
        recenterUsedInCurrentRoom = false;
        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearEventOverlayState();

        if (result.StartCombat)
        {
            StartCombat(rooms[currentRoomIndex]);
            return;
        }

        enemies.Clear();
        RebuildArenaForCurrentRoom();
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void HandleAdvanceResult(ExpeditionAdvanceResult result)
    {
        if (result == null)
        {
            return;
        }

        if (result.ShouldSearchCurrentRoom)
        {
            SearchCurrentRoom();
            return;
        }

        if (result.ShouldCompleteExpedition)
        {
            CompleteExpedition();
            return;
        }

        if (result.ShouldEnterNextRoom)
        {
            EnterRoom(result.NextRoomIndex);
            return;
        }

        if (result.ShouldReturnToWorldMap)
        {
            ReturnToWorldMap();
        }
    }

    private void ApplySupportActionResult(ExpeditionSupportActionResult result, ExpeditionFlowPhase? nextPhase = null)
    {
        if (result == null)
        {
            return;
        }

        torchlight = result.Torchlight;
        supplies = result.Supplies;
        SyncPlayerHealthVisual();

        if (result.ExpeditionFailed)
        {
            FailExpedition(string.IsNullOrWhiteSpace(result.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : result.FailureReason);
            return;
        }

        if (nextPhase.HasValue)
        {
            phase = nextPhase.Value;
        }

        if (result.RoomResolved)
        {
            RebuildArenaForCurrentRoom();
        }

        if (!string.IsNullOrWhiteSpace(result.HintMessage))
        {
            SetHint(result.HintMessage);
        }

        logMessage = !string.IsNullOrWhiteSpace(result.LogMessage)
            ? result.LogMessage
            : result.FailureReason;
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void ClearEventOverlayState()
    {
        activeEventCard = null;
        activeEventResult = null;
        if (view != null)
        {
            view.HideEventOverlay();
        }
    }

    private void SyncExpeditionRuntime()
    {
        if (saveData == null || region == null || hero == null || rooms.Count == 0)
        {
            return;
        }

        CultivationApp.SyncExpeditionRuntime(CreateCombatTurnContext());
    }

    private void SyncPlayerHealthVisual()
    {
        if (livePlayer == null)
        {
            return;
        }

        if (livePlayer.CurrentHealth == hero.CurrentHealth && livePlayer.MaxHealth == hero.MaxHealth)
        {
            return;
        }

        livePlayer.SyncHealth(hero.CurrentHealth, hero.MaxHealth);
    }
}
