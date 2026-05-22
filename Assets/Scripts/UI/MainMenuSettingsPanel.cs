using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class MainMenuSettingsPanel : CultivationUIPanel
{
    private static readonly Vector2 WindowDesignSize = new Vector2(1380f, 760f);

    public Button blockerButton;
    public Button musicVolumeDownButton;
    public Button musicVolumeUpButton;
    public Button sfxVolumeDownButton;
    public Button sfxVolumeUpButton;
    public Button voiceVolumeDownButton;
    public Button voiceVolumeUpButton;
    public Button fullscreenToggleButton;
    public Button resetSettingsButton;
    public Button closeButton;

    public Text musicVolumeValueText;
    public Text sfxVolumeValueText;
    public Text voiceVolumeValueText;
    public Text fullscreenValueText;

    public RectTransform windowRect;

    private MainMenuController owner;
    private RectTransform rootRect;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;
    private bool buttonsBound;

    protected override void OnOpen(IUIData uiData = null)
    {
        owner = (uiData as MainMenuSettingsPanelData)?.Owner;
        rootRect = transform as RectTransform;
        EnsureBindings();
        RefreshFromOwner();
        RefreshResponsiveLayout(true);
        UiPanelOrderUtility.BringToFront(this, 220);
    }

    protected override void OnClose()
    {
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
    }

    private void EnsureBindings()
    {
        if (buttonsBound)
        {
            return;
        }

        BindButton(blockerButton, ClosePanel, CultivationButtonSound.Cancel);
        BindButton(musicVolumeDownButton, () => AdjustAndRefresh(() => owner?.ChangeMusicVolume(-0.1f)));
        BindButton(musicVolumeUpButton, () => AdjustAndRefresh(() => owner?.ChangeMusicVolume(0.1f)));
        BindButton(sfxVolumeDownButton, () => AdjustAndRefresh(() => owner?.ChangeSfxVolume(-0.1f)));
        BindButton(sfxVolumeUpButton, () => AdjustAndRefresh(() => owner?.ChangeSfxVolume(0.1f)));
        BindButton(voiceVolumeDownButton, () => AdjustAndRefresh(() => owner?.ChangeVoiceVolume(-0.1f)));
        BindButton(voiceVolumeUpButton, () => AdjustAndRefresh(() => owner?.ChangeVoiceVolume(0.1f)));
        BindButton(fullscreenToggleButton, () => AdjustAndRefresh(() => owner?.ToggleFullscreenMode()));
        BindButton(resetSettingsButton, () => AdjustAndRefresh(() => owner?.ResetSettings()));
        BindButton(closeButton, ClosePanel, CultivationButtonSound.Cancel);
        buttonsBound = true;
    }

    private void RefreshFromOwner()
    {
        if (owner == null)
        {
            return;
        }

        var snapshot = owner.BuildSettingsSnapshot();
        if (musicVolumeValueText != null) musicVolumeValueText.text = Mathf.RoundToInt(snapshot.MusicVolume * 100f) + "%";
        if (sfxVolumeValueText != null) sfxVolumeValueText.text = Mathf.RoundToInt(snapshot.SfxVolume * 100f) + "%";
        if (voiceVolumeValueText != null) voiceVolumeValueText.text = Mathf.RoundToInt(snapshot.VoiceVolume * 100f) + "%";
        if (fullscreenValueText != null) fullscreenValueText.text = snapshot.IsFullscreen ? "全屏" : "窗口";
    }

    private void AdjustAndRefresh(System.Action action)
    {
        action?.Invoke();
        RefreshFromOwner();
    }

    private void ClosePanel()
    {
        owner?.CloseSettings();
    }

    private void RefreshResponsiveLayout(bool force)
    {
        if (rootRect == null || windowRect == null)
        {
            return;
        }

        var rect = rootRect.rect;
        if (rect.width < 1f || rect.height < 1f)
        {
            return;
        }

        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        if (!force && width == lastLayoutWidth && height == lastLayoutHeight)
        {
            return;
        }

        lastLayoutWidth = width;
        lastLayoutHeight = height;
        windowRect.sizeDelta = WindowDesignSize;
        var scale = Mathf.Min(1f, rect.width * 0.92f / WindowDesignSize.x, rect.height * 0.9f / WindowDesignSize.y);
        windowRect.localScale = new Vector3(scale, scale, 1f);
    }

}
