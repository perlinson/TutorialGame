using QFramework;
using UnityEngine;

public sealed partial class MainMenuController
{
    private const string ModalMusicDuckReason = "MainMenuModal";

    private enum MainMenuUiState
    {
        Home,
        Settings,
        Load,
        CharacterCreate
    }

    private FSM<MainMenuUiState> uiStateMachine = new FSM<MainMenuUiState>();
    private bool uiStateMachineConfigured;

    private void ResetUiStateMachine(MainMenuUiState initialState)
    {
        uiStateMachine = new FSM<MainMenuUiState>();
        uiStateMachineConfigured = false;
        ConfigureUiStateMachine();
        uiStateMachine.StartState(initialState);
    }

    private void ConfigureUiStateMachine()
    {
        if (uiStateMachineConfigured)
        {
            return;
        }

        uiStateMachineConfigured = true;

        uiStateMachine.State(MainMenuUiState.Home)
            .OnEnter(EnterHomeState)
            .OnUpdate(UpdateHomeState);

        uiStateMachine.State(MainMenuUiState.Settings)
            .OnEnter(EnterSettingsState)
            .OnUpdate(UpdateModalState);

        uiStateMachine.State(MainMenuUiState.Load)
            .OnEnter(EnterLoadState)
            .OnUpdate(UpdateModalState);

        uiStateMachine.State(MainMenuUiState.CharacterCreate)
            .OnEnter(EnterCharacterCreateState)
            .OnUpdate(UpdateModalState);
    }

    private void ChangeUiState(MainMenuUiState nextState)
    {
        if (uiStateMachine.CurrentState == null)
        {
            ResetUiStateMachine(nextState);
            return;
        }

        if (uiStateMachine.CurrentStateId.Equals(nextState))
        {
            return;
        }

        uiStateMachine.ChangeState(nextState);
    }

    private void EnterHomeState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, false);
        CloseAllPanels();
    }

    private void EnterSettingsState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, true, 6f);
        CultivationApp.OpenMainMenuSettingsPanel(this);
        SetStatus("已打开洞府设置");
    }

    private void EnterLoadState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, true, 6f);
        selectedLoadSlotIndex = MainMenuSaveStore.GetPreferredLoadSlot();
        CultivationApp.OpenMainMenuLoadPanel(this);
        SetStatus("已展开存档卷轴");
    }

    private void EnterCharacterCreateState()
    {
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, true, 6f);
        selectedCharacterSlotIndex = MainMenuSaveStore.GetPreferredNewGameSlot();
        selectedArchetypeIndex = Mathf.Clamp(MainMenuSaveStore.LoadSelectedArchetype(), 0, archetypes.Count - 1);
        if (string.IsNullOrWhiteSpace(GetPendingHeroName()) || MatchesAnyDefaultName(GetPendingHeroName()))
        {
            SetPendingHeroName(string.Empty);
        }

        CultivationApp.OpenMainMenuCharacterCreatePanel(this);
        SetStatus("选择一条修途与存档档位");
    }

    private void UpdateHomeState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPanels();
        }
    }

    private void UpdateModalState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeUiState(MainMenuUiState.Home);
        }
    }

    protected override void OnBeforeDestroy()
    {
        base.OnBeforeDestroy();
        CultivationApp.SetMusicDuck(ModalMusicDuckReason, false);
        uiStateMachine.Clear();
    }
}
