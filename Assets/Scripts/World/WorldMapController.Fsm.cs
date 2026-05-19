using QFramework;
using UnityEngine;

public sealed partial class WorldMapController
{
    private const string ModalMusicDuckReason = "WorldMapModal";

    private enum WorldMapPrimaryState
    {
        Map,
        SectResidence
    }

    private enum WorldMapModalState
    {
        None,
        Inventory,
        Workshop
    }

    private FSM<WorldMapPrimaryState> primaryStateMachine = new FSM<WorldMapPrimaryState>();
    private FSM<WorldMapModalState> modalStateMachine = new FSM<WorldMapModalState>();
    private bool stateMachinesConfigured;

    private void ResetUiStateMachines(WorldMapPrimaryState primaryState, WorldMapModalState modalState)
    {
        primaryStateMachine = new FSM<WorldMapPrimaryState>();
        modalStateMachine = new FSM<WorldMapModalState>();
        stateMachinesConfigured = false;
        ConfigureUiStateMachines();
        primaryStateMachine.StartState(primaryState);
        modalStateMachine.StartState(modalState);
    }

    private void ConfigureUiStateMachines()
    {
        if (stateMachinesConfigured)
        {
            return;
        }

        stateMachinesConfigured = true;

        primaryStateMachine.State(WorldMapPrimaryState.Map)
            .OnEnter(EnterMapState)
            .OnUpdate(UpdateMapState);

        primaryStateMachine.State(WorldMapPrimaryState.SectResidence)
            .OnEnter(EnterSectResidenceState)
            .OnUpdate(UpdateSectResidenceState);

        modalStateMachine.State(WorldMapModalState.None)
            .OnEnter(EnterNoModalState);

        modalStateMachine.State(WorldMapModalState.Inventory)
            .OnEnter(EnterInventoryState)
            .OnUpdate(UpdateModalState);

        modalStateMachine.State(WorldMapModalState.Workshop)
            .OnEnter(EnterWorkshopState)
            .OnUpdate(UpdateModalState);
    }

    private void ChangePrimaryState(WorldMapPrimaryState nextState)
    {
        if (primaryStateMachine.CurrentState == null)
        {
            ResetUiStateMachines(nextState, WorldMapModalState.None);
            return;
        }

        if (primaryStateMachine.CurrentStateId.Equals(nextState))
        {
            return;
        }

        primaryStateMachine.ChangeState(nextState);
    }

    private void ChangeModalState(WorldMapModalState nextState)
    {
        if (modalStateMachine.CurrentState == null)
        {
            ResetUiStateMachines(WorldMapPrimaryState.Map, nextState);
            return;
        }

        if (modalStateMachine.CurrentStateId.Equals(nextState))
        {
            return;
        }

        modalStateMachine.ChangeState(nextState);
    }

    private bool HasActiveModalState()
    {
        return modalStateMachine.CurrentState != null && !modalStateMachine.CurrentStateId.Equals(WorldMapModalState.None);
    }

    private void EnterMapState()
    {
        ShowMapScreen();
    }

    private void EnterSectResidenceState()
    {
        ShowSectScreen();
        ChangeModalState(WorldMapModalState.None);
        RefreshPanels();
    }

    private void EnterNoModalState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, false);
        modalPanels.HideAll();
    }

    private void EnterInventoryState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, true, 5f);
        modalPanels.Show(inventoryPanel);
        RefreshPanels();
    }

    private void EnterWorkshopState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, true, 5f);
        modalPanels.Show(workshopPanel);
        RefreshPanels();
    }

    private void UpdateMapState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMain();
        }
    }

    private void UpdateSectResidenceState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSect();
        }
    }

    private void UpdateModalState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeModalState(WorldMapModalState.None);
        }
    }

    protected override void OnBeforeDestroy()
    {
        base.OnBeforeDestroy();
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, false);
        primaryStateMachine.Clear();
        modalStateMachine.Clear();
    }
}
