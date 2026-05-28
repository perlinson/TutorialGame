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
        SetMusicDuck(ModalMusicDuckReason, false);
        CloseAllPanels();
    }

    private void EnterSettingsState()
    {
        SetMusicDuck(ModalMusicDuckReason, true, 6f);
        OpenGameUiPanel(GameUiPanelId.MainMenuSettings, new MainMenuSettingsPanelData(this));
        SetStatus("已打开洞府设置");
    }

    private void EnterLoadState()
    {
        SetMusicDuck(ModalMusicDuckReason, true, 6f);
        selectedLoadSlotIndex = CultivationLocalSaveStore.GetPreferredLoadSlot();
        OpenGameUiPanel(GameUiPanelId.MainMenuLoad, new MainMenuLoadPanelData(this));
        SetStatus("已展开存档卷轴");
    }

    private void EnterCharacterCreateState()
    {
        SetMusicDuck(ModalMusicDuckReason, true, 6f);
        selectedCharacterSlotIndex = CultivationLocalSaveStore.GetPreferredNewGameSlot();
        selectedArchetypeIndex = Mathf.Clamp(CultivationLocalSaveStore.LoadSelectedArchetype(), 0, archetypes.Count - 1);
        if (string.IsNullOrWhiteSpace(GetPendingHeroName()) || MatchesAnyDefaultName(GetPendingHeroName()))
        {
            SetPendingHeroName(string.Empty);
        }

        OpenGameUiPanel(GameUiPanelId.MainMenuCharacterCreate, new MainMenuCharacterCreatePanelData(this));
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
        SetMusicDuck(ModalMusicDuckReason, false);
        uiStateMachine.Clear();
    }
}
