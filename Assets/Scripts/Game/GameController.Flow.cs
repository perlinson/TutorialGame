using UnityEngine;

public sealed partial class GameController
{
    private void StartCombat(ExpeditionRoomState room)
    {
        enemies.Clear();
        combatRound = 1;
        enemies.AddRange(BuildEncounterEnemies(region, room, saveData, random));
        currentEncounterSnapshot.Clear();
        currentEncounterSnapshot.AddRange(enemies);

        PlaySound(SoundType.BattleStart);
        logMessage = room.Kind == ExpeditionRoomKind.Boss
            ? "前方核心灵压暴涨，凶煞、邪修与残阵气息纠缠在一起。"
            : "黑暗中有气机锁定了远征队，只能当场开战。";
        SetHint("战斗重点不是无脑输出，而是看门派技能和随身法器如何稳住节奏。");
        ChangeFlowState(ExpeditionFlowPhase.CombatPlayerTurn);
    }

    private void AdvanceOrSearch()
    {
        HandleAdvanceResult(AdvanceExpedition(CreateAdvanceContext()));
    }

    private void SearchCurrentRoom()
    {
        var room = rooms[currentRoomIndex];
        if (room.Resolved)
        {
            ChangeFlowState(ExpeditionFlowPhase.AfterRoom);
            return;
        }

        activeEventCard = OpenRoomEvent(CreateCombatTurnContext());
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

        var visualSnapshot = CaptureCombatVisualSnapshot();
        if (livePlayer != null && enemyActorBindings.Count > 0 && enemyActorBindings[0] != null && enemyActorBindings[0].View != null)
        {
            livePlayer.PlayAttackFeedback(enemyActorBindings[0].View.transform.position);
            SpawnAttackEffect(livePlayer.transform.position, enemyActorBindings[0].View.transform.position, new Color(1f, 0.84f, 0.52f, 0.94f), true);
        }

        ApplyCombatTurnResult(ResolveSkillTurn(CreateCombatTurnContext(), skillIndex), visualSnapshot);
    }

    private void UseTalisman()
    {
        if (phase != ExpeditionFlowPhase.CombatPlayerTurn || hero.TalismanCharges <= 0)
        {
            return;
        }

        var visualSnapshot = CaptureCombatVisualSnapshot();
        if (livePlayer != null && enemyActorBindings.Count > 0 && enemyActorBindings[0] != null && enemyActorBindings[0].View != null)
        {
            livePlayer.PlayAttackFeedback(enemyActorBindings[0].View.transform.position);
            SpawnAttackEffect(livePlayer.transform.position, enemyActorBindings[0].View.transform.position, new Color(0.72f, 0.9f, 1f, 0.94f), true);
        }

        ApplyCombatTurnResult(ResolveTalismanTurn(CreateCombatTurnContext()), visualSnapshot);
    }

    private void UseMedicine()
    {
        if (phase != ExpeditionFlowPhase.CombatPlayerTurn || hero.MedicineCharges <= 0)
        {
            return;
        }

        var visualSnapshot = CaptureCombatVisualSnapshot();
        ApplyCombatTurnResult(ResolveMedicineTurn(CreateCombatTurnContext()), visualSnapshot);
    }

    private void ApplyCombatTurnResult(CombatTurnResult result, CombatVisualSnapshot? visualSnapshot = null)
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
            if (visualSnapshot.HasValue)
            {
                ApplyCombatVisualFeedback(visualSnapshot.Value);
            }

            FailExpedition(string.IsNullOrWhiteSpace(result.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : result.FailureReason);
            return;
        }

        if (visualSnapshot.HasValue)
        {
            ApplyCombatVisualFeedback(visualSnapshot.Value);
        }

        if (result.CombatCleared)
        {
            currentEncounterSnapshot.Clear();
            logMessage = result.LogMessage;
            SetHint(result.HintMessage);
            ChangeFlowState(ExpeditionFlowPhase.AfterRoom);
            return;
        }

        if (!string.IsNullOrWhiteSpace(result.LogMessage))
        {
            logMessage = result.LogMessage;
            if (!string.IsNullOrWhiteSpace(result.HintMessage))
            {
                SetHint(result.HintMessage);
            }

            SyncExpeditionRuntime();
            RefreshView();
        }
    }

    private void UseTorchSupply()
    {
        if (supplies <= 0 || phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            return;
        }

        var visualSnapshot = CaptureCombatVisualSnapshot();
        ApplySupportActionResult(UseTorchSupply(CreateCombatTurnContext()), null, visualSnapshot);
    }

    private void CampAndRecover()
    {
        if (supplies <= 0 || phase == ExpeditionFlowPhase.Completed || phase == ExpeditionFlowPhase.Retreated || phase == ExpeditionFlowPhase.Failed)
        {
            return;
        }

        var visualSnapshot = CaptureCombatVisualSnapshot();
        ApplySupportActionResult(CampAndRecover(CreateCombatTurnContext()), null, visualSnapshot);
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
        var visualSnapshot = CaptureCombatVisualSnapshot();
        ApplySupportActionResult(RecenterMind(CreateCombatTurnContext()), null, visualSnapshot);
    }

    private void SkipCurrentRoom()
    {
        var visualSnapshot = CaptureCombatVisualSnapshot();
        ApplySupportActionResult(SkipRoom(CreateCombatTurnContext()), ExpeditionFlowPhase.AfterRoom, visualSnapshot);
    }

    private void AdvanceToNextPhase()
    {
        HandleAdvanceResult(AdvanceExpedition(CreateAdvanceContext()));
    }

    private void OnEventOptionSelected(string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId) || activeEventCard == null)
        {
            return;
        }

        activeEventResult = ResolveEventOption(CreateCombatTurnContext(), activeEventCard.EventId, optionId);
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

        logMessage = activeEventResult.LogMessage;
        if (!string.IsNullOrWhiteSpace(activeEventResult.HintMessage))
        {
            SetHint(activeEventResult.HintMessage);
        }

        ClearEventOverlayState();
        ChangeFlowState(ExpeditionFlowPhase.AfterRoom);
    }

    private void SetHint(string message)
    {
        hintMessage = message ?? string.Empty;
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
        currentEncounterSnapshot.Clear();
        if (phase != result.Phase)
        {
            ChangeFlowState(result.Phase);
            return;
        }

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

    private void ApplySupportActionResult(ExpeditionSupportActionResult result, ExpeditionFlowPhase? nextPhase = null, CombatVisualSnapshot? visualSnapshot = null)
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
            if (visualSnapshot.HasValue)
            {
                ApplyCombatVisualFeedback(visualSnapshot.Value);
            }

            FailExpedition(string.IsNullOrWhiteSpace(result.FailureReason) ? "远征队在 " + region.DisplayName + " 深处彻底溃散。" : result.FailureReason);
            return;
        }

        if (visualSnapshot.HasValue)
        {
            ApplyCombatVisualFeedback(visualSnapshot.Value);
        }

        if (!string.IsNullOrWhiteSpace(result.HintMessage))
        {
            SetHint(result.HintMessage);
        }

        logMessage = !string.IsNullOrWhiteSpace(result.LogMessage)
            ? result.LogMessage
            : result.FailureReason;

        if (nextPhase.HasValue)
        {
            if (phase != nextPhase.Value)
            {
                ChangeFlowState(nextPhase.Value);
                return;
            }
        }

        if (result.RoomResolved)
        {
            RebuildArenaForCurrentRoom();
        }

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
}
