using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using QFramework;
public sealed partial class GameController : CultivationController, ICanSendEvent
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

    private CultivationSaveData saveData;
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
    private CameraFollow2D cameraFollow;
    private CombatHitStop combatHitStop;
    private Transform arenaRoomContentRoot;
    private Vector2 arenaMinBounds;
    private Vector2 arenaMaxBounds;
    private bool shouldResumeOnViewAttach;

    public int RoomCount => rooms.Count;

    public void Initialize(int slotIndex, CultivationSaveData activeSave, WorldRegionDefinition activeRegion)
    {
        currentSlotIndex = slotIndex;
        saveData = activeSave;
        region = activeRegion;

        if (saveData == null || region == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        random = CreateExpeditionRandom();
        hero = ExpeditionBuildFactory.CreateHero(saveData, region);
        ResetFlowStateMachine(ExpeditionFlowPhase.RoomDecision, false);
        torchlight = 82 + hero.Loadout.StartingTorchBonus;
        supplies = 2 + Mathf.CeilToInt(region.DangerRank * 0.5f) + hero.Loadout.StartingSupplyBonus;
        pendingQiGain = 0;
        pendingCrystalGain = 0;
        combatRound = 1;
        recenterUsedInCurrentRoom = false;
        logMessage = string.Empty;
        hintMessage = string.Empty;
        pendingItemRewards.Clear();
        enemies.Clear();
        currentEncounterSnapshot.Clear();
        ClearTransientState();
        currentRoomIndex = 0;
        shouldResumeOnViewAttach = false;

        rooms.Clear();
        rooms.AddRange(BuildExpeditionRooms(region, saveData, random));
        SyncExpeditionRuntime();
    }

    public void SetView(ExpeditionView expeditionView)
    {
        view = expeditionView;
        if (view == null)
        {
            return;
        }

        view.HidePauseOverlay();

        if (shouldResumeOnViewAttach)
        {
            shouldResumeOnViewAttach = false;
            RebuildArenaForCurrentRoom();
            SyncPlayerHealthVisual();
            RefreshView();
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

    public void AttachCombatPresentation(CameraFollow2D follow, CombatHitStop hitStop)
    {
        cameraFollow = follow;
        combatHitStop = hitStop;
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
            if (livePlayer != null)
            {
                var missTarget = origin + new Vector2(livePlayer.IsFacingLeft ? -1.5f : 1.5f, 0f);
                livePlayer.PlayAttackFeedback(missTarget);
                SpawnAttackEffect(origin, missTarget, new Color(0.9f, 0.88f, 0.84f, 0.9f), false);
            }

            PlaySound(SoundType.CombatMiss);
            SpawnCombatText(new Vector3(origin.x, origin.y + 1.1f, 0f), "落空", new Color(0.84f, 0.84f, 0.84f, 1f), false);
            logMessage = "你挥出近身法器，但没能逼到敌方身前。";
            RefreshView();
            return;
        }

        if (livePlayer != null)
        {
            livePlayer.PlayAttackFeedback(targetBinding.View.transform.position);
            SpawnAttackEffect(livePlayer.transform.position, targetBinding.View.transform.position, new Color(1f, 0.92f, 0.64f, 0.94f), true);
        }

        var visualSnapshot = CaptureCombatVisualSnapshot();
        var result = ResolveDirectAttackTurn(
            CreateCombatTurnContext(),
            targetBinding.State,
            damage + TorchAttackBonus(),
            "你挥出近身法器，但没能逼到敌方身前。");
        ApplyCombatTurnResult(result, visualSnapshot);
    }

    public void OnSpiritCollected(SpiritNode node, int qiAmount)
    {
        PlaySound(SoundType.Pickup);
        pendingQiGain += Mathf.Max(1, qiAmount);
        torchlight = Mathf.Min(100, torchlight + 2);
        SyncExpeditionRuntime();
        logMessage = "你收拢了一缕散逸灵机，修为 +" + Mathf.Max(1, qiAmount) + "。";
        RefreshView();
    }

    public void OnHerbCollected(SpiritHerb herb, int healAmount, int qiAmount)
    {
        PlaySound(SoundType.Pickup);
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
        PlaySound(SoundType.Pickup);
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
        this.SendEvent(new ExpeditionHeroHealthChangedEvent { CurrentHealth = hero.CurrentHealth, MaxHealth = hero.MaxHealth });

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

        if (HandlePauseInput())
        {
            return;
        }

        flowStateMachine.Update();
    }

    public bool ShouldBlockRealtimeInput()
    {
        return IsPauseOverlayVisible() || (view != null && view.IsEventOverlayVisible);
    }

    private void EnterRoom(int index)
    {
        ApplyTraversalResult(EnterExpeditionRoom(CreateTraversalContext(index)));
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

    private void CompleteExpedition()
    {
        if (phase == ExpeditionFlowPhase.Completed)
        {
            ReturnToWorldMap();
            return;
        }

        var result = CompleteExpedition(
            currentSlotIndex,
            saveData,
            region,
            hero,
            torchlight,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards);

        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearEventOverlayState();
        ClearPauseOverlayState();
        ClearRoomContent();
        ClearExpeditionRuntime();
        ChangeFlowState(ExpeditionFlowPhase.Completed);
    }

    private void RetreatExpedition()
    {
        if (phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            ReturnToWorldMap();
            return;
        }

        var result = RetreatExpedition(
            currentSlotIndex,
            saveData,
            region,
            pendingQiGain,
            pendingCrystalGain,
            pendingItemRewards);

        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearEventOverlayState();
        ClearPauseOverlayState();
        ClearRoomContent();
        ClearExpeditionRuntime();
        ChangeFlowState(ExpeditionFlowPhase.Retreated);
    }

    private void FailExpedition(string reason)
    {
        currentEncounterSnapshot.Clear();
        ClearEventOverlayState();
        ClearPauseOverlayState();
        var result = FailExpedition(currentSlotIndex, saveData, region, reason, pendingItemRewards);

        logMessage = result.LogMessage;
        SetHint(result.HintMessage);
        ClearRoomContent();
        ClearExpeditionRuntime();
        ChangeFlowState(ExpeditionFlowPhase.Failed);
    }

    private void ReturnToWorldMap()
    {
        ClearPauseOverlayState();
        var targetScene = Application.CanStreamedLevelBeLoaded(worldMapSceneName)
            ? worldMapSceneName
            : mainSceneName;
        SceneFlow.RequestScene(targetScene);
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
            PreviewEnemyIntents(CreateCombatTurnContext()),
            torchlight,
            supplies,
            combatRound,
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
        RefreshPauseOverlay();
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

    private int TorchAttackBonus()
    {
        return torchlight >= 65 ? 1 : 0;
    }

    private bool HandlePauseInput()
    {
        if (view == null || !CanPauseCurrentPhase())
        {
            return false;
        }

        if (IsPauseOverlayVisible())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResumeExpeditionFromPause();
            }

            return true;
        }

        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return false;
        }

        return EnterPausedState();
    }

    private bool EnterPausedState()
    {
        var gameFlowManager = AppRoot.GetGameFlowManager();
        if (gameFlowManager == null || !gameFlowManager.EnterPaused())
        {
            return false;
        }

        RefreshPauseOverlay();
        return true;
    }

    private void ResumeExpeditionFromPause()
    {
        var gameFlowManager = AppRoot.GetGameFlowManager();
        if (gameFlowManager != null)
        {
            gameFlowManager.ResumeGameplay();
        }

        ClearPauseOverlayState();
        RefreshView();
    }

    private void RetreatFromPause()
    {
        ResumeExpeditionFromPause();
        RetreatExpedition();
    }

    private void ReturnToMainMenuFromPause()
    {
        ClearPauseOverlayState();
        ClearExpeditionRuntime();
        SceneFlow.RequestScene(mainSceneName);
    }

    private void RefreshPauseOverlay()
    {
        if (view == null)
        {
            return;
        }

        if (!IsPauseOverlayVisible())
        {
            view.HidePauseOverlay();
            return;
        }

        view.ShowPauseOverlay(
            "历练暂停",
            "当前历练已冻结，时间与场内操作都会停止。\n按 Esc 或点击“继续历练”可返回当前房间。",
            ResumeExpeditionFromPause,
            RetreatFromPause,
            ReturnToMainMenuFromPause);
    }

    private void ClearPauseOverlayState()
    {
        if (view != null)
        {
            view.HidePauseOverlay();
        }
    }

    private bool CanPauseCurrentPhase()
    {
        switch (phase)
        {
            case ExpeditionFlowPhase.RoomDecision:
            case ExpeditionFlowPhase.CombatPlayerTurn:
            case ExpeditionFlowPhase.AfterRoom:
                return true;
            default:
                return false;
        }
    }

    private bool IsPauseOverlayVisible()
    {
        return view != null && view.IsPauseOverlayVisible;
    }

    private void HealHero(int amount)
    {
        hero.CurrentHealth = Mathf.Min(hero.MaxHealth, hero.CurrentHealth + Mathf.Max(0, amount));
        this.SendEvent(new ExpeditionHeroHealthChangedEvent { CurrentHealth = hero.CurrentHealth, MaxHealth = hero.MaxHealth });
        SyncPlayerHealthVisual();
    }

    private void ReceiveDamage(int amount)
    {
        hero.CurrentHealth = Mathf.Max(0, hero.CurrentHealth - Mathf.Max(0, amount));
        this.SendEvent(new ExpeditionHeroHealthChangedEvent { CurrentHealth = hero.CurrentHealth, MaxHealth = hero.MaxHealth });
        SyncPlayerHealthVisual();
        if (hero.CurrentHealth <= 0 && livePlayer == null)
        {
            FailExpedition("远征队在 " + region.DisplayName + " 深处彻底溃散。");
        }
    }

    private void ApplyStress(int amount)
    {
        var mindResult = ApplyCombatMindStress(CreateCombatTurnContext(), amount);
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
