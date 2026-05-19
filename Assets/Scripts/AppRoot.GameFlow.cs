using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GlobalGameFlowState
{
    Splash,
    MainMenu,
    WorldMap,
    Gameplay,
    Paused,
    GameOver
}

public sealed class GlobalGameFlowManager : MonoBehaviour
{
    private readonly FSM<GlobalGameFlowState> stateMachine = new FSM<GlobalGameFlowState>();

    private bool isInitialized;
    private bool startupSplashCompleted;
    private float splashDuration = 0.45f;
    private GlobalGameFlowState pauseResumeState = GlobalGameFlowState.Gameplay;

    public GlobalGameFlowState CurrentState => stateMachine.CurrentState != null ? stateMachine.CurrentStateId : GlobalGameFlowState.Splash;
    public bool IsPaused => CurrentState == GlobalGameFlowState.Paused;

    public void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        ConfigureStateMachine();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        isInitialized = true;
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void OnDestroy()
    {
        if (isInitialized)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        stateMachine.Clear();
    }

    public void BeginBootFlow(string defaultSceneName, float configuredSplashDuration)
    {
        splashDuration = Mathf.Max(0f, configuredSplashDuration);
        var targetSceneName = SceneFlow.ConsumePendingSceneNameOrDefault(defaultSceneName);

        if (!startupSplashCompleted)
        {
            SceneFlow.SetRuntimeTargetScene(targetSceneName);
            StartOrChangeState(GlobalGameFlowState.Splash);
            return;
        }

        RouteToScene(targetSceneName);
    }

    public void RouteToScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        SceneFlow.SetRuntimeTargetScene(sceneName);
        var state = SceneFlow.ResolveFlowState(sceneName);
        if (!state.HasValue)
        {
            SceneFlow.LoadSceneDirect(sceneName);
            return;
        }

        StartOrChangeState(state.Value);
    }

    public void SyncToScene(string sceneName)
    {
        var state = SceneFlow.ResolveFlowState(sceneName);
        if (!state.HasValue || state.Value == GlobalGameFlowState.Splash)
        {
            return;
        }

        StartOrChangeState(state.Value);
    }

    public bool EnterPaused()
    {
        if (CurrentState != GlobalGameFlowState.Gameplay)
        {
            return false;
        }

        pauseResumeState = CurrentState;
        StartOrChangeState(GlobalGameFlowState.Paused);
        return true;
    }

    public bool ResumeGameplay()
    {
        if (CurrentState != GlobalGameFlowState.Paused)
        {
            return false;
        }

        StartOrChangeState(pauseResumeState == GlobalGameFlowState.Paused ? GlobalGameFlowState.Gameplay : pauseResumeState);
        return true;
    }

    private void ConfigureStateMachine()
    {
        stateMachine.State(GlobalGameFlowState.Splash)
            .OnEnter(EnterSplashState)
            .OnUpdate(UpdateSplashState);

        stateMachine.State(GlobalGameFlowState.MainMenu)
            .OnEnter(() => LoadSceneIfNeeded(SceneFlow.MainMenuSceneName));

        stateMachine.State(GlobalGameFlowState.WorldMap)
            .OnEnter(() => LoadSceneIfNeeded(SceneFlow.WorldMapSceneName));

        stateMachine.State(GlobalGameFlowState.Gameplay)
            .OnEnter(() => LoadSceneIfNeeded(SceneFlow.GameplaySceneName));

        stateMachine.State(GlobalGameFlowState.Paused)
            .OnEnter(() => Time.timeScale = 0f)
            .OnExit(() => Time.timeScale = 1f);

        stateMachine.State(GlobalGameFlowState.GameOver);
    }

    private void StartOrChangeState(GlobalGameFlowState nextState)
    {
        if (stateMachine.CurrentState == null)
        {
            stateMachine.StartState(nextState);
            return;
        }

        if (stateMachine.CurrentStateId.Equals(nextState))
        {
            return;
        }

        stateMachine.ChangeState(nextState);
    }

    private void EnterSplashState()
    {
        Time.timeScale = 1f;
    }

    private void UpdateSplashState()
    {
        if (stateMachine.SecondsOfCurrentState < splashDuration)
        {
            return;
        }

        startupSplashCompleted = true;
        var targetSceneName = SceneFlow.ConsumeRuntimeTargetSceneOrDefault(SceneFlow.MainMenuSceneName);
        RouteToScene(targetSceneName);
    }

    private static void LoadSceneIfNeeded(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneFlow.LoadSceneDirect(sceneName);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneFlow.BootSceneName)
        {
            return;
        }

        SyncToScene(scene.name);
    }
}
