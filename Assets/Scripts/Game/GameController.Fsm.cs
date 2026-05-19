using QFramework;
using UnityEngine;

public sealed partial class GameController
{
    private FSM<ExpeditionFlowPhase> flowStateMachine = new FSM<ExpeditionFlowPhase>();
    private bool flowStateMachineConfigured;
    private bool suppressFlowStateEnterActions;

    private void ResetFlowStateMachine(ExpeditionFlowPhase initialPhase, bool runEnterActions)
    {
        flowStateMachine = new FSM<ExpeditionFlowPhase>();
        flowStateMachineConfigured = false;
        ConfigureFlowStateMachine();

        phase = initialPhase;
        suppressFlowStateEnterActions = !runEnterActions;
        flowStateMachine.StartState(initialPhase);
        suppressFlowStateEnterActions = false;
    }

    private void ConfigureFlowStateMachine()
    {
        if (flowStateMachineConfigured)
        {
            return;
        }

        flowStateMachineConfigured = true;
        flowStateMachine.OnStateChanged((_, nextState) => phase = nextState);

        flowStateMachine.State(ExpeditionFlowPhase.RoomDecision)
            .OnEnter(EnterRoomDecisionState)
            .OnUpdate(UpdateRoomDecisionState);

        flowStateMachine.State(ExpeditionFlowPhase.CombatPlayerTurn)
            .OnEnter(EnterCombatPlayerTurnState)
            .OnUpdate(UpdateCombatPlayerTurnState);

        flowStateMachine.State(ExpeditionFlowPhase.AfterRoom)
            .OnEnter(EnterAfterRoomState)
            .OnUpdate(UpdateAfterRoomState);

        flowStateMachine.State(ExpeditionFlowPhase.Completed)
            .OnEnter(EnterCompletedState)
            .OnUpdate(UpdateTerminalFlowState);

        flowStateMachine.State(ExpeditionFlowPhase.Retreated)
            .OnEnter(EnterRetreatedState)
            .OnUpdate(UpdateTerminalFlowState);

        flowStateMachine.State(ExpeditionFlowPhase.Failed)
            .OnEnter(EnterFailedState)
            .OnUpdate(UpdateTerminalFlowState);
    }

    private void ChangeFlowState(ExpeditionFlowPhase nextPhase)
    {
        if (flowStateMachine.CurrentState == null)
        {
            ResetFlowStateMachine(nextPhase, true);
            return;
        }

        if (phase == nextPhase)
        {
            return;
        }

        flowStateMachine.ChangeState(nextPhase);
    }

    private void EnterRoomDecisionState()
    {
        if (suppressFlowStateEnterActions)
        {
            return;
        }

        RebuildArenaForCurrentRoom();
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void EnterCombatPlayerTurnState()
    {
        if (suppressFlowStateEnterActions)
        {
            return;
        }

        RebuildArenaForCurrentRoom();
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void EnterAfterRoomState()
    {
        if (suppressFlowStateEnterActions)
        {
            return;
        }

        RebuildArenaForCurrentRoom();
        SyncExpeditionRuntime();
        RefreshView();
    }

    private void EnterCompletedState()
    {
        if (suppressFlowStateEnterActions)
        {
            return;
        }

        RefreshView();
    }

    private void EnterRetreatedState()
    {
        if (suppressFlowStateEnterActions)
        {
            return;
        }

        RefreshView();
    }

    private void EnterFailedState()
    {
        if (suppressFlowStateEnterActions)
        {
            return;
        }

        RefreshView();
    }

    private void UpdateRoomDecisionState()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceOrSearch();
            return;
        }
    }

    private void UpdateCombatPlayerTurnState()
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

    private void UpdateAfterRoomState()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceToNextPhase();
            return;
        }
    }

    private void UpdateTerminalFlowState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToWorldMap();
        }
    }

    private void FixedUpdate()
    {
        flowStateMachine.FixedUpdate();
    }

    private void OnDestroy()
    {
        flowStateMachine.Clear();
    }
}
